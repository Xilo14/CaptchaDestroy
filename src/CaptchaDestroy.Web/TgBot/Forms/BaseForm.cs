using System.Globalization;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace CaptchaDestroy.Web.TgBot.Forms
{
    public class BaseForm : AutoCleanForm
    {
        [SaveState]
        protected CultureInfo _culture;
        protected CultureInfo Culture
        {
            get
            {
                if (_culture == null)
                    _culture = ReqLocalizationOptions.Value.DefaultRequestCulture.UICulture;
                return _culture;
            }
            set { _culture = value; }
        }
        protected Serilog.ILogger logger;
        static public ILifetimeScope DIContainer;
        public IOptions<RequestLocalizationOptions> ReqLocalizationOptions { get; set; }
        public BaseForm() : base()
        {
            logger = Serilog.Log.Logger;
            DIContainer.InjectProperties(this);
            Init += Initilization;
            DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            DeleteSide = TelegramBotBase.Enums.eDeleteSide.Both;
        }
        public override async Task Load(MessageResult message)
        {
            if (message.MessageType == Telegram.Bot.Types.Enums.MessageType.SuccessfulPayment)
                await OnSuccessfulPayment(message);

            if (message.RawUpdateArgs.Update.Type == Telegram.Bot.Types.Enums.UpdateType.PreCheckoutQuery)
                await OnPreCheckoutQuery(message);

            await base.Load(message);
            logger.Debug(
                "Form '{form}' loaded because of {updateType} event from {user} (DeviceId = {deviceId}, MessageId = {messageId}).",
                GetType().Name,
                message.MessageType,
                message.Message?.Chat?.Username,
                message.DeviceId,
                message.MessageId);
        }
        public virtual Task OnSuccessfulPayment(MessageResult message) => Task.CompletedTask;
        public virtual Task OnPreCheckoutQuery(MessageResult message) => Task.CompletedTask;
        public virtual async Task NavigateTo<T>() where T : BaseForm
        {
            var form = DIContainer.Resolve<T>();
            form.Culture = Culture;
            await NavigateTo(form);
        }
        public virtual Task Initilization(object sender, InitEventArgs args) => Task.CompletedTask;
        public virtual Task UpdateForm(MessageResult message) => Task.CompletedTask;
        public virtual async Task RedrawControls()
        {
            foreach (var b in Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Hidden(false);
            }
            //await RenderControls(null);
        }
        public override async Task Action(MessageResult message)
        {
            await UpdateForm(message);
            await RedrawControls();
        }
    }
    public class BaseForm<T> : BaseForm
    {
        public IStringLocalizer<T> Localizer { get; set; }
    }
}