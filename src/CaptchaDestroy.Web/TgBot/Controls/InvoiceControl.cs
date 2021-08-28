using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CaptchaDestroy.Web.TgBot.Forms;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using static TelegramBotBase.Base.Async;

namespace CaptchaDestroy.Web.TgBot.Controls
{
    public class InvoiceControl : ControlBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Payload { get; set; }
        public string ProviderToken { get; set; }
        public string Currency { get; set; }
        public List<LabeledPrice> Prices { get; set; }
        private bool RenderNecessary = true;
        private static readonly object __evButtonClicked = new object();
        private readonly EventHandlerList Events = new();
        public int? MessageId { get; set; }

        /// <summary>
        /// Defines which type of Button Keyboard should be rendered.
        /// </summary>
        public InvoiceControl() { }
        public event AsyncEventHandler<ButtonClickedEventArgs> ButtonClicked
        {
            add
            {
                this.Events.AddHandler(__evButtonClicked, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evButtonClicked, value);
            }
        }
        public async Task OnButtonClicked(ButtonClickedEventArgs e)
        {
            var handler = Events[__evButtonClicked]?
                .GetInvocationList()
                .Cast<AsyncEventHandler<ButtonClickedEventArgs>>();
            if (handler == null)
                return;

            foreach (var h in handler)
            {
                await Async.InvokeAllAsync(h, this, e);
            }
        }
        public override void Init()
        {
            Device.MessageDeleted += Device_MessageDeleted;
        }
        private void Device_MessageDeleted(object sender, MessageDeletedEventArgs e)
        {
            if (MessageId == null)
                return;

            if (e.MessageId != MessageId)
                return;

            MessageId = null;
        }
        public async override Task Action(MessageResult result, string value = null)
        {
            if (result.Handled)
                return;

            if (!result.IsFirstHandler)
                return;


            //await OnButtonClicked(new ButtonClickedEventArgs(button, index));

            result.Handled = true;
            return;

        }
        public async override Task Render(MessageResult result)
        {
            if (!RenderNecessary)
                return;

            RenderNecessary = false;

            if (MessageId != null)
                await Cleanup();

            var m = await Device.RAW(a => a.SendInvoiceAsync(
            Device.DeviceId, Title, Description,
            Payload ?? "", ProviderToken, Currency,
            Prices));

            MessageId = m?.MessageId ?? MessageId;
        }
        public override async Task Hidden(bool FormClose)
        {
            //Prepare for opening Modal, and comming back
            if (!FormClose)
            {
                this.Updated();
            }
            else
            {
                //Remove event handler
                this.Device.MessageDeleted -= Device_MessageDeleted;
            }
        }

        /// <summary>
        /// Tells the control that it has been updated.
        /// </summary>
        public void Updated()
        {
            this.RenderNecessary = true;
        }

        public async override Task Cleanup()
        {
            if (MessageId == null)
                return;

            await Device.DeleteMessage(MessageId.Value);
            MessageId = null;
        }
    }
}