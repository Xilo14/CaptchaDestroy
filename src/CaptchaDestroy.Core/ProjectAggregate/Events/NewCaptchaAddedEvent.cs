using CaptchaDestroy.Core.ProjectAggregate.Events.Interfaces;
using CaptchaDestroy.SharedKernel;

namespace CaptchaDestroy.Core.ProjectAggregate.Events
{
    public class NewCaptchaAddedEvent : BaseDomainEvent, IHasPublishStrategy
    {
        public Captcha Captcha { get; set; }
        public Account Account { get; set; }
        public PublishStrategy? PublishStrategy { get; set; } = null;

        public NewCaptchaAddedEvent(
            Account account,
            Captcha captcha)
        {
            Account = account;
            Captcha = captcha;
        }
        public NewCaptchaAddedEvent(
            Account account,
            Captcha captcha,
            PublishStrategy? publishStrategy)
        {
            PublishStrategy = publishStrategy;
            Account = account;
            Captcha = captcha;
        }
    }
}