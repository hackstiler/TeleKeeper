using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTelegram;
using TL;

namespace TeleKeeper.TelegramCore
{
    internal class TelegramClient : IDisposable
    {
        private Client _client;

        public Func<Task<string>> RequestVerificationCode { get; set; }
        //public Func<Task<string>> RequestPassword { get; set; }

        public string ApiId { get; }
        public string ApiHash { get; }
        public string PhoneNumber { get; }
        public string Password { get; set; }

        public bool IsAuthorized { get; private set; }


        public TelegramClient(string apiId, string apiHash, string phoneNumber)
        {
            ApiId = apiId;
            ApiHash = apiHash;
            PhoneNumber = phoneNumber;
            _client = new Client(Config);

            _client.OnUpdates += Update;

        }

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return ApiId;
                case "api_hash": return ApiHash;
                case "phone_number": return PhoneNumber;
                case "session_pathname": return $"Sessions/{PhoneNumber}.session";
                case "password": return Password; //RequestPassword?.Invoke().GetAwaiter().GetResult();
                case "verification_code": return RequestVerificationCode?.Invoke().GetAwaiter().GetResult();

                default: return null;
            }
        }

        public async Task ConnectAsync(Func<string, Task> onError = null)
        {
            string sessionPath = $"Sessions/{PhoneNumber}.session";

            if (!File.Exists(sessionPath))
                File.Create(sessionPath).Dispose();

            try
            {
                await _client.LoginUserIfNeeded();

                IsAuthorized = true;
            }
            catch (WTelegram.WTException ex)
            {
                string msg;
                if (ex.Message.Contains("CODE_INVALID"))
                    msg = "Неверный код подтверждения. Вход отменён.";
                else if (ex.Message.Contains("PASSWORD_HASH_INVALID"))
                    msg = "Неверный пароль 2FA. Вход отменён.";
                else
                    msg = $"Ошибка входа: {ex.Message}";

                _client.Dispose();

                if (onError != null)
                    await onError(msg);

                return;
            }
        }

        private async Task Update(UpdatesBase update)
        {
            foreach (var upd in update.UpdateList)
            {
                switch (upd)
                {

                    case UpdateNewMessage unm:

                        if (unm.message is Message msg &&
                            msg.peer_id is PeerUser pu)
                        {
                            long fromUserId = pu.user_id; // кто отправил

                            long targetId = 777000;   // нужный контакт

                            if (fromUserId == targetId)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Сообщение от {fromUserId}: {msg.message}");
                                Console.ResetColor();

                                Program.OnTryAuthorization.Invoke(PhoneNumber, msg.message);

                                // тут можешь вызвать любое событие
                            }
                        }

                        return;

                    case UpdateNewAuthorization auth:

                        Console.WriteLine("Получена новая авторизация.");

                        // Получаем подробности
                        Console.WriteLine($"IP: {auth.flags}");
                        Console.WriteLine($"Страна: {auth.location}");
                        Console.WriteLine($"Модель устройства: {auth.device}");
                        Console.WriteLine($"Приложение: {auth.hash}");

                        // Отправляем сообщение пользователю, если это важно
                        // Например, предупреждение о новой авторизации
                        //await client.SendMessageAsync(chatId, $"Новая авторизация с IP {auth.ip}, страна: {auth.country}");


                        return;
                }
            }
        }

        public async Task<string> GetAccountInfoFormatted()
        {
            // Получаем информацию о текущем пользователе
            var me = _client.User;

            // Получаем имя и фамилию
            string fullName = $"{me.first_name} {me.last_name}".Trim();

            // Получаем тег (username)
            string username = me.username ?? "Не указан";

            // Дата последнего входа
            string lastLogin = "Неизвестно";
            if (me.status is UserStatusOffline offline)
            {
                DateTime lastSeen = offline.was_online;
                lastLogin = lastSeen.AddHours(3).ToString("dd.MM.yyyy | HH:mm:ss");
            }
            else if (me.status is UserStatusOnline)
            {
                lastLogin = "В сети";
            }

            // Статус аккаунта
            string accountStatus = "Активен";
            if (me.flags.HasFlag(User.Flags.deleted))
            {
                accountStatus = "Заблокирован";
            }
            else if (me.status is UserStatusOffline)
            {
                accountStatus = "Оффлайн";
            }
            // Формируем строку с информацией
            string accountInfo = $"Имя: {fullName}\n" +
                                 $"Тег: @{username}\n" +
                                 $"Статус аккаунта: {accountStatus}\n" +
                                 $"Дата последнего появления в сети: {lastLogin} (МСК)";

            return accountInfo;
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }

    }
}
