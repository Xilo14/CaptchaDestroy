using System.Collections.Generic;

namespace CaptchaDestroy.Web.TgBot
{
    public class TelegramPaymentsConfiguration
    {
        public const string TelegramPayments = "TelegramPayments";
        public List<ProviderInfo> Providers { get; set; }
    }

    public class ProviderInfo
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public List<string> Currencies { get; set; }
    }
}