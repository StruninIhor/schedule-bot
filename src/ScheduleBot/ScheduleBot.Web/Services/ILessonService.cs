using ScheduleBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScheduleBot.Web.Services
{
    public interface ILessonService
    {
        public IAsyncEnumerable<Lesson> GetLessonsForDate(DateTime? date = null);
    }
}
