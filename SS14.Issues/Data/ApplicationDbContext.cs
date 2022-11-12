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

        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetIssue), new[] { typeof(string), typeof(int) })!)
            .HasName("getissue");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(GetIssues), new[] { typeof(string) })!)
            .HasName("getissues");
        
        modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(SearchIssues), new[] { typeof(string), typeof(string), typeof(int) })!)
            .HasName("searchissues");
    }
    
    public void CreateIssueSyncTable(string repoKey)
        => Database.ExecuteSqlInterpolated($"SELECT createissuesynctable({repoKey})");
    
    public void InsertIntoIssueSyncTable(string repoKey, int id, int number, string title, string url, string status, string excerpt, Guid repoConfigId)
        => Database.ExecuteSqlInterpolated($"SELECT insertintoissuesynctable({repoKey}, {id}, {number}, {title}, {url}, {status}, {excerpt}, {repoConfigId})");
    
    public void SwapIssueSyncTable(string repoKey)
        => Database.ExecuteSqlInterpolated($"SELECT swapissuesynctable({repoKey})");
    
    public void CreateIssueTable(string repoKey)
        => Database.ExecuteSqlInterpolated($"SELECT createissuetable({repoKey})");

    public void InsertIntoIssueTable(string repoKey, int id, int number, string title, string url, string status, string excerpt, Guid repoConfigId)
        => Database.ExecuteSqlInterpolated($"SELECT insertintoissuetable({repoKey}, {id}, {number}, {title}, {url}, {status}, {excerpt}, {repoConfigId})");

    public IQueryable<SyncedIssue> GetIssue(string repoKey, int issueId)
        => FromExpression(() => GetIssue(repoKey, issueId));
    
    public IQueryable<SyncedIssue> GetIssues(string repoKey)
        => FromExpression(() => GetIssues(repoKey));

    public IQueryable<SyncedIssue> SearchIssues(string repoKey, string term, int limit)
        => FromExpression(() => SearchIssues(repoKey, term, limit));
}