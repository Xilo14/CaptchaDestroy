namespace CaptchaDestroy.Core.ProjectAggregate.Events.Interfaces
{
    public interface IHasPublishStrategy
    {
        public PublishStrategy? PublishStrategy { get; set; }
    }
}