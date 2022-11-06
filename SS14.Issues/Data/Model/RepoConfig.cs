using System.ComponentModel.DataAnnotations;

namespace SS14.Issues.Data.Model;

public class RepoConfig
{
    public Guid Id { get; set; }
    
    [Required] public string Name { get; set; }
    
    [Required] public int GhInstallationId { get; set; }
    
    [Required] public int GhRepoId { get; set; }
    
    [Required] public string GhRepoSearchKey { get; set; }
    
    [Required] public string Slug { get; set; }
    
    [Required] public bool Active { get; set; } = true;
    
    [Required] public bool AuthRequired { get; set; }
}