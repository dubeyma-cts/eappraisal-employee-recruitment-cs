using eAppraisal.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eAppraisal.Infrastructure.CrossCutting;

public class ObservabilityAgent : IObservabilityAgent
{
    private readonly ILogger<ObservabilityAgent> _logger;

    public ObservabilityAgent(ILogger<ObservabilityAgent> logger) => _logger = logger;

    public void LogInfo(string message, params object[] args) => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args) => _logger.LogWarning(message, args);

    public void LogError(Exception ex, string message, params object[] args) => _logger.LogError(ex, message, args);

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        var propsStr = properties != null ? string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")) : "none";
        _logger.LogInformation("Event: {EventName} | Properties: {Properties}", eventName, propsStr);
    }
}
