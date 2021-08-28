using System.Collections.Generic;

namespace CaptchaDestroy.Web.TgBot
{
    public class DCPointsAmountsConfiguration
    {
        public const string DCPointsAmounts = "DCPointsAmounts";
        public List<AmountInfo> Amounts { get; set; }
        public int DCPointsPerDollar { get; set; }
    }
    public class AmountInfo
    {
        public int Amount { get; set; }
        public double Discount { get; set; }
    }
}