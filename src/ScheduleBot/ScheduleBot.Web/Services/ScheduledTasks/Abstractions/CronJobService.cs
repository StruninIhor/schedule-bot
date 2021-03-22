using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleBot.Web.Services.ScheduledTasks.Abstractions
{
    public abstract class CronJobService : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly string _cronExpression;
        private readonly CronExpression _expression;
        private readonly ILogger<CronJobService> _logger;
        private readonly TimeZoneInfo _timeZoneInfo;
        public string Name => GetType().Name;
        protected CronJobService(ILogger<CronJobService> logger, string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            _cronExpression = cronExpression;
            _expression = CronExpression.Parse(cronExpression);
            _logger = logger;
            _timeZoneInfo = timeZoneInfo;
        }

        public Task StartAsync(CancellationToken cancellationToken) => ScheduleJob(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing scheduled task {taskName} with recurrence {recurrence}", Name, _cronExpression);
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            _logger.LogInformation("Next occurence is {nextOccurence}", next);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                _logger.LogInformation("The delay for given occurence is {taskDelay}", delay);
                if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                {
                    await ScheduleJob(cancellationToken);
                }
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _logger.LogInformation("Task {taskName} execution", Name);
                    _timer.Dispose();  // reset and dispose timer
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ExecuteAsync(cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleJob(cancellationToken);    // reschedule next
                    }
                };
                _timer.Start();
            }
            await Task.CompletedTask;
        }
        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
