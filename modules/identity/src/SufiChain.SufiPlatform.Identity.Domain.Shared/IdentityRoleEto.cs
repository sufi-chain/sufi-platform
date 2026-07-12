using System;
using Volo.Abp.Auditing;

using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiPlatform.Identity;

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
