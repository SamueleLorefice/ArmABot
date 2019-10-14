using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ArmA_Bot.DBTables;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmA_Bot.DBTables.Poll;

namespace ArmA_Bot {//TODO add a timer system to notify peoples if an event quota is reached

    internal class Program {
        private static DBManager DBManager;
        private static TelegramBotClient telegramBot;
        public static string ConnectionString;

        private static void Main(string[] args) {
            string token = "0";
            if (args.Length < 2) {
                Console.WriteLine("Missing arguments.");
                Console.WriteLine("BotToken ConnectionString");
                Console.WriteLine("Trying enviroment variables");
                var env = (Dictionary<string, string>)Environment.GetEnvironmentVariables();
                foreach (var keyPair in env) {
                    switch (keyPair.Key) {
                        case "BotToken":
                            token = keyPair.Value;
                            break;
                        case "DBAddress":
                            ConnectionString = keyPair.Value;
                            break;
                        default:
                            break;
                    }
                }
                if (token == "0" || ConnectionString == null) {
                    Console.WriteLine("Please start the bot using command line arguments <Token> <ConnectionString> or set up \"BotToken\" and \"DBAddress\" enviroment variables.");
                    env.Add("BotToken", "0");
                    env.Add("DBAddress", "");
                    return;
                }
            } else {
                token = args[0];
                ConnectionString = args[1];
            }
            Console.WriteLine("ArmA Helper Bot V{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Initializing Database Manager...");
            DBManager = new DBManager();
            //TODO: handle an eventual down database situation
            DBManager.TestConnection();
            Console.WriteLine("Initializing bot...");
            telegramBot = new TelegramBotClient(token);
            Console.WriteLine("Registering Callbacks...");
            telegramBot.OnMessage += AddEventHandler;
            telegramBot.OnMessage += AddAdminHandler;
            telegramBot.OnMessage += GetPollsHandler;
            telegramBot.OnMessage += ResendPollHandler;
            telegramBot.OnCallbackQuery += CallbackQueryHandler;
            Console.WriteLine("Starting Bot...");
            telegramBot.StartReceiving(new UpdateType[] { UpdateType.CallbackQuery, UpdateType.Message});
            Console.WriteLine("All fine, bot running...");
            Thread.Sleep(Timeout.Infinite);
        }

        private static void CallbackQueryHandler(object sender, CallbackQueryEventArgs e) {
            var senderId = e.CallbackQuery.From.Id;
            DecodeInlineQuery(e.CallbackQuery.Data, out EVote choice, out var chatId, out var pollId);
            Poll poll = DBManager.GetPoll(pollId);
            var votes = DBManager.GetVotesInPollFrom(senderId, pollId).ToList();
            if (poll.EventDate < DateTime.Now) {
                telegramBot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Questo poll è chiuso");
            } else {
                if (votes.Count == 1) {
                    var id = votes[0].Id;
                    DBManager.EditVote(id, choice);
                    telegramBot.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, GetText(pollId), replyMarkup: GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
                    telegramBot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Il tuo voto è stato modificato");
                } else if (votes.Count == 0 || votes == null) {
                    DBManager.AddVote(choice, pollId, senderId, e.CallbackQuery.From.FirstName);
                    telegramBot.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, GetText(pollId), replyMarkup: GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
                    telegramBot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Il tuo voto è stato aggiunto");
                } else {
                    Console.WriteLine("ERROR: Something is wrong on the DB! There are 2 or more votes from the same user in a poll!");
                    Console.WriteLine(string.Format(" PollID: {0}\n UserID: {1}", pollId, senderId));
                }
            }
        }

        private static void AddAdminHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            if (e.Message.Text.ToLower().Contains("/addadmin")) {
                Admin checkAdm = DBManager.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);
                if (checkAdm == null) {
                    DBManager.AddAdmin(new Admin { UserId = e.Message.From.Id, GroupId = e.Message.Chat.Id });
                    telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Added as admin of this chat");
                } else {
                    telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "You are already an admin of this chat");
                }
            }
        }

        private static void AddEventHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            //DATA Groups: 0 = Original message, 1 = Text, 2 = date, 3 = time, 4 = quota
            if (Regex.IsMatch(e.Message.Text, @"\/addevent '([\s\S]*)' ([0-9]{2}\/[0-9]{2}\/[0-9]{4}) ([0-9]{4}) \+([0-9]*)", RegexOptions.IgnoreCase)) {
                var data = Regex.Match(e.Message.Text, @"\/addevent '([\s\S]*)' ([0-9]{2}\/[0-9]{2}\/[0-9]{4}) ([0-9]{4}) \+([0-9]*)", RegexOptions.IgnoreCase);
                //checks if the user that has sent the command is an admin of that group
                Admin admin = DBManager.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);//SUGGESTION refactor and remove uLong in favor of Long and Int?
                int pollId;
                if (admin != null && e.Message.Chat.Id == admin.GroupId) {
                    var poll = new Poll() {
                        UserId = admin.UserId,
                        GroupId = admin.GroupId,
                        Title = data.Groups[1].Value,
                        EventDate = ParseDate(data.Groups[2].Value, data.Groups[3].Value),
                        EventQuota = int.Parse(data.Groups[4].Value)
                    };
                    System.Threading.Tasks.Task<Message> MsgId = telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Loading Poll...");
                    poll.MessageId = MsgId.Result.MessageId;
                    pollId = DBManager.AddPoll(poll);
                    InlineKeyboardMarkup markup = GetReplyMarkUp(e.Message.Chat.Id, pollId);
                    telegramBot.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, GetText(pollId), replyMarkup: markup, parseMode: ParseMode.Html);
#if DEBUG
                    var debugPoll = new Poll() {
                        UserId = e.Message.From.Id,
                        GroupId = e.Message.Chat.Id,
                        Title = data.Groups[1].Value,
                        EventDate = ParseDate(data.Groups[2].Value, data.Groups[3].Value),
                        EventQuota = int.Parse(data.Groups[4].Value)
                    };
                    telegramBot.SendTextMessageAsync(
                        new ChatId(e.Message.Chat.Id),
                        string.Format("TITLE {0}\nUserID {1}\nGroupID {2}\nDATETIME {3}\nQUOTA {4}", debugPoll.Title, debugPoll.UserId, debugPoll.GroupId, debugPoll.EventDate, debugPoll.EventQuota));
#endif
                }
            }
        }

        private static void GetPollsHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            if (e.Message.Text.ToLower().Contains("/polls")) {
                Admin admin = DBManager.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);
                if (admin != null) {
                    Poll[] polls = DBManager.GetPollsBy(admin.UserId, admin.GroupId).ToArray();
                    var Buttons = new List<List<KeyboardButton>>();
                    foreach (Poll poll in polls) {
                        Buttons.Add(new List<KeyboardButton>() { new KeyboardButton($"ID{poll.PollId} {poll.Title}") });
                    }
                    var markup = new ReplyKeyboardMarkup(Buttons, oneTimeKeyboard: true) {
                        Selective = true
                    };
                    telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Select a poll", replyToMessageId: e.Message.MessageId, replyMarkup: markup);
                }
            }
        }

        private static void ResendPollHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            if (e.Message.Text.Contains("ID")) {
                var id = int.Parse(Regex.Match(e.Message.Text, @"ID([0-9]+)").Groups[1].Value);
                System.Threading.Tasks.Task<Message> rmId = telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Ignore.", replyMarkup: new ReplyKeyboardRemove());
                telegramBot.DeleteMessageAsync(new ChatId(e.Message.Chat.Id), rmId.Result.MessageId);
                System.Threading.Tasks.Task<Message> MsgId = telegramBot.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Loading Poll...");
                try {
                    DBManager.UpdatePollMessageId(id, MsgId.Result.MessageId);
                    Poll poll = DBManager.GetPoll(id);
                    InlineKeyboardMarkup markup = GetReplyMarkUp(e.Message.Chat.Id, poll.PollId);
                    telegramBot.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), (int)poll.MessageId, GetText(id), replyMarkup: markup, parseMode: ParseMode.Html);
                } catch (NullReferenceException exc) {
                    telegramBot.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, $"Can't load the poll, reason:\n{exc.Message}");
                }
            }
        }

        private static string GetText(int pollId) {
            var text = "<b>📰";
            Poll poll = DBManager.GetPoll(pollId);
            IEnumerable<Vote> votes = DBManager.GetVotesInPoll(pollId);
            Vote[] Present = votes.Where(x => x.Choice == EVote.Present).ToArray();
            Vote[] Maybe = votes.Where(x => x.Choice == EVote.Maybe).ToArray();
            Vote[] Absent = votes.Where(x => x.Choice == EVote.Absent).ToArray();
            text += poll.Title + $"\n————————————————————\n{poll.EventDate.Day:D2}/{poll.EventDate.Month:D2}/{poll.EventDate.Year:D4} {poll.EventDate.Hour:D2}:{poll.EventDate.Minute:D2}\n\n✅Presenti: {Present.Length}</b>\n";

            foreach (Vote people in Present) {
                text += "    • " + people.Username + "\n";
            }
            text += $"<b>⚠️Forse: {Maybe.Length}</b>\n";
            foreach (Vote people in Maybe) {
                text += "    • " + people.Username + "\n";
            }
            text += $"<b>❌Assente: {Absent.Length}</b>\n";
            foreach (Vote people in Absent) {
                text += "    • " + people.Username + "\n";
            }
            return text;
        }

        private static InlineKeyboardMarkup GetReplyMarkUp(long chatId, int pollId) {
            var BtnPresente = new InlineKeyboardButton() { Text = "Presente", CallbackData = string.Format("1 {0} {1}", chatId, pollId) };
            var BtnForse = new InlineKeyboardButton() { Text = "Forse", CallbackData = string.Format("3 {0} {1}", chatId, pollId) };
            var BtnAssente = new InlineKeyboardButton() { Text = "Assente", CallbackData = string.Format("2 {0} {1}", chatId, pollId) };

            var RowPresente = new List<InlineKeyboardButton> { BtnPresente };

            var RowForse = new List<InlineKeyboardButton> { BtnForse };

            var RowAssente = new List<InlineKeyboardButton> { BtnAssente };

            var ReplyKB = new List<List<InlineKeyboardButton>> {
                RowPresente,
                RowForse,
                RowAssente
            };

            return new InlineKeyboardMarkup(ReplyKB);
        }

        private static DateTime ParseDate(string date, string time) {
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

        private static void DecodeInlineQuery(string query, out EVote choice, out long chatId, out int pollId) {
            var split = query.Split(' ');
            choice = (EVote)Enum.Parse(typeof(EVote), split[0]);
            chatId = long.Parse(split[1]);
            pollId = int.Parse(split[2]);
        }
    }
}