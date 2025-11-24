using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTelegram;
using TL;

namespace TeleKeeper.TelegramCore
{
    internal class TelegramClient
    {
        private readonly Client _client;

        public TelegramClient(string apiId, string apiHash, string phoneNumber)
        {
            ApiId = apiId;
            ApiHash = apiHash;
            PhoneNumber = phoneNumber;
            _client = new Client(Config);

        }

        private string ApiId { get; }
        private string ApiHash { get; }
        private string PhoneNumber { get; }

        private string Config(string what)
        {
            switch (what)
            {
                case "api_id": return ApiId;
                case "api_hash": return ApiHash;
                case "phone_number": return PhoneNumber;
                case "verification_code":
                    Console.Write("Введите код из Telegram: ");
                    return Console.ReadLine();
                case "password":
                    Console.Write("Введите пароль 2FA (если включен): ");
                    return Console.ReadLine();
                default: return null;
            }
        }

        public async Task ConnectAsync()
        {
            await _client.LoginUserIfNeeded();
            _client.OnUpdates += Update;
            return;
        }

        private async Task Update(UpdatesBase update)
        {
            foreach (var upd in update.UpdateList)
            {
                switch (upd)
                {

                }


            }
        }
    }
}
