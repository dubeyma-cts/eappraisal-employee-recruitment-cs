namespace eAppraisal.Shared.Models;

public enum AppraisalStatus
{
    Draft                    = 0,
    AwaitingManagerComment   = 1,
    AwaitingEmployeeInput    = 2,
    AwaitingFinalAssessment  = 3,
    Completed                = 4
}

public class Appraisal
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ManagerId { get; set; }
    public int Year { get; set; }
    public AppraisalStatus Status { get; set; } = AppraisalStatus.Draft;

    public string? ManagerComments { get; set; }
    public string? SelfAssessmentInput { get; set; }
    public string? FinalAssessment { get; set; }
    public int? Rating { get; set; }   // 1-5

    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ManagerCommentAt { get; set; }
    public DateTime? EmployeeInputAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InitiatedByHR { get; set; } = string.Empty;

    public Employee? Employee { get; set; }
    public Employee? Manager { get; set; }
}
