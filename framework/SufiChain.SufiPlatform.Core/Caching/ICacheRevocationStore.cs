namespace SufiChain.SufiPlatform.Caching;

public interface ICacheRevocationStore
{
    bool IsRevoked(string cacheName, string? key);

    Task RevokeAsync(string cacheName, string? key, DateTime revokedUntilUtc, CancellationToken cancellationToken = default);

    Task LiftAsync(string cacheName, string? key, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CacheRevocationEntry>> GetActiveAsync(CancellationToken cancellationToken = default);
}
