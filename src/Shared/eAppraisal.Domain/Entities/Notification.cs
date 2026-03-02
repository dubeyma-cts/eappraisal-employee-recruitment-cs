using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    [StringLength(450)]
    public string? RecipientUserId { get; set; }

    [Required, StringLength(256)]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Topic { get; set; } = string.Empty;

    public string? PayloadJson { get; set; }

    /// <summary>
    /// Status values: Queued, Sent, Failed.
    /// </summary>
    [Required, StringLength(20)]
    public string Status { get; set; } = "Queued";

    public int? AboutAppraisalId { get; set; }

    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }
}
