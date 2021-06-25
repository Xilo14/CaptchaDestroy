using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.SharedKernel;

namespace CaptchaDestroy.Core.ProjectAggregate.Events
{
    public class ToDoItemCompletedEvent : BaseDomainEvent
    {
        public ToDoItem CompletedItem { get; set; }

        public ToDoItemCompletedEvent(ToDoItem completedItem)
        {
            CompletedItem = completedItem;
        }
    }
}