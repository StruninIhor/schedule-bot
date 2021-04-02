using Cronos;
using System;

namespace ScheduleBot.Web.Services.ScheduledTasks.Abstractions
{
    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
        public CronFormat CronFormat { get; set; }
    }
}
