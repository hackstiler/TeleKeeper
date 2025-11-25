using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using TeleKeeper.TelegramCore;
using Microsoft.Extensions.Configuration;
using TeleKeeper.Sessions;

namespace TeleKeeper;

class Program
{
    static long AdminId;

    static TelegramBot bot; //Telegram Bot
    static string Token;

    static string ApiId;
    static string ApiHash;

    static List<long> AwaitingPhoneNumber = new();
    static Dictionary<long, TelegramClient> AwaitingPassword = new();
    static Dictionary<long, TaskCompletionSource<string>> AwaitingVerificationCode = new();

    static Dictionary<string, TelegramClient> ActiveClients = new();

    public static Action<string, string> OnTryAuthorization;

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        AdminId = long.Parse(config["ADMIN_ID"]);
        Token = config["BOT_TOKEN"];
        ApiHash = config["APP_HASH"];
        ApiId = config["APP_ID"];

        bot = new TelegramBot(Token);
        bot.Start();
        bot.OnMessage += OnMessage;

        OnTryAuthorization += OnTryAutorization;
        await AutoAuthorization();

        await Task.Delay(-1);
    }

    private async static void OnMessage(ITelegramBotClient client, Telegram.Bot.Types.Update update)
    {
        long chatId;
        Telegram.Bot.Types.Message message;

        switch (update.Type)
        {
            //-----------------------------------------------------------------------------------------------------обработка сообщений
            case UpdateType.Message:
                chatId = update.Message.Chat.Id;
                message = update.Message;

                Console.WriteLine($"[{DateTime.Now}] {chatId}: {message?.Text}");

                if (AdminId != chatId)
                    return;

                // TELEGRAM CLIENT ЖДЁТ КОД
                if (AwaitingVerificationCode.TryGetValue(chatId, out var tcs))
                {
                    tcs.SetResult(message.Text);
                    AwaitingVerificationCode.Remove(chatId);
                    return;
                }

                // ЖДЁМ ТЕЛЕФОН
                if (AwaitingPhoneNumber.Contains(chatId))
                {
                    AwaitingPhoneNumber.Remove(chatId);

                    if (SessionManager.ActiveSessions.Contains(message.Text))
                    {
                        await client.SendMessage(chatId, "Данная сессия уже запущена");
                        await client.SendMessage(chatId,
                                                 "Выберите дальнейшее действие:",
                                                 replyMarkup: KeyboardService.GetInlineKeyboardMarkup("Menu"));
                        return;
                    }

                    var tg = new TelegramClient(ApiId, ApiHash, message.Text);

                    AwaitingPassword[chatId] = tg;

                    await client.SendMessage(chatId, "Введите 2FA пароль (если есть, иначе отправьте '-')");
                    return;
                }

                // ЖДЁМ 2FA ПАРОЛЬ
                if (AwaitingPassword.ContainsKey(chatId))
                {
                    var tgClient = AwaitingPassword[chatId];
                    tgClient.Password = message.Text == "-" ? "" : message.Text;

                    tgClient.RequestVerificationCode = async () =>
                    {
                        var source = new TaskCompletionSource<string>();
                        AwaitingVerificationCode[chatId] = source;

                        await client.SendMessage(chatId, "Введите код, отправленный в Telegram:");
                        return await source.Task;
                    };

                    _ = Task.Run(async () =>
                    {
                        await tgClient.ConnectAsync(async msg =>
                        {
                            await client.SendMessage(chatId, msg);
                        });

                        if (tgClient.IsAuthorized)
                        {
                            await client.SendMessage(chatId, "✅Аккаунт успешно подключён!");
                            SessionManager.AddSession(tgClient.PhoneNumber);

                            ActiveClients.Add(tgClient.PhoneNumber, tgClient);

                            await client.SendMessage(chatId,
                                                 "Выберите дальнейшее действие:",
                                                 replyMarkup: KeyboardService.GetInlineKeyboardMarkup("Menu"));
                        }
                    });

                    AwaitingPassword.Remove(chatId);
                    return;
                }

                switch (message.Text)
                {
                    case "/start":

                        await client.SendMessage(chatId,
                                                 "Выберите дальнейшее действие:",
                                                 replyMarkup: KeyboardService.GetInlineKeyboardMarkup("Menu"));



                        return;
                }

                return;

            //-----------------------------------------------------------------------------------------------------обработка нажатий inline кнопки
            case UpdateType.CallbackQuery:
                chatId = update.CallbackQuery.Message.Chat.Id;
                var callBackQuery = update.CallbackQuery;

                Console.WriteLine($"[{DateTime.Now}] {chatId}: {callBackQuery.Data}");

                if (AdminId != chatId)
                    return;

                switch (callBackQuery.Data)
                {
                    case "AddSession":

                        await client.SendMessage(chatId,
                                                 "Напишите номер телефона");

                        AwaitingPhoneNumber.Add(chatId);

                        return;

                    case "GetAllSessions":

                        await client.EditMessageText(chatId,
                                                     callBackQuery.Message.Id,
                                                     "Выберите интересующий вас аккаунт:",
                                                     replyMarkup: KeyboardService.GetInlineKeyboardMarkup("AllSessions"));

                        return;

                    case "Menu":

                        await client.EditMessageText(chatId,
                                                     callBackQuery.Message.Id,
                                                     "Выберите дальнейшее действие:",
                                                     replyMarkup: KeyboardService.GetInlineKeyboardMarkup("Menu"));

                        return;

                    case string data when data.StartsWith("Session_"):

                        string phoneNumber = data.Split("_")[1];

                        var info = await ActiveClients[phoneNumber].GetAccountInfoFormatted();

                        await client.EditMessageText(chatId,
                                                     callBackQuery.Message.Id,
                                                     info,
                                                     replyMarkup: KeyboardService.GetInlineKeyboardMarkup("Profile"));

                        return;


                }

                return;

        }
    }

    private static async Task AutoAuthorization()
    {
        foreach (var session in SessionManager.ActiveSessions)
        {
            var tgClient = new TelegramClient(ApiId, ApiHash, session);
            await tgClient.ConnectAsync();

            ActiveClients.Add(tgClient.PhoneNumber, tgClient);
        }
    }

    private static async void OnTryAutorization(string sessionNumber, string msg)
    {
        await bot.Client.SendMessage(AdminId, $"⚠️Попытка входа на {sessionNumber}:");
        await bot.Client.SendMessage(AdminId, msg);
    }
}
