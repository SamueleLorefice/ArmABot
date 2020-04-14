using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace ArmABot {
	interface ICommand {
		public void Setup(DBManager database, TelegramBotClient botClient);
		public void OnMessage(object sender, MessageEventArgs e);
	}
}
