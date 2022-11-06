using System.ComponentModel.DataAnnotations;

namespace SS14.Issues.Data.Model;

public class IssueCreationParameters
{
    [Required]
    public string Title { get; set; }
    
    public string? Message { get; set; }
}