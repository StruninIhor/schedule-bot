using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScheduleBot.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScheduleBot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // 100 megabytes maximum log size
                .WriteTo.RollingFile("Logs/ScheduleBot-{Date}.log", retainedFileCountLimit: 7, fileSizeLimitBytes: 100*1024*1024)
                .CreateLogger();
            try
            {
                Log.Information("Application started");
                var host = CreateHostBuilder(args).Build();
                Log.Debug("Ensuring database is in last state");
                {
                    using var scope = host.Services.CreateScope();
                    using var context = scope.ServiceProvider.GetService<BotDbContext>();
                    context.Database.Migrate();
                }
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
