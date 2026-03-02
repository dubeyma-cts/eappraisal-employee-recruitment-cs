using System.Collections.Concurrent;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Infrastructure.CrossCutting;

public class InMemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _attempts = new();

    public bool IsAllowed(string key, int maxAttempts, TimeSpan window)
    {
        CleanExpired(key, window);
        var attempts = _attempts.GetOrAdd(key, _ => new List<DateTime>());
        lock (attempts)
        {
            if (attempts.Count >= maxAttempts)
                return false;
            attempts.Add(DateTime.UtcNow);
            return true;
        }
    }

    public int GetRemainingAttempts(string key, int maxAttempts, TimeSpan window)
    {
        CleanExpired(key, window);
        var attempts = _attempts.GetOrAdd(key, _ => new List<DateTime>());
        lock (attempts)
        {
            return Math.Max(0, maxAttempts - attempts.Count);
        }
    }

    private void CleanExpired(string key, TimeSpan window)
    {
        if (_attempts.TryGetValue(key, out var attempts))
        {
            lock (attempts)
            {
                var cutoff = DateTime.UtcNow - window;
                attempts.RemoveAll(t => t < cutoff);
            }
        }
    }
}
