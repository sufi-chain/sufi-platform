using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

public class EfCoreIdentityClaimTypeRepository : EfCoreRepository<ISufiIdentityDbContext, IdentityClaimType, Guid>, IIdentityClaimTypeRepository
{
    public EfCoreIdentityClaimTypeRepository(IDbContextProvider<ISufiIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<bool> AnyAsync(
        string name,
        Guid? ignoredId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(ignoredId != null, ct => ct.Id != ignoredId)
            .AnyAsync(ct => ct.Name == name, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityClaimType>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(!filter.IsNullOrWhiteSpace(), ct => ct.Name.Contains(filter!))
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityClaimType.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(!filter.IsNullOrWhiteSpace(), ct => ct.Name.Contains(filter!))
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
