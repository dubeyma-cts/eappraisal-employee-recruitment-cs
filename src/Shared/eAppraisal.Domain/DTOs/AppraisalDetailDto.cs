namespace eAppraisal.Domain.DTOs;

public class AppraisalDetailDto
{
    // Appraisal info
    public int AppraisalId { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalisedAt { get; set; }

    // Cycle info
    public string? CycleName { get; set; }

    // Employee info
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public DateTime DateOfJoining { get; set; }

    // Manager info
    public int ManagerEmployeeId { get; set; }
    public string? ManagerName { get; set; }

    // Manager comment
    public string? Achievements { get; set; }
    public string? Gaps { get; set; }
    public string? Suggestions { get; set; }
    public bool IsCommentLocked { get; set; }

    // Employee feedback
    public string? FeedbackText { get; set; }
    public string? SelfAssessment { get; set; }

    // CTC snapshot
    public bool IsPromoted { get; set; }
    public decimal Basic { get; set; }
    public decimal DA { get; set; }
    public decimal HRA { get; set; }
    public decimal FoodAllowance { get; set; }
    public decimal PF { get; set; }
    public decimal TotalCTC { get; set; }
    public DateTime? NextAppraisalDate { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
