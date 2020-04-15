using Telegram.Bot;
using Telegram.Bot.Args;

namespace ArmABot {

	public interface ICallback {

		public void Setup(DBManager database, TelegramBotClient botClient);

		public void OnCallback(object sender, CallbackQueryEventArgs e);
	}
}