using Autofac;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Core.Services;

namespace CaptchaDestroy.Core
{
    public class DefaultCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // builder.RegisterType<ToDoItemSearchService>()
            //     .As<IToDoItemSearchService>().InstancePerLifetimeScope();

            builder.RegisterType<CaptchaService>()
                .As<ICaptchaService>().InstancePerLifetimeScope();

            builder.RegisterType<CurrencyConverter>()
                .As<ICurrencyConverter>().InstancePerLifetimeScope();

            builder.RegisterType<AccountService>()
                .As<IAccountService>().InstancePerLifetimeScope();
        }
    }
}
