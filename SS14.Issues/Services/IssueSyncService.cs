using Humanizer;
using SS14.Issues.Data;

namespace SS14.Issues.Services;

public sealed class IssueSyncService
{
    private readonly GithubApiService _githubApiService;
    private readonly ApplicationDbContext _context;
    
    public IssueSyncService(GithubApiService githubApiService, ApplicationDbContext context)
    {
        _githubApiService = githubApiService;
        _context = context;
    }

    public async Task SyncAllIssues()
    {
        var repoConfigs = _context.RepoConfigs.ToList();
        
        foreach (var repoConfig in repoConfigs)
        {
            await IssueSyncTask(repoConfig.GhInstallationId, repoConfig.GhRepoSearchKey, repoConfig.Id);
        }
    }

    public async Task IssueSyncTask(int installationId, string repoSearchKey, Guid repoConfigId)
    {
        //create temporary issue table for repository
        var syncTableName = _context.CreateIssueSyncTable(repoSearchKey);

        //populate temporary issue table
        var enumerator = _githubApiService.IterateIssues(installationId, repoSearchKey);
        await foreach (var issue in enumerator)
        {
            var excerpt = issue.Body.Truncate(200, "...");
            _context.InsertIntoIssueSyncTable(syncTableName, issue.Id, issue.Number, issue.Title, issue.HtmlUrl, issue.State.StringValue,
                string.IsNullOrWhiteSpace(excerpt) ? "<!--empty-->" : excerpt, repoConfigId);
        }
        
        //swap original issue table with temporary and drop the original one
        _context.SwapIssueSyncTable(repoSearchKey);
    }
}