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

public class EfCoreIdentityRoleRepository : EfCoreRepository<ISufiIdentityDbContext, IdentityRole, Guid>, IIdentityRoleRepository
{
    public EfCoreIdentityRoleRepository(IDbContextProvider<ISufiIdentityDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<IdentityRole?> FindByNormalizedNameAsync(
        string normalizedRoleName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                r => r.NormalizedName == normalizedRoleName,
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task<List<IdentityRole>> GetListAsync(
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string? filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .WhereIf(
                !filter.IsNullOrWhiteSpace(),
                r => r.Name.Contains(filter!)
            )
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetListAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(r => ids.Contains(r.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetListAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(r => names.Contains(r.Name))
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<IdentityRole>> GetDefaultOnesAsync(
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(r => r.IsDefault)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .WhereIf(
                !filter.IsNullOrWhiteSpace(),
                r => r.Name.Contains(filter!)
            )
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public override async Task<IQueryable<IdentityRole>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }
}
