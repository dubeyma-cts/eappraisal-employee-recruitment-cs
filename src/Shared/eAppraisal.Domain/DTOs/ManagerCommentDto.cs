using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.DTOs;

public class ManagerCommentDto
{
    [Required]
    public int AppraisalId { get; set; }

    [Required, StringLength(2000)]
    public string Achievements { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Gaps { get; set; }

    [StringLength(2000)]
    public string? Suggestions { get; set; }
}
