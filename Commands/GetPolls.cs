using ArmABot;
using ArmABot.DBTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot.Commands {
	public class GetPolls : ICommand {

		DBManager database;
		TelegramBotClient botClient;

		public void Setup(DBManager database, TelegramBotClient botClient) {
			this.database = database;
			this.botClient = botClient;
		}

		public void OnMessage(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            if (e.Message.Text.ToLower().Contains("/polls")) {
                Admin admin = database.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);
                if (admin != null) {
                    Poll[] polls = database.GetPollsBy(admin.UserId, admin.GroupId).ToArray();
                    var Buttons = new List<List<KeyboardButton>>();
                    foreach (Poll poll in polls) {
                        Buttons.Add(new List<KeyboardButton>() { new KeyboardButton($"ID{poll.PollId} {poll.Title}") });
                    }
                    var markup = new ReplyKeyboardMarkup(Buttons, oneTimeKeyboard: true) {
                        Selective = true
                    };
                    botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Select a poll", replyToMessageId: e.Message.MessageId, replyMarkup: markup);
                }
            }
        }
	}
}
