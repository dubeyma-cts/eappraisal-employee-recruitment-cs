namespace eAppraisal.Shared.Models;

public class AuditLog
{
    public long Id { get; set; }
    public string Actor { get; set; } = string.Empty;   // username (never raw PAN)
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  // e.g. LOGIN, APPRAISAL_SUBMIT
    public string Details { get; set; } = string.Empty; // masked / non-sensitive only
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
