using System;

namespace ScheduleBot.Data.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }

        public string TeacherName { get; set; }
        public string MeetingLink { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public CustomRecurrency? CustomRecurrency { get; set; }
        public string Chat { get; set; }
        public string Email { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
