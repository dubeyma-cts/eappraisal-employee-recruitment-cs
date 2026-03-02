using eAppraisal.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eAppraisal.Infrastructure.ExternalServices;

public class LocalSmtpGateway : ISmtpGateway
{
    private readonly ILogger<LocalSmtpGateway> _logger;

    public LocalSmtpGateway(ILogger<LocalSmtpGateway> logger) => _logger = logger;

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("[SMTP Stub] Email to: {To}, Subject: {Subject}, Body length: {Length}", to, subject, body?.Length ?? 0);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string to, string subject, string body, string? cc = null, string? bcc = null)
    {
        _logger.LogInformation("[SMTP Stub] Email to: {To}, CC: {CC}, BCC: {BCC}, Subject: {Subject}", to, cc ?? "none", bcc ?? "none", subject);
        return Task.CompletedTask;
    }
}
