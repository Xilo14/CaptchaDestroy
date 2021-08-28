using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.SharedKernel;

namespace CaptchaDestroy.Core.ProjectAggregate.Events
{
    public class CaptchaSolvedEvent : BaseDomainEvent
    {
        public Captcha SolvedCaptcha { get; set; }

        public CaptchaSolvedEvent(Captcha solvedCaptcha)
        {
            SolvedCaptcha = solvedCaptcha;
        }
    }
}