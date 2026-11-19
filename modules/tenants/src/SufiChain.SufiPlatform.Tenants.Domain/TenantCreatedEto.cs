using System;
using System.Collections.Generic;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Published after a host creates a tenant. Property names match
/// <c>IdentityDataSeedContributor</c> so the admin seed reads this payload.
/// The password property is for the in-process seed only. Do not log this event.
/// </summary>
[Serializable]
[EventName("SufiChain.SufiPlatform.Tenants.TenantCreated")]
public class TenantCreatedEto
{
    public const string AdminEmailPropertyName = "AdminEmail";

    public const string AdminPasswordPropertyName = "AdminPassword";

    public Guid Id { get; set; }

    public string Name { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new();
}
