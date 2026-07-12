using System;
using JetBrains.Annotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserOrganizationUnit : Entity, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid UserId { get; protected set; }

    public virtual Guid OrganizationUnitId { get; protected set; }

    public virtual DateTime CreationTime { get; protected set; }

    [CanBeNull]
    public virtual Guid? CreatorId { get; protected set; }

    protected IdentityUserOrganizationUnit()
    {

    }

    protected internal IdentityUserOrganizationUnit(Guid userId, Guid organizationUnitId, Guid? tenantId)
    {
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
        TenantId = tenantId;
        CreationTime = DateTime.UtcNow;
    }

    public override object[] GetKeys()
    {
        return new object[] { OrganizationUnitId, UserId };
    }
}
