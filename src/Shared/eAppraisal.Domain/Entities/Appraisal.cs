using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class Appraisal
{
    public int Id { get; set; }

    [Required]
    public int CycleId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int ManagerEmployeeId { get; set; }

    /// <summary>
    /// Status values: Draft, ManagerCommented, EmployeeFeedback, Final.
    /// </summary>
    [Required, StringLength(30)]
    public string Status { get; set; } = "Draft";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? NextAppraisalDate { get; set; }

    public DateTime? FinalisedAt { get; set; }

    // Navigation properties
    [ForeignKey("CycleId")]
    public AppraisalCycle? Cycle { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee? Employee { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    public Employee? ManagerEmployee { get; set; }

    public Comment? ManagerComment { get; set; }

    public EmployeeFeedback? EmployeeFeedback { get; set; }

    public CtcSnapshot? CtcSnapshot { get; set; }

    public ICollection<StageHistory> StageHistories { get; set; } = new List<StageHistory>();
}
