using eAppraisal.Application.Contracts;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;
using System.Text.Json;

namespace eAppraisal.Infrastructure.CrossCutting;

public class AuditLogCollector : IAuditService
{
    private readonly IAppDbContext _db;

    public AuditLogCollector(IAppDbContext db) => _db = db;

    public async Task LogAsync(string entityType, int? entityId, string action, string? actorEmail, string? ip, string? correlationId, object? details = null)
    {
        var audit = new AuditEvent
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ActorEmail = actorEmail,
            IpAddress = ip,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            DetailsJson = details != null ? JsonSerializer.Serialize(details) : null,
            At = DateTime.UtcNow
        };
        _db.AuditEvents.Add(audit);
        await _db.SaveChangesAsync();
    }
}
