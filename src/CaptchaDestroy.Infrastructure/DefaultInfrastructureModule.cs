using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Infrastructure.Data;
using CaptchaDestroy.Infrastructure.Data.Mediator;
using CaptchaDestroy.SharedKernel.Interfaces;
using MediatR;
using MediatR.Pipeline;
using Module = Autofac.Module;

namespace CaptchaDestroy.Infrastructure
{
    public class DefaultInfrastructureModule : Module
    {
        private readonly bool _isDevelopment = false;
        private readonly List<Assembly> _assemblies = new List<Assembly>();

        public DefaultInfrastructureModule(bool isDevelopment, Assembly callingAssembly =  null)
        {
            _isDevelopment = isDevelopment;
            var coreAssembly = Assembly.GetAssembly(typeof(Account)); // TODO: Replace "Project" with any type from your Core project
            var infrastructureAssembly = Assembly.GetAssembly(typeof(StartupSetup));
            _assemblies.Add(coreAssembly);
            _assemblies.Add(infrastructureAssembly);
            if (callingAssembly != null)
            {
                _assemblies.Add(callingAssembly);
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_isDevelopment)
            {
                RegisterDevelopmentOnlyDependencies(builder);
            }
            else
            {
                RegisterProductionOnlyDependencies(builder);
            }
            RegisterCommonDependencies(builder);
        }

        private void RegisterCommonDependencies(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EfRepository<>))
                .As(typeof(IRepository<>))
                .As(typeof(IReadRepository<>))
                .InstancePerLifetimeScope();

            builder
                .RegisterType<Publisher>()
                .InstancePerLifetimeScope();

            builder.Register<ServiceFactoryNested>(context =>
            {
                var c = context.Resolve<ILifetimeScope>();
                return t => c.Resolve(t);
            });
            builder.Register<ServiceFactoryNeighboured>(context =>
            {
                var c = context.Resolve<ILifetimeScope>();
                var newl = (c as LifetimeScope).RootLifetimeScope.BeginLifetimeScope();
                return t => newl.Resolve(t);
            });

            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(IRequestExceptionHandler<,,>),
                typeof(IRequestExceptionAction<,>),
                typeof(INotificationHandler<>),
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder
                .RegisterAssemblyTypes(_assemblies.ToArray())
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();
            }

            builder
                .RegisterType<EmailSender>().As<IEmailSender>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<VkCaptchaSolver>().As<IVkCaptchaSolver>()
                .InstancePerLifetimeScope();
        }

        private void RegisterDevelopmentOnlyDependencies(ContainerBuilder builder)
        {
            // TODO: Add development only services
        }

        private void RegisterProductionOnlyDependencies(ContainerBuilder builder)
        {
            // TODO: Add production only services
        }

    }
}
