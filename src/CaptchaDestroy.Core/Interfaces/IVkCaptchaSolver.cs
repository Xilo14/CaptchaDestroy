using System;
using System.Threading.Tasks;

namespace CaptchaDestroy.Core.Interfaces
{
    public interface IVkCaptchaSolver
    {
        Task<string> SolveCaptcha(Uri uri);
    }
}
