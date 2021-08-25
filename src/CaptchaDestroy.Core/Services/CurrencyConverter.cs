using System.Threading.Tasks;
using CaptchaDestroy.Core.Interfaces;

namespace CaptchaDestroy.Core.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        public Task<int> Convert(string CurrencyFrom, int AmountFrom, string CurrencyTo)
        {
            if (CurrencyFrom == CurrencyTo)
                return Task.FromResult(AmountFrom);

            if (CurrencyFrom == "RUB")
                return Task.FromResult((int)(AmountFrom * 0.014));

            if (CurrencyTo == "RUB")
                return Task.FromResult((int)(AmountFrom / 0.014));

            if (CurrencyFrom == "UAH")
                return Task.FromResult((int)(AmountFrom * 0.037));

            if (CurrencyTo == "UAH")
                return Task.FromResult((int)(AmountFrom / 0.037));

            throw new System.Exception($"Converter for {nameof(CurrencyFrom)} not working now");
        }
    }
}