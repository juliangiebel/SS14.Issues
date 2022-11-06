using Microsoft.EntityFrameworkCore;
using SS14.Issues.Data.Model;

namespace SS14.Issues.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<RepoConfig> RepoConfigs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SyncedIssue>()
            .ToTable("syncedissue", t => t.ExcludeFromMigrations());
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(CreateIssueSyncTable), new[] { typeof(string) })!)
            .HasName("createissuesynctable");

        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(InsertIntoIssueSyncTable), new[]
            { typeof(string), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Guid) })!)
            .HasName("insertintoissuesynctable");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(SwapIssueSyncTable), new[] { typeof(string) })!)
            .HasName("swapissuesynctable");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(CreateIssueTable), new[] { typeof(string) })!)
            .HasName("createissuetable");

        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(InsertIntoIssueTable), new[]
                { typeof(string), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Guid) })!)
            .HasName("insertintoissuetable");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetIssue), new[] { typeof(string), typeof(int) })!)
            .HasName("getissue");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetIssues), new[] { typeof(string) })!)
            .HasName("getissues");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(SearchIssues), new[] { typeof(string), typeof(string), typeof(int) })!)
            .HasName("searchissues");
    }
    
    public string CreateIssueSyncTable(string repoKey)
        => RepoConfigs.Select(c => CreateIssueSyncTable(repoKey)).Single();
    
    public int InsertIntoIssueSyncTable(string syncTableName, int id, int number, string title, string url, string status, string excerpt, Guid repoConfigId)
        => RepoConfigs.Select(c => InsertIntoIssueSyncTable(syncTableName, id, number, title, url, status, excerpt, repoConfigId)).Single();

    public int SwapIssueSyncTable(string repoKey)
        => RepoConfigs.Select(c => SwapIssueSyncTable(repoKey)).Single();
    
    public string CreateIssueTable(string repoKey)
        => RepoConfigs.Select(c => CreateIssueTable(repoKey)).Single();
    
    public int InsertIntoIssueTable(string repoKey, int id, int number, string title, string url, string status, string excerpt, Guid repoConfigId)
        => RepoConfigs.Select(c => InsertIntoIssueTable(repoKey, id, number, title, url, status, excerpt, repoConfigId)).Single();

    public IQueryable<SyncedIssue> GetIssue(string repoKey, int issueId)
        => FromExpression(() => GetIssue(repoKey, issueId));
    
    public IQueryable<SyncedIssue> GetIssues(string repoKey)
        => FromExpression(() => GetIssues(repoKey));

    public IQueryable<SyncedIssue> SearchIssues(string repoKey, string term, int limit)
        => FromExpression(() => SearchIssues(repoKey, term, limit));

}