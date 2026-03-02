using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class CtcSnapshot
{
    public int Id { get; set; }

    [Required]
    public int AppraisalId { get; set; }

    public bool IsPromoted { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Basic { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DA { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HRA { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FoodAllowance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PF { get; set; }

    public DateTime? NextAppraisalDate { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [NotMapped]
    public decimal TotalCTC => Basic + DA + HRA + FoodAllowance + PF;

    // Navigation
    [ForeignKey("AppraisalId")]
    public Appraisal? Appraisal { get; set; }
}
