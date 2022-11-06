using Quartz;
using SS14.Issues.Services;

namespace SS14.Issues.Jobs;

[CronSchedule("0 0 * * * ?", "ClearStaleLimiters", "maintenance")]
public class ClearStaleLimiters : IJob
{
    private readonly IssueRateLimiterService _rateLimiter;

    public ClearStaleLimiters(IssueRateLimiterService rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _rateLimiter.ClearStaleLimiters();
    }
}