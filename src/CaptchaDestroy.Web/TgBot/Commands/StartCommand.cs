using System.Threading.Tasks;
using CaptchaDestroy.Web.TgBot.Forms;
using TelegramBotBase.Args;

namespace CaptchaDestroy.Web.TgBot.Commands
{
    public class StartCommand : BaseCommand
    {
        public StartCommand()
        {
            Command = "start";
            Description = "Startss the bot";
        }
        public override Task HandleCommand(object sender, BotCommandEventArgs e)
        {
            if (e.Device.ActiveForm is StartForm)
                return Task.CompletedTask;
            return (e.Device.ActiveForm as BaseForm).NavigateTo<StartForm>();
        }
    }
}