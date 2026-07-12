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

public class EfCoreTagRepository : EfCoreRepository<ITagsDbContext, Tag, Guid>, ITagRepository
{
    public EfCoreTagRepository(IDbContextProvider<ITagsDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public virtual async Task<Tag?> FindByNameAsync(string scope, string normalizedName, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x =>
            x.Scope == scope &&
            x.NormalizedName == normalizedName &&
            x.TenantId == tenantId, cancellationToken);
    }

    public virtual async Task<List<Tag>> GetListByScopeAsync(string scope, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(x => x.Scope == scope && x.TenantId == tenantId).OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public virtual async Task<List<Tag>> SearchAsync(
        string? scope,
        string? filter,
        Guid? tenantId = null,
        int skipCount = 0,
        int maxResultCount = 20,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scope))
        {
            query = query.Where(x => x.Scope == scope);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var trimmedFilter = filter.Trim();
            var normalizedFilter = trimmedFilter.ToUpperInvariant();
            query = query.Where(x =>
                x.Name.Contains(trimmedFilter) ||
                x.NormalizedName.Contains(normalizedFilter));
        }

        return await query
            .OrderBy(x => x.Name)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }
}
