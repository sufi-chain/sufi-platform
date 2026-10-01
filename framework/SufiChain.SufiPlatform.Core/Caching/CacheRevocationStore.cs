using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Caching;

/// <summary>
/// Stores temporary revocations in the raw distributed cache so the typed cache wrapper cannot recurse into itself.
/// Callers see a process snapshot for two seconds, and the process that writes a revocation sees it immediately.
/// </summary>
public class CacheRevocationStore : ICacheRevocationStore, ISingletonDependency
{
    public const string StorageKey = "SufiPlatform:CacheRevocations:v1";

    private static readonly TimeSpan SnapshotLifetime = TimeSpan.FromSeconds(2);
    private readonly IDistributedCache _cache;
    private readonly object _sync = new();
    private IReadOnlyList<CacheRevocationEntry> _snapshot = Array.Empty<CacheRevocationEntry>();
    private long _snapshotUntilTicks;

    public CacheRevocationStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public bool IsRevoked(string cacheName, string? key)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ReadSnapshot())
        {
            if (entry.RevokedUntilUtc > now && entry.AppliesTo(cacheName, key))
            {
                return true;
            }
        }

        return false;
    }

    public async Task RevokeAsync(
        string cacheName,
        string? key,
        DateTime revokedUntilUtc,
        CancellationToken cancellationToken = default)
    {
        var entries = await ReadDocumentAsync(cancellationToken);
        entries.RemoveAll(entry =>
            string.Equals(entry.CacheName, cacheName, StringComparison.Ordinal) &&
            string.Equals(entry.Key ?? string.Empty, key ?? string.Empty, StringComparison.Ordinal));
        entries.Add(new CacheRevocationEntry
        {
            CacheName = cacheName,
            Key = string.IsNullOrEmpty(key) ? null : key,
            RevokedUntilUtc = DateTime.SpecifyKind(revokedUntilUtc, DateTimeKind.Utc)
        });
        await WriteDocumentAsync(entries, cancellationToken);
    }

    public async Task LiftAsync(string cacheName, string? key, CancellationToken cancellationToken = default)
    {
        var entries = await ReadDocumentAsync(cancellationToken);
        var removed = entries.RemoveAll(entry =>
            string.Equals(entry.CacheName, cacheName, StringComparison.Ordinal) &&
            string.Equals(entry.Key ?? string.Empty, key ?? string.Empty, StringComparison.Ordinal));
        if (removed > 0)
        {
            await WriteDocumentAsync(entries, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<CacheRevocationEntry>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var entries = await ReadDocumentAsync(cancellationToken);
        ReplaceSnapshot(entries);
        return entries;
    }

    private IReadOnlyList<CacheRevocationEntry> ReadSnapshot()
    {
        var snapshot = _snapshot;
        if (DateTime.UtcNow.Ticks < Interlocked.Read(ref _snapshotUntilTicks))
        {
            return snapshot;
        }

        lock (_sync)
        {
            if (DateTime.UtcNow.Ticks < _snapshotUntilTicks)
            {
                return _snapshot;
            }

            try
            {
                var entries = Decode(_cache.Get(StorageKey));
                ReplaceSnapshot(entries);
                return entries;
            }
            catch (Exception)
            {
                // A revocation-store outage must not fail every cached read. Keep the last snapshot.
                _snapshotUntilTicks = DateTime.UtcNow.Add(SnapshotLifetime).Ticks;
                return _snapshot;
            }
        }
    }

    private async Task<List<CacheRevocationEntry>> ReadDocumentAsync(CancellationToken cancellationToken)
    {
        var bytes = await _cache.GetAsync(StorageKey, cancellationToken);
        return Decode(bytes).ToList();
    }

    private async Task WriteDocumentAsync(List<CacheRevocationEntry> entries, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        entries.RemoveAll(entry => entry.RevokedUntilUtc <= now);
        if (entries.Count == 0)
        {
            await _cache.RemoveAsync(StorageKey, cancellationToken);
            ReplaceSnapshot(Array.Empty<CacheRevocationEntry>());
            return;
        }

        var until = entries.Max(entry => entry.RevokedUntilUtc);
        await _cache.SetAsync(
            StorageKey,
            Encode(entries),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.SpecifyKind(until, DateTimeKind.Utc))
            },
            cancellationToken);
        ReplaceSnapshot(entries);
    }

    private void ReplaceSnapshot(IReadOnlyList<CacheRevocationEntry> entries)
    {
        _snapshot = entries;
        Interlocked.Exchange(ref _snapshotUntilTicks, DateTime.UtcNow.Add(SnapshotLifetime).Ticks);
    }

    private static byte[] Encode(IReadOnlyList<CacheRevocationEntry> entries)
    {
        var builder = new StringBuilder();
        foreach (var entry in entries)
        {
            if (builder.Length > 0)
            {
                builder.Append('\u001e');
            }

            builder.Append(entry.CacheName);
            builder.Append('\u001f');
            builder.Append(entry.Key ?? string.Empty);
            builder.Append('\u001f');
            builder.Append(entry.RevokedUntilUtc.Ticks.ToString(CultureInfo.InvariantCulture));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static IReadOnlyList<CacheRevocationEntry> Decode(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return Array.Empty<CacheRevocationEntry>();
        }

        var now = DateTime.UtcNow;
        var entries = new List<CacheRevocationEntry>();
        foreach (var record in Encoding.UTF8.GetString(bytes).Split('\u001e'))
        {
            var parts = record.Split('\u001f');
            if (parts.Length != 3 ||
                string.IsNullOrWhiteSpace(parts[0]) ||
                !long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks))
            {
                continue;
            }

            var until = new DateTime(ticks, DateTimeKind.Utc);
            if (until <= now)
            {
                continue;
            }

            entries.Add(new CacheRevocationEntry
            {
                CacheName = parts[0],
                Key = string.IsNullOrEmpty(parts[1]) ? null : parts[1],
                RevokedUntilUtc = until
            });
        }

        return entries;
    }
}
