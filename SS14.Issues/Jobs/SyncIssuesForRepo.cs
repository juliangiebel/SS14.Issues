using Quartz;
using SS14.Issues.Data;
using SS14.Issues.Services;

namespace SS14.Issues.Jobs;

public class SyncIssuesForRepo : IJob
{
    public const string ConfigIdKey = "configId";
    
    private readonly IssueSyncService _issueSyncService;
    private readonly ApplicationDbContext _dbContext;

    public SyncIssuesForRepo(IssueSyncService issueSyncService, ApplicationDbContext dbContext)
    {
        _issueSyncService = issueSyncService;
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var configId = dataMap.GetGuidValue(ConfigIdKey);

        var config = await _dbContext.RepoConfigs.FindAsync(configId);

        if (config is not null)
            await _issueSyncService.IssueSyncTask(config.GhInstallationId, config.GhRepoSearchKey, configId);
    }
}