using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Messenger.Client.Extensions;
using RandoBot.Service.Repositories;
using RandoBot.Service.Services;

namespace RandoBot.Service
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="Startup"/> class
        /// </summary>
        /// <param name="env">The environment.</param>
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// The configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var pageAccessToken = Environment.GetEnvironmentVariable("PAGE_ACCESS_TOKEN");
            if (pageAccessToken == null)
            {
                throw new Exception("Cannot find PAGE_ACCESS_TOKEN in this env.");
            }

            var verifyToken = Environment.GetEnvironmentVariable("VERIFY_TOKEN");
            if (verifyToken == null)
            {
                throw new Exception("Cannot find VERIFY_TOKEN in this env.");
            }

            services.AddMvc();

            services.AddScoped<UserRepository, UserRepository>();
            services.AddScoped<PictureRepository, PictureRepository>();
            services.AddSingleton<RedisService, RedisService>();
            services.AddSingleton<MessageProcessorService, MessageProcessorService>();

            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Price TAG Cloud API",
                    Description = "API Version 1",
                    TermsOfService = "None"
                });
            });

            services.AddMessengerClient(pageAccessToken);
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application to configure.</param>
        /// <param name="env">The environment to configure.</param>
        /// <param name="loggerFactory">The log factory to configure.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUi();
        }
    }
}
