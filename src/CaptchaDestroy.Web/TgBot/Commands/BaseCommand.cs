using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Args;

namespace CaptchaDestroy.Web.TgBot.Commands
{
	public interface IBotCommand {};
	public abstract class BaseCommand : BotCommand, IBotCommand
	{
		public BaseCommand() {
			Console.WriteLine();
		}
        public abstract Task HandleCommand(object sender, BotCommandEventArgs e);
	}
}