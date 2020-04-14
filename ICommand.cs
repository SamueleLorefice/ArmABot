using Telegram.Bot;
using Telegram.Bot.Args;

namespace ArmABot {

	internal interface ICommand {

		public void Setup(DBManager database, TelegramBotClient botClient);

		public void OnMessage(object sender, MessageEventArgs e);
	}
}