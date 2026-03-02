using eAppraisal.Application.Contracts;
using eAppraisal.Domain.Entities;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Infrastructure.CrossCutting;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _db;

    public NotificationService(IAppDbContext db) => _db = db;

    public async Task QueueAsync(string recipientEmail, string topic, int? appraisalId, string? payload = null)
    {
        var notification = new Notification
        {
            RecipientEmail = recipientEmail,
            Topic = topic,
            AboutAppraisalId = appraisalId,
            PayloadJson = payload,
            Status = "Queued",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
}
