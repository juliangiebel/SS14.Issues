using Humanizer;
using Octokit;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SS14.Issues.Data;
using SS14.Issues.Helpers;

namespace SS14.Issues.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class GhWebhookController : ControllerBase
    {
        private const string GithubEventHeader = "x-github-event";
        
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        
        public GhWebhookController(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        // POST: api/GhWebhook
        [HttpPost]
        public async void Post()
        {
            if (!Request.Headers.TryGetValue(GithubEventHeader, out var eventName) || !await GithubWebhookHelper.VerifyWebhook(Request, _configuration))
                return;

            var json = await GithubWebhookHelper.RetrievePayload(Request);
            var serializer = new Octokit.Internal.SimpleJsonSerializer();
            
            switch (eventName)
            {
                case "issues":
                    HandleIssueEvent(serializer.Deserialize<IssueEventPayload>(json));
                    break;
            }
        }
        
        public void HandleIssueEvent(IssueEventPayload issueEvent)
        {
            try
            {
                var repoConfigs = _dbContext.RepoConfigs.Where(c => c.GhRepoId == issueEvent.Repository.Id).ToList();
                var repoConfig = repoConfigs.First();
                var issue = issueEvent.Issue;

                var excerpt = issue.Body.Truncate(200, "...");
                
                _dbContext.InsertIntoIssueTable(repoConfig.GhRepoSearchKey, issue.Id, issue.Number, issue.Title,
                    issue.HtmlUrl, issue.State.StringValue, string.IsNullOrWhiteSpace(excerpt) ? "<!--empty-->" : issue.Body.Truncate(200, "..."), repoConfig.Id);
            }
            catch (InvalidOperationException e)
            {
                Log.Error(e, "Error while handling issue event. Issue number: {IssueNumber} Repository name: {RepositoryName}",
                    issueEvent.Issue.Number, issueEvent.Repository.FullName);
            }
        }
    }
}
