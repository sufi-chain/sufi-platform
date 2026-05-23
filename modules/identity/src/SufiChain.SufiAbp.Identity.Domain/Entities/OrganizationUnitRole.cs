using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Identity;

public class OrganizationUnitRole : CreationAuditedEntity, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid RoleId { get; protected set; }

    public virtual Guid OrganizationUnitId { get; protected set; }

    protected OrganizationUnitRole()
    {

    }

    public OrganizationUnitRole(Guid roleId, Guid organizationUnitId, Guid? tenantId)
    {
        RoleId = roleId;
        OrganizationUnitId = organizationUnitId;
        TenantId = tenantId;
    }

    public override object[] GetKeys()
    {
        return new object[] { OrganizationUnitId, RoleId };
    }
}
