using System.Threading.RateLimiting;

namespace SS14.Issues.Services;

public sealed class IssueRateLimiterService : IDisposable
{
    private readonly Dictionary<long, RateLimiter> _rateLimiters = new();
    private readonly TokenBucketRateLimiterOptions _options;

    public IssueRateLimiterService()
    {
        _options = new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = true,
            QueueLimit = 3,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokenLimit = 3,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            TokensPerPeriod = 1
        };
    }

    public async Task<bool> Acquire(long id)
    {
        if (!_rateLimiters.TryGetValue(id, out var limiter))
        {
            limiter = new TokenBucketRateLimiter(_options);
            _rateLimiters.Add(id, limiter);
        }

        var lease = await limiter.AcquireAsync();
        return lease.IsAcquired;
    }

    public async Task ClearStaleLimiters()
    {
        List<long> staleLimiters = new();

        foreach ((var id, var limiter) in _rateLimiters)
        {
            if (limiter.IdleDuration > TimeSpan.FromMinutes(30))
                staleLimiters.Add(id);
        }

        foreach (var staleLimiterId in staleLimiters)
        {
            await _rateLimiters[staleLimiterId].DisposeAsync();
            _rateLimiters.Remove(staleLimiterId);
        }
    }

    public void Dispose()
    {
        foreach (var limiter in _rateLimiters.Values)
        {
            limiter.Dispose();
        }
        
        _rateLimiters.Clear();
    }
}