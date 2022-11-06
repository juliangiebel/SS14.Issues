using Quartz;
using SS14.Issues.Services;

namespace SS14.Issues.Jobs;

[CronSchedule("0 0 2 * * ?", "SyncIssues", "sync")]
public sealed class SyncIssues : IJob
{
    private readonly IssueSyncService _issueSyncService;

    public SyncIssues(IssueSyncService issueSyncService)
    {
        _issueSyncService = issueSyncService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _issueSyncService.SyncAllIssues();
    }
}