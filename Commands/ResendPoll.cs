using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot.Commands {

	public class ResendPoll : ICommand {
		private DBManager database;
		private TelegramBotClient botClient;

		public void Setup(DBManager database, TelegramBotClient botClient) {
			this.database = database;
			this.botClient = botClient;
		}

		public void OnMessage(object sender, MessageEventArgs e) {
			if (e.Message.Text == null) {
				return;
			}
			if (e.Message.Text.Contains("ID")) {
				var id = int.Parse(Regex.Match(e.Message.Text, @"ID([0-9]+)").Groups[1].Value);
				System.Threading.Tasks.Task<Message> rmId = botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Loading Poll.", replyMarkup: new ReplyKeyboardRemove());
				botClient.DeleteMessageAsync(new ChatId(e.Message.Chat.Id), rmId.Result.MessageId);
				System.Threading.Tasks.Task<Message> MsgId = botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Loading Poll...");
				try {
					database.UpdatePollMessageId(id, MsgId.Result.MessageId);
					Poll poll = database.GetPoll(id);
					InlineKeyboardMarkup markup = Program.GetReplyMarkUp(e.Message.Chat.Id, poll.Id);
					botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), (int)poll.MessageId, Program.GetText(id), replyMarkup: markup, parseMode: ParseMode.Html);
				} catch (NullReferenceException exc) {
					botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, $"Can't load the poll, reason:\n{exc.Message}");
				}
			}
		}
	}
}