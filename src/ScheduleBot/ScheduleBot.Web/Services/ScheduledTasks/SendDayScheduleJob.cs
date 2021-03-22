using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduleBot.Web.Configurations;
using ScheduleBot.Web.Services.ScheduledTasks.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace ScheduleBot.Web.Services.ScheduledTasks
{
    public class SendDayScheduleJob : CronJobService
    {
        private readonly ILogger<SendDayScheduleJob> _logger;
        private readonly IBotService _botService;
        private readonly IMessagesFormatService _messagesFormatService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public SendDayScheduleJob(
            IScheduleConfig<SendDayScheduleJob> config, 
            ILogger<SendDayScheduleJob> logger,
            IBotService botService,
            IMessagesFormatService messagesFormatService,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _botService = botService;
            _messagesFormatService = messagesFormatService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing sheduled \"schedule message\" sending task");
            using var scope = _serviceScopeFactory.CreateScope();
            var chatOptions = scope.ServiceProvider.GetService<IOptionsSnapshot<BotChatConfiguration>>();
            if (chatOptions.Value != null)
            {
                var lessonService = scope.ServiceProvider.GetService<ILessonService>();
                var configuration = chatOptions.Value;
                var todaysLessons = await lessonService.GetLessonsForDay(cancellationToken, DateTime.Now);
                var messageText = _messagesFormatService.FormatLessons(todaysLessons);
                if (string.IsNullOrWhiteSpace(messageText))
                {
                    messageText = scope.ServiceProvider.GetService<IOptionsSnapshot<BotMessageConfiguration>>().Value.TodayNoLessonsMessage;
                }
                var message = await _botService.Client.SendTextMessageAsync(configuration.ChatId, messageText, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown);
                if (configuration.PinMessage)
                {
                    await _botService.Client.PinChatMessageAsync(configuration.ChatId, message.MessageId);
                }
            } 
            else
            {
                _logger.LogInformation("Chat configuration is null, skipping");
            }
            
        }
    }
}
