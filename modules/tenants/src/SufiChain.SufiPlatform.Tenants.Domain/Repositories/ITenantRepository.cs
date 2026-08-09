using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantRepository : IBasicRepository<Tenant, Guid>
{
    Task<Tenant> FindByNameAsync(
        string normalizedName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<Tenant> FindByDatabaseNameAsync(
        string databaseName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<Tenant> FindByPrimarySubdomainAsync(
        string primarySubdomain,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<Tenant> FindByDomainHostAsync(
        string host,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    Task<List<Tenant>> GetListAsync(
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string filter = null,
        CancellationToken cancellationToken = default);
}
