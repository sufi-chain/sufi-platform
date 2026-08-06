using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.ShortLinks;

public interface IShortUrlRepository : IRepository<ShortUrl, Guid>
{
    Task<ShortUrl?> FindByShortCodeAsync(
        string shortCode, 
        CancellationToken cancellationToken = default);
    
    Task<bool> ShortCodeExistsAsync(
        string shortCode, 
        CancellationToken cancellationToken = default);
    
    Task<List<ShortUrl>> GetExpiredUrlsAsync(
        CancellationToken cancellationToken = default);
    
    Task IncrementClickCountAsync(
        Guid id, 
        CancellationToken cancellationToken = default);
}

