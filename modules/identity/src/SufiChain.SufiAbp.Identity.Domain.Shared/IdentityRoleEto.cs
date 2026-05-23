using System;
using SufiChain.SufiAbp.Auditing;
using SufiChain.SufiAbp.MultiTenancy;

namespace SufiChain.SufiAbp.Identity;

[Serializable]
public class IdentityRoleEto : IMultiTenant, IHasEntityVersion
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDefault { get; set; }

    public bool IsStatic { get; set; }

    public bool IsPublic { get; set; }

    public int EntityVersion { get; set; }
}
