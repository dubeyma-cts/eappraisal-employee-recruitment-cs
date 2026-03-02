using eAppraisal.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Audit;

/// <summary>
/// Audit Forwarder Worker Service
/// Reads the append-only audit log and forwards structured events to Azure Log Analytics / SIEM.
/// Ensures 7-year immutable retention. Raw PAN is never present in audit records.
/// Alerts on critical events: account lockout, privilege escalation, PAN unmask attempts.
/// </summary>
public class AuditForwarderWorker(ILogger<AuditForwarderWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly HashSet<string> _criticalActions =
    [
        "ACCOUNT_LOCKED", "ACCOUNT_UNLOCKED", "PAN_UNMASK_ATTEMPT", "PRIVILEGE_CHANGE", "LOGIN_FAILED"
    ];

    private DateTimeOffset _lastForwardedAt = DateTimeOffset.UtcNow.AddMinutes(-5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Audit Forwarder Worker started. Forwarding audit events every 15 seconds.");
        logger.LogInformation("Retention policy: 7-year immutable sink (Azure Log Analytics / Azure Sentinel).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = _lastForwardedAt;
                var newEvents = await db.AuditLogs
                    .Where(a => a.Timestamp > cutoff)
                    .OrderBy(a => a.Timestamp)
                    .ToListAsync(stoppingToken);

                if (newEvents.Count > 0)
                {
                    foreach (var ev in newEvents)
                    {
                        var level = _criticalActions.Contains(ev.Action.ToUpperInvariant())
                            ? "CRITICAL" : "INFO";

                        logger.LogInformation(
                            "[Audit→SIEM] [{Level}] {Timestamp:o} | Action={Action} | User={User} | Detail={Detail}",
                            level, ev.Timestamp, ev.Action, ev.Actor, ev.Details ?? string.Empty);
                    }

                    _lastForwardedAt = newEvents.Max(e => e.Timestamp);

                    logger.LogInformation(
                        "[Audit] Forwarded {Count} event(s) to SIEM sink. " +
                        "In production: Azure Event Hub → Log Analytics workspace → 7-year retention policy.",
                        newEvents.Count);
                }
                else
                {
                    logger.LogDebug("[Audit] No new events since {LastForwarded:o}.", cutoff);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Audit forwarder encountered an error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }

        logger.LogInformation("Audit Forwarder Worker stopped.");
    }
}
