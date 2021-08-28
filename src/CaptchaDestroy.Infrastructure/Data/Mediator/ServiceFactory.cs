using System;

namespace CaptchaDestroy.Infrastructure.Data.Mediator
{
	public delegate object ServiceFactoryNested(Type serviceType);
    public delegate object ServiceFactoryNeighboured(Type serviceType);
}