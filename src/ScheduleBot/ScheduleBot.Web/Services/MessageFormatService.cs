using Microsoft.Extensions.Options;
using ScheduleBot.Data.Models;
using ScheduleBot.Web.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleBot.Web.Services
{
    public class MessageFormatService : IMessagesFormatService
    {
        public string FormatLesson(Lesson lesson)
        {
            var builder = new StringBuilder();
            AppendToBuilder(builder, lesson);
            return builder.ToString();
        }

        public string FormatLessons(Lesson [] lessons)
        {
            var builder = new StringBuilder();
            List<string> existingInfos = new List<string>();
            for (var i = 0; i < lessons.Length; i++)
            {
                existingInfos.Append(lessons[i].AdditionalInfo);
                builder.Append("*")
                    .Append(i + 1)
                    .Append("*. ");
                AppendToBuilder(builder, lessons[i], existingInfos);
                builder.Append("------\n");
            }
            return builder.ToString();
        }

        private void AppendToBuilder(StringBuilder builder, Lesson lesson, List<string> existingInfos = null)
        {
            builder.Append($"{lesson.TimeStart:hh\\:mm}-{lesson.TimeEnd:hh\\:mm}\n");
            builder
                .Append("*")
                .Append(lesson.Title)
                .Append("*")
                .Append('\n')
                .Append(lesson.TeacherName).Append('\n');
            if (!string.IsNullOrWhiteSpace(lesson.MeetingLink))
            {
                builder.Append("📞 ").Append($"[Meeting]({lesson.MeetingLink})").Append('\n');
            }
            if (!string.IsNullOrWhiteSpace(lesson.Chat))
            {
                builder.Append("💬 ");
                if (IsUrl(lesson.Chat))
                {
                    builder.Append($"[Chat]({lesson.Chat})");
                } 
                else
                {
                    builder.Append($"__{lesson.Chat}__");
                }
                builder.Append('\n');
            }
            if (!string.IsNullOrWhiteSpace(lesson.Email))
            {
                builder.Append("📧 ").Append(lesson.Email).Append('\n');
            }
            if (!string.IsNullOrWhiteSpace(lesson.AdditionalInfo))
            {
                if (existingInfos != null && existingInfos.Any(x => x == lesson.AdditionalInfo))
                {
                    return;
                } 
                builder.Append("ℹ️ ").Append(lesson.AdditionalInfo).Append('\n');
            }
        }

        private bool IsUrl(string uriName) => Uri.TryCreate(uriName, UriKind.Absolute, out var uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
