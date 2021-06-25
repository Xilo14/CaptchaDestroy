using Autofac;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.Services;

namespace CaptchaDestroy.Core
{
    public class DefaultCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ToDoItemSearchService>()
                .As<IToDoItemSearchService>().InstancePerLifetimeScope();
        }
    }
}
