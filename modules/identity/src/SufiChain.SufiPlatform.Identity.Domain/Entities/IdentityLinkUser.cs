using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityLinkUser : CreationAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }


    public virtual Guid SourceUserId { get; protected set; }

    public virtual Guid? SourceTenantId { get; protected set; }

    public virtual Guid TargetUserId { get; protected set; }

    public virtual Guid? TargetTenantId { get; protected set; }

    protected IdentityLinkUser()
    {

    }

    public IdentityLinkUser(
        Guid id,
        Guid sourceUserId,
        Guid? sourceTenantId,
        Guid targetUserId,
        Guid? targetTenantId)
        : base(id)
    {
        SourceUserId = sourceUserId;
        SourceTenantId = sourceTenantId;
        TargetUserId = targetUserId;
        TargetTenantId = targetTenantId;
        TenantId = sourceTenantId;
    }
}
