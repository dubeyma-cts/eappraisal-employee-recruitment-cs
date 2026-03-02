using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.DTOs;

public class FinalizeAppraisalDto
{
    [Required]
    public int AppraisalId { get; set; }

    public bool IsPromoted { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal Basic { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal DA { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal HRA { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal FoodAllowance { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal PF { get; set; }

    public DateTime? NextAppraisalDate { get; set; }
}
