using System.ComponentModel.DataAnnotations;

namespace SS14.Issues.Data.Model;

public class SyncedIssue
{
    public int Id { get; set; }
    [Required]
    public int Number { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Url { get; set; }
    [Required]
    public string Status { get; set; }
    [Required]
    public string? Excerpt { get; set; }
    [Required]
    public Guid RepoConfigId { get; set; }

    public RepoConfig RepoConfig { get; set; }
}