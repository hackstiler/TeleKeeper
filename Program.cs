using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using TeleKeeper.TelegramCore;
using Microsoft.Extensions.Configuration;

namespace TeleKeeper;

class Program
{
    static TelegramBot bot; //Telegram Bot

    static string Token;

    static string ApiId;
    static string ApiHash;

    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();


        Token = config["BOT_TOKEN"];

        bot = new TelegramBot(Token);
        bot.Start();
        bot.OnMessage += OnMessage;

        await Task.Delay(-1);
    }

    private async static void OnMessage(ITelegramBotClient client, Telegram.Bot.Types.Update update)
    {
        switch (update.Type)
        {
            //-----------------------------------------------------------------------------------------------------обработка сообщений
            case UpdateType.Message:
                var chatId = update.Message.Chat.Id;
                var message = update.Message;

                Console.WriteLine($"[{DateTime.Now}] {chatId}: {message?.Text}");

                switch (message.Text)
                {
                    case "/start":

                        Console.WriteLine("");

                        return;
                }

                return;

            //-----------------------------------------------------------------------------------------------------обработка нажатий inline кнопки
            case UpdateType.CallbackQuery:
                var callbackChatId = update.CallbackQuery.Message.Chat.Id;
                var callBackQuery = update.CallbackQuery;

                Console.WriteLine($"[{DateTime.Now}] {callbackChatId}: {callBackQuery.Data}");

                switch (callBackQuery.Data)
                {

                }

                return;

        }

    }
}
