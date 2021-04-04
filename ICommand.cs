using Telegram.Bot;
using Telegram.Bot.Args;

namespace ArmABot {
	/// <summary>
	/// Interface describing a generic command that can be triggered on the bot
	/// </summary>
	internal interface ICommand {
		/// <summary>
		/// Method called once every startup of the bot, during commands initialization.
		/// Use this to set up references to database and bot client classes, or initial values setup.
		/// </summary>
		/// <param name="database">reference to the detabase manager class</param>
		/// <param name="botClient">ereference to the botclient class</param>
		public void Setup(DBManager database, TelegramBotClient botClient);

		/// <summary>
		/// Called when a message is received. To be used to actually check if the received message is referencing this command.
		/// </summary>
		/// <param name="sender">Who sent the message</param>
		/// <param name="e">EventArgs containing a Telegram Api Message</param>
		public void OnMessage(object sender, MessageEventArgs e);
	}
}