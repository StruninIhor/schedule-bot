namespace ScheduleBot.Web.Configurations
{
    public class BotMessageConfiguration
    {
        public int AdminId { get; set; }
        public string NoLessonsNowMessage { get; set; }
        public string CurrentLessonsMessage { get; set; }
        public string NowLessonsCommand { get; set; }
        public string TomorrowScheduleCommand { get; set; }
        public string TodaysScheduleCommand { get; set; }
        public string HelloMessage { get; set; }
        public string TodayNoLessonsMessage { get; set; }
        public string TomorrowNoLessonsMessage { get; set; }
        public string NotUnderstandMessage { get; set; }
    }
}