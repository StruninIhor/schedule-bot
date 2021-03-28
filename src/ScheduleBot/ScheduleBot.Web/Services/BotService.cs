using Microsoft.Extensions.Options;
using ScheduleBot.Web.Configurations;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Web.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;
        HttpClient _client;
        public BotService(IOptions<BotConfiguration> config, IHttpClientFactory httpClientFactory)
        {
            _config = config.Value;
            _client = httpClientFactory.CreateClient();
            Client = new TelegramBotClient(_config.BotToken, _client);
        }

        public TelegramBotClient Client { get; }

        public Task<HttpResponseMessage> UnpinChatMessageAsync(ChatId chatId, long messageId)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, 
                $"https://api.telegram.org/bot{_config.BotToken}/unpinChatMessage?chat_id={chatId}&message_id={messageId}");
            return _client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}
