using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TeleKeeper.Sessions;

namespace TeleKeeper.TelegramCore
{
    internal class KeyboardService
    {
        public static InlineKeyboardMarkup GetInlineKeyboardMarkup(in string key, params object[] objects)
        {
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

            switch (key)
            {
                case "Menu":

                    keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // 1 row
                        {
                            InlineKeyboardButton.WithCallbackData("Добавить профиль","AddSession"),
                        },
                        new [] // 2 row
                        {
                            InlineKeyboardButton.WithCallbackData("Список аккаунтов","GetAllSessions"),
                        }
                    });


                    break;

                case "AllSessions":

                    string[] sessions = SessionManager.ActiveSessions.ToArray();
                    List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

                    for (int i = 0; i < sessions.Length; i++)
                    {
                        List<InlineKeyboardButton> btns = new List<InlineKeyboardButton>(); //коллекция кнопок для buttons
                        InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData($"{sessions[i]}", $"Session_{sessions[i]}"); //кнокпка, которая будет добавляться в btns

                        btns.Add(button);
                        buttons.Add(btns);
                    }

                    InlineKeyboardButton returnBtn = InlineKeyboardButton.WithCallbackData("🔙Назад", "Menu");
                    List<InlineKeyboardButton> btnss = new List<InlineKeyboardButton>();

                    btnss.Add(returnBtn);
                    buttons.Add(btnss);

                    keyboard = buttons;

                    break;


                case "Profile":

                    string session = objects[0].ToString();

                    keyboard = new InlineKeyboardMarkup(new[]
                    {

                        new [] // 1 row
                        {

                            InlineKeyboardButton.WithCallbackData("Удалить аккаунт", $"DeleteSession_{session}"),
                        },
                        new [] // 2 row
                        {

                            InlineKeyboardButton.WithCallbackData("🔙Назад", "GetAllSessions"),
                        },
                    });

                    break;
            }
            return keyboard;
        }
    }
}
