using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TeleKeeper.TelegramCore
{
    internal class TelegramBot
    {
        public Action<ITelegramBotClient, Update> OnMessage;

        public TelegramBotClient Client { get => _bot; }

        private TelegramBotClient _bot;
        private string _paymentToken;

        public string PaymentToken
        {
            get { return _paymentToken; }
            set { _paymentToken = value; }
        }

        public TelegramBot(string token)
        {
            _bot = new TelegramBotClient(token);

        }

        public void Start()
        {
            _bot.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Bot started");
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            OnMessage?.Invoke(client, update);
            await Task.CompletedTask;
        }

        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine("Error: " + exception.Message);
            await Task.CompletedTask;
        }
    }
}
