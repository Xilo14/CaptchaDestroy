using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CaptchaDestroy.Web.TgBot.Controls;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace CaptchaDestroy.Web.TgBot.Forms
{
    public class StartForm : BaseForm<StartForm>
    {
        private ButtonBase _gotItBtn;
        private MessageControl _msg;
        public override async Task Initilization(object sender, InitEventArgs args)
        {
            await base.Initilization(sender, args);

            var bf = new ButtonForm();

            bf.AddButtonRow(ReqLocalizationOptions.Value.SupportedUICultures.Select(
                c => new ButtonBase(c.GetFlagEmoji(), c.Name)).ToArray());

            _gotItBtn = new ButtonBase("", "gotit");
            bf.AddButtonRow(_gotItBtn);

            _msg = new MessageControl(bf);

            await UpdateForm(null);
            AddControl(_msg);
        }
        public override Task UpdateForm(MessageResult message)
        {
            _msg.Text = Localizer.GetWithCulture("Bot is started!", Culture)
                + Environment.NewLine
                + Localizer.GetWithCulture("Choose language:", Culture);
            _gotItBtn.Text = Localizer.GetWithCulture("Got it!", Culture);

            return Task.CompletedTask;
        }
        public override async Task Action(MessageResult message)
        {
            if (message.RawData == null)
                return;

            if (message.RawData == "gotit")
            {
                await NavigateTo<MainForm>();
                return;
            }

            Culture = ReqLocalizationOptions.Value.SupportedUICultures
                .Where(c => c.Name == message.RawData)
                .FirstOrDefault();

            await message.ConfirmAction(Localizer.GetWithCulture("Language changed", Culture));

            await base.Action(message);
        }
    }
    public static class CultureInfoExtension
    {
        public static string GetFlagEmoji(this CultureInfo culture)
        {
            var r = new RegionInfo(culture.LCID);
            return string.Concat(r.TwoLetterISORegionName.Select(c =>
            {
                var bytes = Encoding.Unicode.GetBytes(c.ToString());
                var num = BitConverter.ToInt16(bytes);
                var newnum = num + 127397;
                var res = Encoding.UTF32.GetString(BitConverter.GetBytes(newnum));

                return res;
            }).ToList());
        }
    }
}