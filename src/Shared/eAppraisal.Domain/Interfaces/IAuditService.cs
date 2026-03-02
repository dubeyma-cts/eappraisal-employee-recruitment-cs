namespace eAppraisal.Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string entityType,
        int? entityId,
        string action,
        string? actorEmail,
        string? ip,
        string? correlationId,
        object? details = null);
}
