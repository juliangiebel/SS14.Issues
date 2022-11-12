
using System.Diagnostics;
using Octokit;
using Serilog;
using SS14.Issues.Data.Model;
using SS14.Issues.Exceptions;
using SS14.Issues.Helpers;

namespace SS14.Issues.Services;

public sealed class GithubApiService
{
    private readonly IConfiguration _configuration;

    private readonly GithubAppApiClientStore _clientStore;
    private readonly IssueRateLimiterService _rateLimiter;
    
    public GithubApiService(IConfiguration configuration, IssueRateLimiterService rateLimiter)
    {
        _configuration = configuration;
        _rateLimiter = rateLimiter;

        var productHeader = _configuration["Github:AppName"];
        if (productHeader == null)
            throw new ConfigurationException("The github app name needs to be configured. [Github:AppName]");
        
        var keyLocation = _configuration["Github:AppPrivateKeyLocation"];
        if (keyLocation == null)
            throw new ConfigurationException("The github private key location needs to be configured. [Github:AppPrivateKeyLocation]");

        if (_configuration["Github:AppId"] == null)
            throw new ConfigurationException("The github app id needs to be configured. [Github:AppId]");
        
        var appId = _configuration.GetSection("Github").GetValue<int>("AppId");

        _clientStore = new GithubAppApiClientStore(productHeader, keyLocation, appId);
    }

    /// <summary>
    /// Gets a list of all installations of this github app
    /// </summary>
    /// <remarks>
    /// Used for selecting which installation to configure in the administration for example.
    /// Refrain from iterating over all installations and creating clients for them. Use installation ids saved in the database instead.
    /// </remarks>
    /// <returns>List of installations</returns>
    public async Task<IReadOnlyList<Installation>> GetInstallations()
    {
        return await _clientStore.AppClient.GitHubApps.GetAllInstallationsForCurrent();
    }

    /// <summary>
    /// Gets a list of repositories the installation with the given id has access to.
    /// </summary>
    /// <remarks>
    /// Used for configuration in the administration interface
    /// </remarks>
    /// <param name="installationId">The installation id</param>
    /// <returns>A list of repositories the app has access to</returns>
    public async Task<RepositoriesResponse> GetRepositories(long installationId)
    {
        var client = await _clientStore.GetInstallationClient(installationId);
        return await client.GitHubApps.Installation.GetAllRepositoriesForCurrent();
    }

    /// <summary>
    /// Gets a paginated list of issues for the given repository
    /// </summary>
    /// <param name="installationId">The installation id</param>
    /// <param name="repositorySearchKey">The organization or account name plus the repository name in the following format: org/repo</param>
    /// <param name="page">The current page</param>
    /// <param name="perPage">The maximum amount of items per page</param>
    /// <returns></returns>
    public async Task<SearchIssuesResult> GetIssuesPaginated(long installationId, string repositorySearchKey, int page, int perPage, DateTimeOffset from)
    {
        var client = await _clientStore.GetInstallationClient(installationId);
        return await client.Search.SearchIssues(new SearchIssuesRequest()
        {
            Type = IssueTypeQualifier.Issue,
            Repos = new RepositoryCollection { repositorySearchKey },
            Page = page,
            PerPage = Math.Clamp(perPage, 0, 100),
            SortField = IssueSearchSort.Created,
            Created = new DateRange(from, SearchQualifierOperator.LessThan)
        });
    }

    public async IAsyncEnumerable<Issue> IterateIssues(long installationId, string repositorySearchKey)
    {
        var client = await _clientStore.GetInstallationClient(installationId);
        var page = 0;
        var from = DateTimeOffset.Now;
        
        Log.Information("Syncing issues for repository: {Repository}", repositorySearchKey);
        Log.Information("Syncing issues from date: {From}", from);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (true)
        {
            var issues = await GetIssuesPaginated(installationId, repositorySearchKey, page, 100, from);
            var links = client.GetLastApiInfo().Links;
            
            foreach (var issue in issues.Items)
                yield return issue;

            if (!links.ContainsKey("next"))
            {
                if (issues.TotalCount == 0 || issues.Items.Count == 0)
                    break;
                
                from = issues.Items[^1].CreatedAt;
                page = -1;
                
                Log.Information("Syncing issues from date: {From}", from);
            }
                
            page++;
            
            var limit = client.GetLastApiInfo().RateLimit;
            if (limit.Remaining <= 1)
            {
                var waitTime = limit.Reset - DateTimeOffset.Now + TimeSpan.FromSeconds(10);
                Log.Information("Hit rate limit. Waiting for: {WaitTime}", waitTime);
                await Task.Delay(waitTime);
            }
        }
        stopwatch.Stop();
        Log.Information("Finished syncing issues. Time taken: {Elapsed}", stopwatch.Elapsed);
    }
    
    public async void CreateIssue(long installationId, long repositoryId, IssueCreationParameters issueParameters)
    {
        if (!await _rateLimiter.Acquire(repositoryId))
            throw new RateLimitException($"Hit rate limit for creating issues for repository with id: {repositoryId}");
        

        
        var issue = new NewIssue(issueParameters.Title)
        {
            Body = issueParameters.Message
        };

        var client = await _clientStore.GetInstallationClient(installationId);
        await client.Issue.Create(repositoryId, issue);
    }
}