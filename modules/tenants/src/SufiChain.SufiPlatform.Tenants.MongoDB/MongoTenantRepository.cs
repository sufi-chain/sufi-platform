using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Linq.Dynamic.Core;
using MongoDB.Driver;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tenants.MongoDB;

public class MongoTenantRepository : MongoDbRepository<ITenantsMongoDbContext, Tenant, Guid>, ITenantRepository
{
    public MongoTenantRepository(IMongoDbContextProvider<ITenantsMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }

    public virtual async Task<Tenant> FindByNameAsync(
        string normalizedName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(t => t.NormalizedName == normalizedName, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Tenant>> GetListAsync(
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .WhereIf(
                !filter.IsNullOrWhiteSpace(),
                u =>
                    u.Name.Contains(filter)
            )
            .OrderBy(sorting.IsNullOrEmpty() ? nameof(Tenant.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(string filter = null, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .WhereIf(
                !filter.IsNullOrWhiteSpace(),
                u =>
                    u.Name.Contains(filter)
            ).CountAsync(cancellationToken: GetCancellationToken(cancellationToken));
    }
}
