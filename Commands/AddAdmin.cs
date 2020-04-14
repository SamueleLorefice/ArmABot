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
using ArmABot;

namespace ArmABot.Commands {
	public class AddAdmin : ICommand {
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
	}
}
