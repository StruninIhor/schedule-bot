using Microsoft.Extensions.Options;
using ScheduleBot.Web.Configurations;
using System.Net.Http;
using Telegram.Bot;

namespace ScheduleBot.Web.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            Client = new TelegramBotClient(_config.BotToken);
        }

        public TelegramBotClient Client { get; }
    }
}
