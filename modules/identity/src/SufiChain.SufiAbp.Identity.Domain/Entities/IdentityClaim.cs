using System;
using System.Security.Claims;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Identity;

public abstract class IdentityClaim : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string ClaimType { get; protected set; } = null!;

    public virtual string? ClaimValue { get; protected set; }

    protected IdentityClaim()
    {

    }

    protected internal IdentityClaim(Guid id, [NotNull] Claim claim, Guid? tenantId)
        : this(id, claim.Type, claim.Value, tenantId)
    {

    }

    protected internal IdentityClaim(Guid id, [NotNull] string claimType, string? claimValue, Guid? tenantId)
    {
        Check.NotNull(claimType, nameof(claimType));

        Id = id;
        ClaimType = claimType;
        ClaimValue = claimValue;
        TenantId = tenantId;
    }

    public virtual Claim ToClaim()
    {
        return new Claim(ClaimType, ClaimValue ?? string.Empty);
    }

    public virtual void SetClaim([NotNull] Claim claim)
    {
        Check.NotNull(claim, nameof(claim));

        ClaimType = claim.Type;
        ClaimValue = claim.Value;
    }
}
