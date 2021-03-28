using Microsoft.EntityFrameworkCore;
using ScheduleBot.Data.Models;

namespace ScheduleBot.Data
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(BotState).Assembly);
            base.OnModelCreating(builder);
        }

        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<BotState> States { get; set; }
    }
}
