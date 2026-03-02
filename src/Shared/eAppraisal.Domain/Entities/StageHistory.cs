using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eAppraisal.Domain.Entities;

public class StageHistory
{
    public int Id { get; set; }

    [Required]
    public int AppraisalId { get; set; }

    [Required, StringLength(30)]
    public string FromStage { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string ToStage { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Reason { get; set; }

    // Navigation
    [ForeignKey("AppraisalId")]
    public Appraisal? Appraisal { get; set; }
}
