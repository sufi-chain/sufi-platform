using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore.Repos;

public class EfCoreShortUrlRepository : EfCoreRepository<ISufiShortLinksDbContext, ShortUrl, Guid>, IShortUrlRepository
{
    private readonly IClock _clock;

    public EfCoreShortUrlRepository(
        IDbContextProvider<ISufiShortLinksDbContext> dbContextProvider,
        IClock clock)
        : base(dbContextProvider)
    {
        _clock = clock;
    }
    
    public virtual async Task<ShortUrl?> FindByShortCodeAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);
    }
    
    public virtual async Task<bool> ShortCodeExistsAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AnyAsync(x => x.ShortCode == shortCode, cancellationToken);
    }
    
    public virtual async Task<List<ShortUrl>> GetExpiredUrlsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = _clock.Now;
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt < now && x.IsActive)
            .ToListAsync(cancellationToken);
    }
    
    public virtual async Task IncrementClickCountAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var accessedAt = _clock.Now;

        // One UPDATE. The table name is the mapped [SufiShortLinks.ShortUrls], not a
        // SQL parameter, and ClickCount = ClickCount + 1 is atomic.
        await dbSet
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.ClickCount, x => x.ClickCount + 1)
                    .SetProperty(x => x.LastAccessedAt, accessedAt),
                cancellationToken);
    }
}