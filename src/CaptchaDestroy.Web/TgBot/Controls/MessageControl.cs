using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using static TelegramBotBase.Base.Async;

namespace CaptchaDestroy.Web.TgBot.Controls
{
    public class MessageControl : ControlBase
    {
        public string Text { get; set; }
        private bool RenderNecessary = true;
        private static readonly object __evButtonClicked = new object();
        private readonly EventHandlerList Events = new();
        public ButtonForm ButtonsForm { get; set; }
        public int? MessageId { get; set; }

        /// <summary>
        /// Optional. Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller if there are just two rows of buttons). Defaults to false, in which case the custom keyboard is always of the same height as the app's standard keyboard.
        /// Source: https://core.telegram.org/bots/api#replykeyboardmarkup
        /// </summary>
        public bool HideKeyboardOnCleanup { get; set; } = true;
        /// <summary>
        /// Parsemode of the message.
        /// </summary>
        public ParseMode MessageParseMode { get; set; } = ParseMode.Default;

        /// <summary>
        /// Defines which type of Button Keyboard should be rendered.
        /// </summary>
        public MessageControl() : this(new ButtonForm()) { }
        public MessageControl(ButtonForm form)
        {
            ButtonsForm = form;
        }
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

            var button = ButtonsForm.ToList().FirstOrDefault(a => a.Value == result.RawData);

            var index = ButtonsForm.FindRowByButton(button);

            if (button != null)
            {
                await OnButtonClicked(new ButtonClickedEventArgs(button, index));

                result.Handled = true;
                return;
            }
        }
        public async override Task Render(MessageResult result)
        {
            if (!RenderNecessary)
                return;

            RenderNecessary = false;

            ButtonForm form = ButtonsForm;
            Message m;

            if (MessageId != null)
                m = await Device.Edit(MessageId.Value, Text, (InlineKeyboardMarkup)form);
            else
                //When no message id is available or it has been deleted due the use 
                //of AutoCleanForm re-render automatically
                m = await Device.Send(
                    Text, (InlineKeyboardMarkup)form, disableNotification: true,
                    parseMode: MessageParseMode, MarkdownV2AutoEscape: false);

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