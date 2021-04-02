using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScheduleBot.Data;
using ScheduleBot.Data.Models;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public SendDayScheduleJob(
            IScheduleConfig<SendDayScheduleJob> config, 
            ILogger<SendDayScheduleJob> logger,
            IBotService botService,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, config.CronExpression, config.TimeZoneInfo, config.CronFormat)
        {
            _logger = logger;
            _botService = botService;
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
                var todaysLessons = await lessonService.GetLessonsForDay(cancellationToken, DateTime.Now).ConfigureAwait(false);
                var messagesFormatService = scope.ServiceProvider.GetService<IMessagesFormatService>();
                var messageText = messagesFormatService.FormatLessons(todaysLessons);
                if (string.IsNullOrWhiteSpace(messageText))
                {
                    messageText = scope.ServiceProvider.GetService<IOptionsSnapshot<BotMessageConfiguration>>().Value.TodayNoLessonsMessage;
                }
                var message = await _botService.Client.SendTextMessageAsync(configuration.ChatId, messageText, cancellationToken: cancellationToken, parseMode: ParseMode.Markdown, disableWebPagePreview: true)
                    .ConfigureAwait(false);
                if (configuration.PinMessage)
                {
                    var context = scope.ServiceProvider.GetService<BotDbContext>();
                    const string pinnedMessageStateKey = "PinnedMessageId";
                    var state = await context.States.FirstOrDefaultAsync(x => x.Key == pinnedMessageStateKey);
                    if (state != null)
                    {
                        var oldPinnedMesageId = BotState.GetValue<long>(state);
                        state.Value = message.MessageId.ToString();
                        try
                        {
                            var response = await _botService.UnpinChatMessageAsync(configuration.ChatId, oldPinnedMesageId);
                            if (!response.IsSuccessStatusCode)
                            {
                                throw new Exception(response.ReasonPhrase);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error whle unpinning message");
                        }
                    }
                    else
                    {
                        state = BotState.CreateBotState<long>(pinnedMessageStateKey, message.MessageId);
                        context.States.Add(state);
                    }
                    foreach (var lesson in todaysLessons)
                    {
                        if (lesson.IsCanceledOnce)
                        {
                            lesson.IsCanceledOnce = false;
                        }
                        if (context.Entry(lesson).State == EntityState.Detached)
                        {
                            context.Attach(lesson);
                        }
                        context.Entry(lesson).State = EntityState.Modified;
                    }
                    await context.SaveChangesAsync();
                    await _botService.Client
                        .PinChatMessageAsync(configuration.ChatId, message.MessageId)
                        .ConfigureAwait(false);
                }
            } 
            else
            {
                _logger.LogInformation("Chat configuration is null, skipping");
            }
            
        }
    }
}
