using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Web.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        Task<HttpResponseMessage> UnpinChatMessageAsync(ChatId chatId, long messageId);
    }
}
