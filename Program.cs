using ArmABot.DBTables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Poll = ArmABot.DBTables.Poll;

namespace ArmABot {

	internal class Program {
		private static DBManager database;
		private static TelegramBotClient botClient;
		public static string ConnectionString;

		private static void Main(string[] args) {
			var token = "0";
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

			var commands = new List<ICommand> {
				new Commands.AddAdmin(),
				new Commands.AddEvent(),
				new Commands.GetPolls(),
				new Commands.ResendPoll()
			};

			var callbacks = new List<ICallback> {
				new Commands.CallbackQuery()
			};

			foreach (ICommand comm in commands) {
				comm.Setup(database, botClient);
				botClient.OnMessage += comm.OnMessage;
			}

			foreach (ICallback query in callbacks) {
				query.Setup(database, botClient);
				botClient.OnCallbackQuery += query.OnCallback;
			}

			Console.WriteLine("Starting Bot...");
			botClient.StartReceiving(new UpdateType[] { UpdateType.CallbackQuery, UpdateType.Message });
			Console.WriteLine("All fine, bot running...");
			Thread.Sleep(Timeout.Infinite);
		}

		/// <summary>
		/// Decodes Inline query string from poll inline buttons
		/// </summary>
		/// <param name="query"></param>
		/// <param name="choice"></param>
		/// <param name="chatId"></param>
		/// <param name="pollId"></param>
		public static void DecodeInlineQuery(string query, out EVote choice, out long chatId, out int pollId) {
			var split = query.Split(' ');
			choice = (EVote)Enum.Parse(typeof(EVote), split[0]);
			chatId = long.Parse(split[1]);
			pollId = int.Parse(split[2]);
		}

		public static InlineKeyboardMarkup GetReplyMarkUp(long chatId, int pollId) {
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

		//TODO: This needs to be renamed to something more meaningful. This function "gets text", indeed, but actually generates the updated poll message text.
		//TODO: This should really be relocated to a separate class or something.
		//TODO: Start using language strings for an eventual multiple language support.
		public static string GetText(int pollId) {
			Poll poll = database.GetPoll(pollId);
			var closed = poll.EventDate < DateTime.Now;
			IEnumerable<Vote> votes = database.GetVotesInPoll(pollId);
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
	}
}