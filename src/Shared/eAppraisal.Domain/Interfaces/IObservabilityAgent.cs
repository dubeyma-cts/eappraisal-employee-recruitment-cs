namespace eAppraisal.Domain.Interfaces;

public interface IObservabilityAgent
{
    void LogInfo(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null);
}
