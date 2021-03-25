using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScheduleBot.Data;
using ScheduleBot.Web.Configurations;
using ScheduleBot.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly BotDbContext _context;
        private readonly IBotService _botService;
        private readonly ILessonService _lessonService;
        private readonly BotMessageConfiguration _botMessageConfiguration;
        private readonly IMessagesFormatService _messagesFormatService;
        private readonly IReplyMarkup _replyMarkup;
        private readonly ILogger<WebhookController> _logger;
        public WebhookController(BotDbContext context,
            IBotService botService,
            ILessonService lessonService,
            IOptions<BotMessageConfiguration> botMessageConfiguration,
            IMessagesFormatService messagesFormatService,
            ILogger<WebhookController> logger)
        {
            _context = context;
            _botService = botService;
            _lessonService = lessonService;
            _botMessageConfiguration = botMessageConfiguration.Value;
            _messagesFormatService = messagesFormatService;
            _replyMarkup = new ReplyKeyboardMarkup(new KeyboardButton[][] { new KeyboardButton[] {
                new KeyboardButton(_botMessageConfiguration.NowLessonsCommand)
            },
            new KeyboardButton[]
            {
                new KeyboardButton(_botMessageConfiguration.TodaysScheduleCommand),
                new KeyboardButton(_botMessageConfiguration.TomorrowScheduleCommand)
            }
            }, resizeKeyboard: true);
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Type == UpdateType.Message)
            {
               if (update.Message.Chat.Type == ChatType.Private)
               {
                    await ProcessPrivateMessage(update);
               }
               else if (update.Message.From.Id == _botMessageConfiguration.AdminId && update.Message.Text == "/ping")
               {
                    _logger.LogWarning("PING, chatId : {chatId}", update.Message.Chat.Id);               
               }
            }
            return Ok();
        }

        private async Task ProcessPrivateMessage(Update update)
        {
            if (update.Message.From.Id == _botMessageConfiguration.AdminId)
            {
                await ProcessAdmin(update);
            }
            await ProcessCommands(update);
        }

        private async Task ProcessAdmin(Update update)
        {
            switch (update.Message.Text)
            {
                case "/all":
                    {
                        var lessons = await _context.Lessons
                           .AsNoTracking()
                           .OrderBy(x => x.DayOfWeek)
                           .ThenBy(x => x.TimeStart)
                           .ToArrayAsync(HttpContext.RequestAborted);
                            foreach (var lesson in lessons)
                            {
                                await _botService.Client.SendTextMessageAsync(update.Message.From.Id, _messagesFormatService.FormatLesson(lesson), ParseMode.Markdown, disableWebPagePreview: true);
                            }
                        break;
                    }
                case "/chatConfig":
                    {
                        var chatConfiguration = HttpContext.RequestServices.GetService(typeof(IOptions<BotChatConfiguration>)) as IOptions<BotChatConfiguration>;
                        await SendText(update, JsonConvert.SerializeObject(chatConfiguration?.Value));
                        break;
                    }       

            }
        }

        private Task ProcessCommands(Update update)
        {
            var messageText = update.Message.Text;
            if (messageText == "/start")
            {
                return SendText(update, _botMessageConfiguration.HelloMessage);
            }
            else if (messageText == _botMessageConfiguration.NowLessonsCommand)
            {
                return SendLessons(update);
            } else if (messageText == _botMessageConfiguration.TodaysScheduleCommand)
            {
                return SendSchedule(update, DateTime.Now, _botMessageConfiguration.TodayNoLessonsMessage);
            } else if (messageText == _botMessageConfiguration.TomorrowScheduleCommand)
            {
                return SendSchedule(update, DateTime.Now.AddDays(1), _botMessageConfiguration.TomorrowNoLessonsMessage);
            }
            return SendText(update, _botMessageConfiguration.NotUnderstandMessage);
        }
        private async Task SendLessons(Update update)
        {
            bool anyLesson = false;
            await foreach (var lesson in _lessonService.GetLessonsForDate())
            {
                anyLesson = true;
                await SendText(update, _messagesFormatService.FormatLesson(lesson));
            }
            if (!anyLesson)
            {
                await SendText(update, _botMessageConfiguration.NoLessonsNowMessage);
            }
        }
        private async Task SendSchedule(Update update, DateTime date, string noLessonsMessage)
        {
            var lessons = await _lessonService.GetLessonsForDay(HttpContext.RequestAborted, date);
            if (lessons.Length == 0)
            {
                await SendText(update, noLessonsMessage);
            }
            else {
                await SendText(update, _messagesFormatService.FormatLessons(lessons));
            }
        }

        private Task SendText(Update update, string text) => _botService.Client.SendTextMessageAsync(update.Message.From.Id, text, parseMode: ParseMode.Markdown, replyMarkup: _replyMarkup);

    }
}
