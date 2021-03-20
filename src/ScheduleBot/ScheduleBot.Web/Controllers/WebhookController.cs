using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScheduleBot.Data;
using ScheduleBot.Data.Models;
using ScheduleBot.Web.Configurations;
using ScheduleBot.Web.Services;
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

        public WebhookController(BotDbContext context, IBotService botService, ILessonService lessonService, IOptions<BotMessageConfiguration> botMessageConfiguration)
            => (_context, _botService, _lessonService, _botMessageConfiguration) = (context, botService, lessonService, botMessageConfiguration.Value);

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Type == UpdateType.Message)
            {
               if (update.Message.Chat.Type == ChatType.Private)
               {
                    await ProcessPrivateMessage(update);
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
            if (update.Message.Text == _botMessageConfiguration.LessonsCommand)
            {
                await SendLessons(update);
            }
            else
            {
                await _botService.Client.SendTextMessageAsync(update.Message.From.Id, "I can only send current lesson for now", replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton[] { new KeyboardButton(_botMessageConfiguration.LessonsCommand) }, true));
            }
        }

        private async Task ProcessAdmin(Update update)
        {
            if (update.Message.Text == "/all")
            {
                var lessons = await _context.Lessons
                    .AsNoTracking()
                    .OrderBy(x => x.DayOfWeek)
                    .ThenBy(x => x.TimeStart)
                    .ToArrayAsync(HttpContext.RequestAborted);
                foreach (var lesson in lessons)
                {
                    await _botService.Client.SendTextMessageAsync(update.Message.From.Id, FormatMessage(lesson), disableWebPagePreview: true);
                }
            }
        }
        private async Task SendLessons(Update update)
        {
            bool anyLesson = false;
            await foreach (var lesson in _lessonService.GetLessonsForDate())
            {
                anyLesson = true;
                await _botService.Client.SendTextMessageAsync(update.Message.From.Id, FormatMessage(lesson))
                    .ConfigureAwait(false);
            }
            if (!anyLesson)
            {
                await _botService.Client.SendTextMessageAsync(update.Message.From.Id, "No lessons now")
                    .ConfigureAwait(false);
            }
        }
        private static string FormatMessage(object obj) => JsonConvert.SerializeObject(obj, Formatting.Indented, new StringEnumConverter());
    }
}
