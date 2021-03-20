using Microsoft.Extensions.Diagnostics.HealthChecks;
using ScheduleBot.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ScheduleBot.Web.HealthChecks
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly BotDbContext _context;

        public DbHealthCheck(BotDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return canConnect ? HealthCheckResult.Healthy("Ok") : HealthCheckResult.Unhealthy("Database unavailable");
        }
    }
}
