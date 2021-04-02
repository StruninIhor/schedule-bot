using Cronos;
using System;

namespace ScheduleBot.Web.Services.ScheduledTasks.Abstractions
{
    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
        public CronFormat CronFormat { get; set; }
    }

}
