using System;
using System.Security.Claims;
using JetBrains.Annotations;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserClaim : IdentityClaim
{
    public virtual Guid UserId { get; protected set; }

    protected IdentityUserClaim()
    {

    }

    protected internal IdentityUserClaim(Guid id, Guid userId, [NotNull] Claim claim, Guid? tenantId)
        : base(id, claim, tenantId)
    {
        UserId = userId;
    }

    public IdentityUserClaim(Guid id, Guid userId, [NotNull] string claimType, string? claimValue, Guid? tenantId)
        : base(id, claimType, claimValue, tenantId)
    {
        UserId = userId;
    }
}
