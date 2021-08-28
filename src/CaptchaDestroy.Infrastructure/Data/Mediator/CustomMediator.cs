using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace CaptchaDestroy.Infrastructure.Data.Mediator
{
    public class CustomMediator : MediatR.Mediator
    {
        private Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> _publish;

        public CustomMediator(ServiceFactory serviceFactory, Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish) : base(serviceFactory)
        {
            _publish = publish;
        }
        public CustomMediator(ServiceFactoryNested serviceFactoryNested, Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
            : base(s => serviceFactoryNested(s))
        {
            _publish = publish;
        }
        public CustomMediator(ServiceFactoryNeighboured serviceFactoryNeighboured, Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
            : base(s => serviceFactoryNeighboured(s))
        {
            _publish = publish;
        }


        protected override Task PublishCore(IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers, INotification notification, CancellationToken cancellationToken)
        {
            return _publish(allHandlers, notification, cancellationToken);
        }
    }
}