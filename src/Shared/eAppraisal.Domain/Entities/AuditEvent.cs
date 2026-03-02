using System.ComponentModel.DataAnnotations;

namespace eAppraisal.Domain.Entities;

public class AuditEvent
{
    public int Id { get; set; }

    [StringLength(450)]
    public string? ActorUserId { get; set; }

    [StringLength(256)]
    public string? ActorEmail { get; set; }

    [Required, StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [Required, StringLength(100)]
    public string Action { get; set; } = string.Empty;

    public DateTime At { get; set; } = DateTime.UtcNow;

    [StringLength(45)]
    public string? IpAddress { get; set; }

    public string? DetailsJson { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
