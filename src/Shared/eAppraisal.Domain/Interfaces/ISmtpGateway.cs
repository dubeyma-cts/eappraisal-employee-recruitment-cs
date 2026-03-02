namespace eAppraisal.Domain.Interfaces;

public interface ISmtpGateway
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailAsync(string to, string subject, string body, string? cc = null, string? bcc = null);
}
