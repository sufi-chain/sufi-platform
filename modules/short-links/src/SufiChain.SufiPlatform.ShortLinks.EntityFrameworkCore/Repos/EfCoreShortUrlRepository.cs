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
        var dbContext = await GetDbContextAsync();
        var tableName = $"{SufiShortLinksDbProperties.DbTablePrefix}ShortUrls";
        
        var currentTime = _clock.Now;

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE {tableName} SET ClickCount = ClickCount + 1, LastAccessedAt = {currentTime} WHERE Id = {id}",
            cancellationToken);
    }
}