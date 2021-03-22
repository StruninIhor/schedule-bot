using ScheduleBot.Data.Models;
using System.Collections.Generic;

namespace ScheduleBot.Web.Services
{
    public interface IMessagesFormatService
    {
        public string FormatLesson(Lesson lesson);
        public string FormatLessons(Lesson[] lesson);
    }
}
