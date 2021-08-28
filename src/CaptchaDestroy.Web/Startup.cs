using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Ardalis.ListStartupServices;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using CaptchaDestroy.Core;
using CaptchaDestroy.Infrastructure;
using CaptchaDestroy.Infrastructure.Data;
using CaptchaDestroy.Web.Api;
using CaptchaDestroy.Web.TgBot;
using CaptchaDestroy.Web.TgBot.Commands;
using CaptchaDestroy.Web.TgBot.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using TelegramBotBase;

namespace CaptchaDestroy.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            Configuration = config;
            _env = env;
        }

        public IConfiguration Configuration { get; private set; }
        public ILifetimeScope AutofacContainer { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            string connectionString = Configuration.GetConnectionString("SqliteConnection");
            //Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext(connectionString);

            services.AddControllers().AddControllersAsServices();
            //services.AddControllersWithViews().AddNewtonsoftJson();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            //services.AddTgBot(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Captcha Destroy API", Version = "v1" });
                c.EnableAnnotations();

                //comments path
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });


            // add list services for diagnostic purposes - see https://github.com/ardalis/AspNetCoreStartupServices
            services.Configure<ServiceConfig>(config =>
            {
                config.Services = new List<ServiceDescriptor>(services);

                // optional - default path to view services is /listallservices - recommended to choose your own path
                config.Path = "/listservices";
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-us"),
                    new CultureInfo("ru-ru"),
                    new CultureInfo("en-DE")
                };

                options.DefaultRequestCulture = new RequestCulture(culture: "en-us", uiCulture: "en-us");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.Configure<TelegramPaymentsConfiguration>(Configuration.GetSection(
                                        TelegramPaymentsConfiguration.TelegramPayments));
            services.Configure<DCPointsAmountsConfiguration>(Configuration.GetSection(
                                        DCPointsAmountsConfiguration.DCPointsAmounts));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new DefaultCoreModule());
            builder.RegisterModule(new DefaultInfrastructureModule(_env.EnvironmentName == "Development"));
            builder.RegisterModule(new TgBotModule(Configuration, false));
            //builder.RegisterType()
            // builder.RegisterAssemblyTypes(typeof(BaseApiController).Assembly)
            //     .Where(t => t.IsSubclassOf(typeof(BaseApiController)))
            //     .InstancePerLifetimeScope()
            //     .PropertiesAutowired();

            builder.RegisterAssemblyTypes(typeof(BaseCommand).Assembly)
                .Where(t => t.IsSubclassOf(typeof(BaseCommand)))
                .SingleInstance()
                .PropertiesAutowired();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            //BaseForm.DIContainer = AutofacContainer.BeginLifetimeScope("TgBot");
            //AutofacContainer.Resolve<BotBase<StartForm>>();


            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseShowAllServicesMiddleware();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            SwaggerBuilderExtensions.UseSwagger(app);

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Captcha Destroy API V1"));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTgBot(this IServiceCollection services, IConfiguration Configuration)
        {
            var apiKey = Configuration.GetSection("TgBot").GetValue<string>("Token");
            var bot = new BotBase<StartForm>(apiKey);

            bot.Start();



            //bot.BotCommands.Add()
            // bot.BotCommand += async (s, en) => {

            // }

            services.AddSingleton(bot);

            return services;
        }
    }
}
