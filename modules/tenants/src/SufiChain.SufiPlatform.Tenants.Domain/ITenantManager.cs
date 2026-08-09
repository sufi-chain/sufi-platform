using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantManager : IDomainService
{
    [NotNull]
    Task<Tenant> CreateAsync([NotNull] string name);

    /// <summary>
    /// Creates a tenant with a predetermined id (useful for configuration-driven seeding).
    /// </summary>
    [NotNull]
    Task<Tenant> CreateAsync(Guid id, [NotNull] string name);

    Task ChangeNameAsync([NotNull] Tenant tenant, [NotNull] string name);

    Task SetDatabaseNameAsync([NotNull] Tenant tenant, [NotNull] string databaseName);

    Task ConfigureRoutingAsync(
        [NotNull] Tenant tenant,
        [NotNull] string primarySubdomain,
        [NotNull] IEnumerable<TenantDomainConfiguration> domains);
}
