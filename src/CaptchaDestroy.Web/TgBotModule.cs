using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using CaptchaDestroy.Core.Interfaces;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Infrastructure.Data;
using CaptchaDestroy.Infrastructure.Data.Mediator;
using CaptchaDestroy.SharedKernel.Interfaces;
using CaptchaDestroy.Web.TgBot.Commands;
using CaptchaDestroy.Web.TgBot.Forms;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;
using TelegramBotBase;
using Module = Autofac.Module;

namespace CaptchaDestroy.Web
{
    public class TgBotModule : Module
    {
        private readonly bool _isUseWebhook = false;
        private readonly IConfiguration _configuration;
        private readonly List<Assembly> _assemblies = new List<Assembly>();

        public TgBotModule(IConfiguration Configuration, bool isUseWebhook, Assembly callingAssembly = null)
        {
            _configuration = Configuration;
            _isUseWebhook = isUseWebhook;
            var tgCommandsAssembly = Assembly.GetAssembly(typeof(BaseCommand));
            var tgFormsAssembly = Assembly.GetAssembly(typeof(BaseForm));
            _assemblies.Add(tgCommandsAssembly);
            _assemblies.Add(tgFormsAssembly);
            if (callingAssembly != null)
            {
                _assemblies.Add(callingAssembly);
            }
            _assemblies = _assemblies.Distinct().ToList();
        }

        protected override void Load(ContainerBuilder builder)
        {
            var apiKey = _configuration.GetSection("TgBot").GetValue<string>("Token");

            builder
                .RegisterAssemblyTypes(_assemblies.ToArray())
                .Where(t => t.IsSubclassOf(typeof(BaseCommand)))
                .As<IBotCommand>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterAssemblyTypes(_assemblies.ToArray())
                .Where(t => t.IsSubclassOf(typeof(BaseForm)))
                .As<BaseForm>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .Register((context) =>
                {
                    var lifetimeScope = context.Resolve<ILifetimeScope>();
                    var tgLifetimeScope =
                        (lifetimeScope as LifetimeScope).RootLifetimeScope
                        .BeginLifetimeScope("TgBot");

                    BaseForm.DIContainer = tgLifetimeScope;

                    var bot = new BotBase<StartForm>(apiKey);

                    var commands = tgLifetimeScope
                        .Resolve<IEnumerable<IBotCommand>>()
                        .Cast<BaseCommand>();

                    bot.BotCommands = commands
                        .Cast<BotCommand>()
                        .ToList();

                    bot.BotCommand += async (s, en) =>
                    {
                        await en.Device.DeleteMessage(en.OriginalMessage.MessageId);
                        var currentCommand = commands.FirstOrDefault(c => "/" + c.Command == en.Command);
                        await currentCommand.HandleCommand(s, en);
                    };

                    bot.UploadBotCommands().Wait();

                    //TODO: Add webhook
                    bot.Start();

                    return bot;
                })
                .SingleInstance()
                .AutoActivate();
            // builder
            //     .RegisterBuildCallback(c => c.Resolve<BotBase<StartForm>>());

        }

        private void RegisterCommonDependencies(ContainerBuilder builder)
        {
            // builder.RegisterGeneric(typeof(EfRepository<>))
            //     .As(typeof(IRepository<>))
            //     .As(typeof(IReadRepository<>))
            //     .InstancePerLifetimeScope();

            // builder
            //     .RegisterType<Publisher>()
            //     .InstancePerLifetimeScope();

            // builder.Register<ServiceFactoryNested>(context =>
            // {
            //     var c = context.Resolve<ILifetimeScope>();
            //     return t => c.Resolve(t);
            // });
            // builder.Register<ServiceFactoryNeighboured>(context =>
            // {
            //     var c = context.Resolve<ILifetimeScope>();
            //     var newl = (c as LifetimeScope).RootLifetimeScope.BeginLifetimeScope();
            //     return t => newl.Resolve(t);
            // });

            // var mediatrOpenTypes = new[]
            // {
            //     typeof(IRequestHandler<,>),
            //     typeof(IRequestExceptionHandler<,,>),
            //     typeof(IRequestExceptionAction<,>),
            //     typeof(INotificationHandler<>),
            // };

            // foreach (var mediatrOpenType in mediatrOpenTypes)
            // {
            //     builder
            //     .RegisterAssemblyTypes(_assemblies.ToArray())
            //     .AsClosedTypesOf(mediatrOpenType)
            //     .AsImplementedInterfaces();
            // }

            // builder
            //     .RegisterType<EmailSender>().As<IEmailSender>()
            //     .InstancePerLifetimeScope();

            // builder
            //     .RegisterType<VkCaptchaSolver>().As<IVkCaptchaSolver>()
            //     .InstancePerLifetimeScope();
        }
    }
}
