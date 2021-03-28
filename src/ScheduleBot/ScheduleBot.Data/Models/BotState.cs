using System;

namespace ScheduleBot.Data.Models
{
    public class BotState
    {
        public static BotState CreateBotState<T>(string key, T value)
            where T : struct => new BotState
            {
                Key = key,
                TypeName = value.GetType().FullName,
                Value = value.ToString()
            };
        public static T GetValue<T>(BotState state)
            where T : struct => (T)Convert.ChangeType(state.Value, Type.GetType(state.TypeName));
        public string Key { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
    }
}
