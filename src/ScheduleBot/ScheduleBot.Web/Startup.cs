using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.OpenApi.Models;
using ScheduleBot.Data;
using ScheduleBot.Web.Configurations;
using ScheduleBot.Web.Extensions;
using ScheduleBot.Web.HealthChecks;
using ScheduleBot.Web.Services;
using ScheduleBot.Web.Services.ScheduledTasks;
using System;

namespace ScheduleBot.Web
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                  .AddEnvironmentVariables()
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                  .Build();
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHttpClient();
            //services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            services.AddSingleton<IBotService, BotService>();
            services.AddScoped<ILessonService, LessonService>()
                .AddSingleton<IMessagesFormatService, MessageFormatService>();
            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
            services.Configure<BotMessageConfiguration>(Configuration.GetSection("BotMessageConfiguration"));
            services.Configure<BotChatConfiguration>(Configuration.GetSection("BotChatConfiguration"));
            services.AddCronJob<SendDayScheduleJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = Configuration["ScheduleSendInterval"];
            });
            services.AddDbContext<BotDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("BotDb"), builder =>
                {
                    builder.EnableRetryOnFailure(3);
                });
                if (Env.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            services.AddHealthChecks()
                .AddCheck<DbHealthCheck>("database");

            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ScheduleBot.Web", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScheduleBot.Web v1"));
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            })
            .UsePathBase(Configuration["Nginx:PathBase"]);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}
