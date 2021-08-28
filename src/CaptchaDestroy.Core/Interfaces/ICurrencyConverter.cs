using System.Threading.Tasks;
using Ardalis.Result;

namespace CaptchaDestroy.Core.Interfaces
{
    public interface ICurrencyConverter
    {
        Task<int> Convert(
            string CurrencyFrom, int AmountFrom,
            string CurrencyTo);
    }
}