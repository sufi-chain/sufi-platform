using System;
using System.Collections.Generic;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Published after a host creates a tenant. The distributed bus stores this
/// event in the outbox. Do not put the admin password on it.
/// <see cref="AdminPasswordPropertyName"/> is only the in-memory seed-context key.
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
