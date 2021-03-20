using Microsoft.EntityFrameworkCore;
using ScheduleBot.Data.Models;

namespace ScheduleBot.Data
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }
        public DbSet<Lesson> Lessons { get; set; }
    }
}
