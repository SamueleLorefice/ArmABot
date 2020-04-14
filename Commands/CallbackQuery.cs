using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArmABot;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot.Commands {
	public class CallbackQuery : ICallback{
        DBManager database;
        TelegramBotClient botClient;

        public void Setup(DBManager database, TelegramBotClient botClient) {
            this.database = database;
            this.botClient = botClient;
        }

        public void OnCallback(object sender, CallbackQueryEventArgs e) {
            var senderId = e.CallbackQuery.From.Id;
            Program.DecodeInlineQuery(e.CallbackQuery.Data, out EVote choice, out var chatId, out var pollId);
            Poll poll = database.GetPoll(pollId);
            var votes = database.GetVotesInPollFrom(senderId, pollId).ToList();
            if (poll.EventDate < DateTime.Now) {
                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Questo poll è chiuso");
                botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, Program.GetText(pollId), replyMarkup: null, parseMode: ParseMode.Html);
            } else {
                if (votes.Count == 1) {
                    var id = votes[0].Id;
                    database.EditVote(id, choice);
                    botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, Program.GetText(pollId), replyMarkup: Program.GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
                    botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Il tuo voto è stato modificato");
                } else if (votes.Count == 0 || votes == null) {
                    database.AddVote(choice, pollId, senderId, e.CallbackQuery.From.FirstName);
                    botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, Program.GetText(pollId), replyMarkup: Program.GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
                    botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Il tuo voto è stato aggiunto");
                } else {
                    Console.WriteLine("DATABASE ERROR. Run with debug build to see more informations...");
#if DEBUG
                    Console.WriteLine("ERROR: Something is wrong on the DB! There are 2 or more votes from the same user in a poll!");
                    Console.WriteLine(string.Format(" PollID: {0}\n UserID: {1}", pollId, senderId));
#endif
                }
            }
        }
    }
}
