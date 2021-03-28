using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ScheduleBot.Data.Models.Configurations
{
    public class BotStateConfiguration : IEntityTypeConfiguration<BotState>
    {
        public void Configure(EntityTypeBuilder<BotState> builder)
        {
            builder.HasKey(x => x.Key);
        }
    }
}
