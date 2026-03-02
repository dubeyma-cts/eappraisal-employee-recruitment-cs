namespace eAppraisal.Domain.Interfaces;

public interface INotificationService
{
    Task QueueAsync(
        string recipientEmail,
        string topic,
        int? appraisalId,
        string? payload = null);
}
