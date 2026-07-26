using System;
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

    public IdentityLinkUser(Guid id, IdentityLinkUserInfo sourceUser, IdentityLinkUserInfo targetUser)
        : this(id, sourceUser.UserId, sourceUser.TenantId, targetUser.UserId, targetUser.TenantId)
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
