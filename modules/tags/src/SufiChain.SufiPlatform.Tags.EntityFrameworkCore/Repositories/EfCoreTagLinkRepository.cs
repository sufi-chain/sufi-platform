using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.Repositories;

public class EfCoreTagLinkRepository : EfCoreRepository<ITagsDbContext, TagLink, Guid>, ITagLinkRepository
{
    public EfCoreTagLinkRepository(IDbContextProvider<ITagsDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public virtual async Task<bool> ExistsAsync(Guid tagId, string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x =>
            x.TagId == tagId &&
            x.EntityType == entityType &&
            x.EntityId == entityId &&
            x.TenantId == tenantId, cancellationToken);
    }

    public virtual async Task<List<TagLink>> GetListByEntityAsync(string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(x => x.EntityType == entityType && x.EntityId == entityId && x.TenantId == tenantId).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TagLink>> GetListByTagAsync(Guid tagId, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(x => x.TagId == tagId && x.TenantId == tenantId).ToListAsync(cancellationToken);
    }
}
