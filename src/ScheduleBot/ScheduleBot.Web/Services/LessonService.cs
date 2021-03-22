using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScheduleBot.Data;
using ScheduleBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleBot.Web.Services
{
    public class LessonService : ILessonService
    {
        private readonly BotDbContext  _context;
        private readonly ILogger<LessonService> _logger;
        public LessonService(BotDbContext context, ILogger<LessonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IAsyncEnumerable<Lesson> GetLessonsForDate(DateTime? nullableDate = null)
        {
            DateTime date = nullableDate ?? DateTime.Now;
            var query = GetQueryForDate(date);
            query = query.Where(x => x.TimeStart < date.TimeOfDay &&
                    x.TimeEnd > date.TimeOfDay);
            return query.AsAsyncEnumerable();
        }

        public async Task<Lesson[]> GetLessonsForDay(CancellationToken cancellationToken = default, DateTime? nullableDate = null)
        {
            DateTime date = nullableDate ?? DateTime.Now;
            var query = GetQueryForDate(date);
            return await query.ToArrayAsync().ConfigureAwait(false);
        }

        private IQueryable<Lesson> GetQueryForDate(DateTime date)
        {
            int weekNumber = GetIso8601WeekOfYear(date);
            bool isOddWeek = weekNumber % 2 == 0;
            _logger.LogInformation("Requesting lessons for {requestDate}, week number is {weekNumber}, is odd: {isOddWeek}",
                date, weekNumber, isOddWeek);
            return _context.Lessons
                .AsNoTracking()
                .OrderBy(x => x.TimeStart)
                .Where(x => x.DayOfWeek == date.DayOfWeek &&
                    (x.CustomRecurrency == CustomRecurrency.EachWeek ||
                    (isOddWeek && x.CustomRecurrency == CustomRecurrency.OddWeek) ||
                    (!isOddWeek && x.CustomRecurrency == CustomRecurrency.EvenWeek))
                    );
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
