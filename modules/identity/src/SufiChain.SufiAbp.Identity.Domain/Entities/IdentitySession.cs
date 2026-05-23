using System;
using JetBrains.Annotations;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Identity;

public class IdentitySession : BasicAggregateRoot<Guid>, IMultiTenant, IHasCreationTime
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string SessionId { get; protected set; } = null!;

    [CanBeNull]
    public virtual string? Device { get; set; }

    [CanBeNull]
    public virtual string? DeviceInfo { get; set; }

    public virtual Guid UserId { get; protected set; }

    [CanBeNull]
    public virtual string? ClientId { get; set; }

    [CanBeNull]
    public virtual string? IpAddresses { get; set; }

    public virtual DateTime SignedIn { get; set; }

    public virtual DateTime? LastAccessed { get; set; }

    public virtual DateTime CreationTime { get; protected set; }

    protected IdentitySession()
    {

    }

    public IdentitySession(
        Guid id,
        string sessionId,
        Guid userId,
        Guid? tenantId = null)
        : base(id)
    {
        SessionId = sessionId;
        UserId = userId;
        TenantId = tenantId;
        SignedIn = DateTime.UtcNow;
        CreationTime = DateTime.UtcNow;
    }
}
