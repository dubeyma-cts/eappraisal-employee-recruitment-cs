using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class EmployeeFeedback
{
    public int Id { get; set; }

    [Required]
    public int AppraisalId { get; set; }

    [Required, StringLength(2000)]
    public string FeedbackText { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? SelfAssessment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("AppraisalId")]
    public Appraisal? Appraisal { get; set; }
}
