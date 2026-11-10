using System;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Tells every host to drop its SufiTenantDomainMap cache entry.
/// The local invalidator only clears the process that wrote the tenant.
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.Tenants.TenantDomainMapChanged")]
public class TenantDomainMapChangedEto
{
    public Guid? TenantId { get; set; }

    public DateTime OccurredAt { get; set; }
}
