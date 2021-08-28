using System.Globalization;
using Microsoft.Extensions.Localization;

namespace CaptchaDestroy.Web
{
    public static class StringLocalizerExtension
    {
        public static LocalizedString GetWithCulture(
            this IStringLocalizer localizer, string name, CultureInfo culture)
        {
            var oldCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = culture;
            var value = localizer[name];
            CultureInfo.CurrentUICulture = oldCulture;
            return value;
        }
        public static LocalizedString GetWithCulture(
            this IStringLocalizer localizer, string name, CultureInfo culture, params object[] arguments)
        {
            var oldCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = culture;
            var value = localizer[name, arguments];
            CultureInfo.CurrentUICulture = oldCulture;
            return value;
        }
    }
}