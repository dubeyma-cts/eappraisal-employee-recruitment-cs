using eAppraisal.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace eAppraisal.Export;

/// <summary>
/// Export Worker Service
/// Dequeues export jobs from Azure Service Bus / in-memory channel and generates
/// policy-aware PDF/Excel reports with PAN masking and time-limited signed Blob URLs.
/// In this demo the worker polls the DB for completed appraisals and logs export events.
/// </summary>
public class ExportWorker(ILogger<ExportWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Export Worker started. Polling for export jobs every 30 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var completed = await db.Appraisals
                    .Include(a => a.Employee)
                    .Where(a => a.Status == eAppraisal.Shared.Models.AppraisalStatus.Completed)
                    .CountAsync(stoppingToken);

                logger.LogInformation(
                    "[Export] {Time}: {Count} completed appraisal(s) available for export. " +
                    "In production: dequeue from Azure Service Bus, generate masked PDF/Excel, " +
                    "upload to Azure Blob Storage, return time-limited signed URL (TTL ≤ 15 min).",
                    DateTimeOffset.UtcNow, completed);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Export worker encountered an error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        logger.LogInformation("Export Worker stopped.");
    }
}
