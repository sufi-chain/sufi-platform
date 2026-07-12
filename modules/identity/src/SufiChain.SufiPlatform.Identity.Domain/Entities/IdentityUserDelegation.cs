using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserDelegation : CreationAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid SourceUserId { get; protected set; }

    public virtual Guid TargetUserId { get; protected set; }

    public virtual DateTime StartTime { get; set; }

    public virtual DateTime EndTime { get; set; }

    protected IdentityUserDelegation()
    {

    }

    public IdentityUserDelegation(
        Guid id,
        Guid sourceUserId,
        Guid targetUserId,
        DateTime startTime,
        DateTime endTime,
        Guid? tenantId = null)
        : base(id)
    {
        SourceUserId = sourceUserId;
        TargetUserId = targetUserId;
        StartTime = startTime;
        EndTime = endTime;
        TenantId = tenantId;
    }
}
