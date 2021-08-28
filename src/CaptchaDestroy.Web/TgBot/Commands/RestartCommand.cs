using System.Threading.Tasks;
using CaptchaDestroy.Web.TgBot.Forms;
using TelegramBotBase.Args;

namespace CaptchaDestroy.Web.TgBot.Commands
{
    public class RestartCommand : BaseCommand
    {
        public RestartCommand()
        {
            Command = "restart";
            Description = "Restart the bot";
        }
        public override Task HandleCommand(object sender, BotCommandEventArgs e)
        {
            return (e.Device.ActiveForm as BaseForm).NavigateTo<StartForm>();
        }
    }
}