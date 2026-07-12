using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserToken : Entity, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid UserId { get; protected set; }

    public virtual string LoginProvider { get; protected set; } = null!;

    public virtual string Name { get; protected set; } = null!;

    [CanBeNull]
    public virtual string? Value { get; set; }

    protected IdentityUserToken()
    {

    }

    protected internal IdentityUserToken(
        Guid userId,
        [NotNull] string loginProvider,
        [NotNull] string name,
        string? value,
        Guid? tenantId)
    {
        Check.NotNull(loginProvider, nameof(loginProvider));
        Check.NotNull(name, nameof(name));

        UserId = userId;
        LoginProvider = loginProvider;
        Name = name;
        Value = value;
        TenantId = tenantId;
    }

    public override object[] GetKeys()
    {
        return new object[] { UserId, LoginProvider, Name };
    }
}
