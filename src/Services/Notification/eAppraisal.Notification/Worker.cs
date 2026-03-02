using eAppraisal.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Notification;

/// <summary>
/// Notification Worker Service – Outbox Pattern Consumer
/// Polls the outbox for pending notification events and delivers transactional emails
/// via SendGrid / Azure Communication Services with retry and DLQ on repeated failure.
/// Raw PAN is never included in email subjects or bodies.
/// </summary>
public class NotificationWorker(ILogger<NotificationWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly string[] _workflowMessages =
    [
        "Appraisal cycle initiated — notify manager to add comments.",
        "Manager comment submitted — notify employee to complete self-assessment.",
        "Employee self-assessment submitted — notify manager for final assessment.",
        "Final assessment completed — notify employee and HR.",
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification Worker started. Polling outbox every 20 seconds.");

        var msgIndex = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // In production: dequeue from outbox table / Azure Service Bus topic.
                // Here we count pending (non-completed) appraisals as a proxy.
                var pending = await db.Appraisals
                    .Where(a => a.Status != eAppraisal.Shared.Models.AppraisalStatus.Completed)
                    .CountAsync(stoppingToken);

                if (pending > 0)
                {
                    var demoMsg = _workflowMessages[msgIndex % _workflowMessages.Length];
                    logger.LogInformation(
                        "[Notify] {Time}: {Count} active appraisal(s). Demo event → \"{Message}\" " +
                        "In production: send email via SendGrid/ACS — no raw PAN in body/subject — " +
                        "retry with exponential back-off, DLQ after 3 failures.",
                        DateTimeOffset.UtcNow, pending, demoMsg);
                    msgIndex++;
                }
                else
                {
                    logger.LogInformation("[Notify] {Time}: No pending notifications.", DateTimeOffset.UtcNow);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Notification worker encountered an error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }

        logger.LogInformation("Notification Worker stopped.");
    }
}
