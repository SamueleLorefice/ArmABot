using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using ArmABot.DBTables;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot {

    internal class Program {
        private static DBManager database;
        private static TelegramBotClient botClient;
        public static string ConnectionString;

        private static void Main(string[] args) {
            string token = "0";
            if (args.Length < 2) {
                Console.WriteLine("Missing arguments.");
                Console.WriteLine("BotToken ConnectionString");
                Console.WriteLine("Trying enviroment variables");
                var env = (Hashtable)Environment.GetEnvironmentVariables();
                foreach (DictionaryEntry envVar in env) {
                    switch (envVar.Key.ToString()) {
                        case "BOT_TOKEN":
                            token = envVar.Value.ToString();
                            break;

                        case "CONNECTION_STRING":
                            ConnectionString = envVar.Value.ToString();
                            break;

                        default:
                            break;
                    }
                }
                if (token == "0" || ConnectionString == null) {
                    Console.WriteLine("Please start the bot using command line arguments <Token> <ConnectionString> or set up \"BOT_TOKEN\" and \"CONNECTION_STRING\" enviroment variables.");
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
            database = new DBManager();
            //TODO: handle an eventual down database situation
            Console.WriteLine($"Database connection = {database.TestConnection()}");
            Console.WriteLine("Initializing bot...");
            botClient = new TelegramBotClient(token);
            Console.WriteLine("Registering Callbacks...");
            botClient.OnMessage += AddEventHandler;
            botClient.OnMessage += AddAdminHandler;
            botClient.OnMessage += GetPollsHandler;
            botClient.OnMessage += ResendPollHandler;
            botClient.OnCallbackQuery += CallbackQueryHandler;
            Console.WriteLine("Starting Bot...");
            botClient.StartReceiving(new UpdateType[] { UpdateType.CallbackQuery, UpdateType.Message });
            Console.WriteLine("All fine, bot running...");
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Handler for callback from poll button pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CallbackQueryHandler(object sender, CallbackQueryEventArgs e) {
            var senderId = e.CallbackQuery.From.Id;
            DecodeInlineQuery(e.CallbackQuery.Data, out EVote choice, out var chatId, out var pollId);
            Poll poll = database.GetPoll(pollId);
            var votes = database.GetVotesInPollFrom(senderId, pollId).ToList();
            if (poll.EventDate < DateTime.Now) {
                botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Questo poll è chiuso");
                botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, GetText(pollId), replyMarkup: null, parseMode: ParseMode.Html);
            } else {
                if (votes.Count == 1) {
                    var id = votes[0].Id;
                    database.EditVote(id, choice);
                    botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, GetText(pollId), replyMarkup: GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
                    botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Il tuo voto è stato modificato");
                } else if (votes.Count == 0 || votes == null) {
                    database.AddVote(choice, pollId, senderId, e.CallbackQuery.From.FirstName);
                    botClient.EditMessageTextAsync(new ChatId(chatId), (int)poll.MessageId, GetText(pollId), replyMarkup: GetReplyMarkUp(chatId, pollId), parseMode: ParseMode.Html);
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

        [Obsolete]
        /// <summary>
        /// Handler for the AddAdmin command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AddAdminHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
            if (e.Message.Text.ToLower().Contains("/addadmin")) {
                Admin checkAdm = database.FindAdmin(e.Message.From.Id, e.Message.Chat.Id);
                if (checkAdm == null) {
                    database.AddAdmin(new Admin { UserId = e.Message.From.Id, GroupId = e.Message.Chat.Id });
                    botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Added as admin of this chat");
                } else {
                    botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "You are already an admin of this chat");
                }
            }
        }

        /// <summary>
        /// Handler for the AddEvent command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AddEventHandler(object sender, MessageEventArgs e) {
            if (e.Message.Text == null) {
                return;
            }
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
                        InlineKeyboardMarkup markup = GetReplyMarkUp(e.Message.Chat.Id, pollId);
                        botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, GetText(pollId), replyMarkup: markup, parseMode: ParseMode.Html);
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
            }else if (e.Message.Text.ToLower().StartsWith("/addevent")) {
                botClient.SendTextMessageAsync(new ChatId(e.Message.Chat.Id), "Wrong syntax. use: /addevent '<title>' <date> <time> +<minium number of partecipants>");
            }
        }

        /// <summary>
        /// Handler for the GetPolls command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GetPollsHandler(object sender, MessageEventArgs e) {
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

        /// <summary>
        /// Handler for resending polls after the GetPolls command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ResendPollHandler(object sender, MessageEventArgs e) {
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
                    InlineKeyboardMarkup markup = GetReplyMarkUp(e.Message.Chat.Id, poll.PollId);
                    botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), (int)poll.MessageId, GetText(id), replyMarkup: markup, parseMode: ParseMode.Html);
                } catch (NullReferenceException exc) {
                    botClient.EditMessageTextAsync(new ChatId(e.Message.Chat.Id), MsgId.Result.MessageId, $"Can't load the poll, reason:\n{exc.Message}");
                }
            }
        }

        /// <summary>
        /// Returns the current text for a poll
        /// </summary>
        /// <param name="pollId"></param>
        /// <returns></returns>
        private static string GetText(int pollId) {
            Poll poll = database.GetPoll(pollId);
            bool closed = poll.EventDate < DateTime.Now;
            IEnumerable <Vote> votes = database.GetVotesInPoll(pollId);
            Vote[] Present = votes.Where(x => x.Choice == EVote.Present).ToArray();
            Vote[] Maybe = votes.Where(x => x.Choice == EVote.Maybe).ToArray();
            Vote[] Absent = votes.Where(x => x.Choice == EVote.Absent).ToArray();
            var text = "<b>📰";
            if (closed) {
                text += poll.Title + $"\n————————————————————\n<b>POLL CHIUSO</b>\n{poll.EventDate.Day:D2}/{poll.EventDate.Month:D2}/{poll.EventDate.Year:D4} {poll.EventDate.Hour:D2}:{poll.EventDate.Minute:D2}\n\n✅Presenti: {Present.Length}</b>\n";
            } else {
                text += poll.Title + $"\n————————————————————\n{poll.EventDate.Day:D2}/{poll.EventDate.Month:D2}/{poll.EventDate.Year:D4} {poll.EventDate.Hour:D2}:{poll.EventDate.Minute:D2}\n\n✅Presenti: {Present.Length}</b>\n";
            }
            foreach (Vote people in Present) {
                text += "    • " + people.Username + "\n";
            }
            text += $"\n<b>⚠️ Forse: {Maybe.Length}</b>\n";
            foreach (Vote people in Maybe) {
                text += "    • " + people.Username + "\n";
            }
            text += $"\n<b>❌ Assente: {Absent.Length}</b>\n";
            foreach (Vote people in Absent) {
                text += "    • " + people.Username + "\n";
            }
            if (!closed) {
                text += $"\n<b>Slot Minimi:</b> {Present.Length} + ({Maybe.Length}) / {poll.EventQuota}";
            } else {
                text += $"\n<b>Partecipanti: {Present.Length} + ?{Maybe.Length}/{poll.EventQuota} minimi.";
            }
            return text;
        }

        /// <summary>
        /// Returns Inline Keyboards for a poll
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="pollId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Parses date string converting it to DateTime
        /// </summary>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Decodes Inline query string from poll inline buttons
        /// </summary>
        /// <param name="query"></param>
        /// <param name="choice"></param>
        /// <param name="chatId"></param>
        /// <param name="pollId"></param>
        private static void DecodeInlineQuery(string query, out EVote choice, out long chatId, out int pollId) {
            var split = query.Split(' ');
            choice = (EVote)Enum.Parse(typeof(EVote), split[0]);
            chatId = long.Parse(split[1]);
            pollId = int.Parse(split[2]);
        }
    }
}