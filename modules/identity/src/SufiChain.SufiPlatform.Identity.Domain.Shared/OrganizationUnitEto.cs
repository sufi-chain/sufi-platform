using System;
using Volo.Abp.Auditing;

using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiPlatform.Identity;

[Serializable]
public class OrganizationUnitEto : IMultiTenant, IHasEntityVersion
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int EntityVersion { get; set; }
}
