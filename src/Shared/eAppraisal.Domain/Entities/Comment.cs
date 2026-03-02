using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public int AppraisalId { get; set; }

    [Required, StringLength(2000)]
    public string Achievements { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Gaps { get; set; }

    [StringLength(2000)]
    public string? Suggestions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When true the comment is immutable (set after appraisal finalization).
    /// </summary>
    public bool IsLocked { get; set; }

    // Navigation
    [ForeignKey("AppraisalId")]
    public Appraisal? Appraisal { get; set; }
}
