using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Web.TgBot.Controls;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace CaptchaDestroy.Web.TgBot.Forms
{
    public class MainForm : BaseForm<StartForm>
    {
        private ButtonBase _goToDepositBtn;
        private MessageControl _msg;
        public IAccountService AccountService { get; set; }
        public override async Task Initilization(object sender, InitEventArgs args)
        {
            await base.Initilization(sender, args);

            var bf = new ButtonForm();

            _goToDepositBtn = new ButtonBase("", "gotodeposit");
            bf.AddButtonRow(_goToDepositBtn);

            _msg = new MessageControl(bf);

            //await UpdateForm(null);
            AddControl(_msg);
        }
        public override async Task UpdateForm(MessageResult message)
        {
            var account = await AccountService.GetOrCreateAccount(Device.DeviceId);

            var strBuilder = new StringBuilder();

            strBuilder.Append(Localizer.GetWithCulture("Hello, {0}!", Culture, Device.GetChatTitle()));
            strBuilder.Append(Environment.NewLine);
            strBuilder.Append(Environment.NewLine);
            strBuilder.Append(Localizer.GetWithCulture("DC Points: {0}", Culture, account.Value.Points));

            _msg.Text = strBuilder.ToString();
            _goToDepositBtn.Text = Localizer.GetWithCulture("Deposit", Culture);
        }

        public override async Task Action(MessageResult message)
        {
            if (message.RawData == "gotodeposit"){
                await NavigateTo<DepositForm>();
                return;
            }

            await base.Action(message);
        }
    }
}