using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.TagsManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.TagsManagement.Repositories;

public class EfCoreTagRepository : EfCoreRepository<ITagsManagementDbContext, Tag, Guid>, ITagRepository
{
    public EfCoreTagRepository(IDbContextProvider<ITagsManagementDbContext> dbContextProvider) : base(dbContextProvider)
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
}
