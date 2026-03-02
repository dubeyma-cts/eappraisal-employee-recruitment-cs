namespace eAppraisal.Domain.Interfaces;

public interface IRateLimiter
{
    bool IsAllowed(string key, int maxAttempts, TimeSpan window);
    int GetRemainingAttempts(string key, int maxAttempts, TimeSpan window);
}
