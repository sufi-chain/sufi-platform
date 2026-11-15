using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Tenants and their verified custom domains from the host tenant index.
    /// Generated subdomains are not included. The same index answers PanelHost.
    /// </summary>
    Task<IReadOnlyList<TenantPublicHost>> ListPublicHostsAsync();
}

[Serializable]
public class TenantPublicHost
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = "";

    public string? PrimarySubdomain { get; set; }

    public string? NormalizedName { get; set; }

    public List<string> CustomHosts { get; set; } = new();
}
