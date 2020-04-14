using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ArmABot;
using ArmABot.DBTables;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot.Commands {
	public class AddEvent : ICommand {
		DBManager database;
		TelegramBotClient botClient;

		public void OnMessage(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) return;
            //DATA Groups: 0 = Original message, 1 = Text, 2 = date, 3 = time, 4 = quota
            if (Regex.IsMatch(e.Message.Text, @"\/addevent[\S]* '([\s\S]*)' ([0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4}) ([0-9]{4}) \+([0-9]*)", RegexOptions.IgnoreCase)) {
                var data = Regex.Match(e.Message.Text, @"\/addevent[\S]* '([\s\S]*)' ([0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4}) ([0-9]{4}) \+([0-9]*)", RegexOptions.IgnoreCase);
                Admin admin = database.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);//checks if the user that has sent the command is an admin of that group
                int pollId;
                if (e.Message.Chat.Type != ChatType.Private) {
                    if (admin != null && e.Message.Chat.Id == admin.GroupId) {
                        var poll = new Poll() {
                            UserId = admin.UserId,
                            GroupId = admin.GroupId,
                            Title = data.Groups[1].Value,
                            EventDate = ParseDate(data.Groups[2].Value, data.Groups[3].Value),
                            EventQuota = int.Parse(data.Groups[4].Value)
                        };
                        System.Threading.Tasks.Task<Message> MsgId = botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Loading Poll...");
                        poll.MessageId = MsgId.Result.MessageId;
                        pollId = database.AddPoll(poll);
                        InlineKeyboardMarkup markup = Program.GetReplyMarkUp(e.Message.Chat.Id, pollId);
                        botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, Program.GetText(pollId), replyMarkup: markup, parseMode: ParseMode.Html);
#if DEBUG
                        var debugPoll = new Poll() {
                            UserId = e.Message.From.Id,
                            GroupId = e.Message.Chat.Id,
                            Title = data.Groups[1].Value,
                            EventDate = ParseDate(data.Groups[2].Value, data.Groups[3].Value),
                            EventQuota = int.Parse(data.Groups[4].Value)
                        };
                        botClient.SendTextMessageAsync(
                            new ChatId(e.Message.Chat.Id),
                            string.Format("TITLE {0}\nUserID {1}\nGroupID {2}\nDATETIME {3}\nQUOTA {4}", debugPoll.Title, debugPoll.UserId, debugPoll.GroupId, debugPoll.EventDate, debugPoll.EventQuota));
#endif
                    }
                } else {
                    botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Can't create event polls in private chats.");
                }
            } else if (e.Message.Text.ToLower().StartsWith("/addevent")) {
                botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Wrong syntax. use: /addevent '<title>' <date> <time> +<minium number of partecipants>");
            }
        }

		public void Setup(DBManager database, TelegramBotClient botClient) {
			this.database = database;
			this.botClient = botClient;
		}

        private DateTime ParseDate(string date, string time) {
            var parsedDate = date.Split('/');
            var parsedTime = new string[2] {
                time[0].ToString() + time[1].ToString(),
                time[2].ToString() + time[3].ToString()
            };
            var dateTime = new DateTime(
                int.Parse(parsedDate[2]),
                int.Parse(parsedDate[1]),
                int.Parse(parsedDate[0]),
                int.Parse(parsedTime[0]),
                int.Parse(parsedTime[1]),
                0
            );
            return dateTime;
        }


    }
}
