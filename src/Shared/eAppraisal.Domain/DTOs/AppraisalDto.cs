namespace eAppraisal.Domain.DTOs;

public class AppraisalDto
{
    public int AppraisalId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Department { get; set; }
    public string? ManagerName { get; set; }
    public string? CycleName { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalisedAt { get; set; }
}
