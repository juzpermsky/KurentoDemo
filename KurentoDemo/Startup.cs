using System;
using Kurento.NET;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KurentoDemo.Hubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace KurentoDemo
{
    
    public class MyLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                Console.WriteLine($"MyLogger: {message}");
            }   
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
       
            //there is your kms address

            // ConsoleLogger consoleLogger = new ConsoleLogger("KurentoClient",(s, level) => true,false);
            MyLogger consoleLogger = new MyLogger();
            services.AddSingleton(p => new KurentoClient("ws://hive.ru:8888/kurento", consoleLogger));
            services.AddSingleton<RoomSessionManager>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR(config =>
            {
                config.EnableDetailedErrors = true;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
            });
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSignalR(routes =>
            {
                routes.MapHub<RoomHub>("/roomHub");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
