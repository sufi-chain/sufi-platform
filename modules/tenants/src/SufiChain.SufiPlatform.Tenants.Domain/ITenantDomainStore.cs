using System;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantDomainStore
{
    Task<string?> FindTenantNameByHostAsync(string host);

    /// <summary>
    /// Returns the tenant's primary host from verified, active domains
    /// (Custom before Generated, then CreationTime), or null.
    /// </summary>
    Task<string?> FindPrimaryHostAsync(Guid tenantId);
}
