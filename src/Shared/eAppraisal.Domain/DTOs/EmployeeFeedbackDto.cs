using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.DTOs;

public class EmployeeFeedbackDto
{
    [Required]
    public int AppraisalId { get; set; }

    [Required, StringLength(2000)]
    public string FeedbackText { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? SelfAssessment { get; set; }
}
