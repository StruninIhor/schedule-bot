using Telegram.Bot;

namespace ScheduleBot.Web.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}
