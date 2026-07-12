using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserLogin : Entity, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid UserId { get; protected set; }

    public virtual string LoginProvider { get; protected set; } = null!;

    [CanBeNull]
    public virtual string? TenantName { get; protected set; }

    public virtual string ProviderKey { get; protected set; } = null!;

    [CanBeNull]
    public virtual string? ProviderDisplayName { get; protected set; }

    protected IdentityUserLogin()
    {

    }

    protected internal IdentityUserLogin(
        Guid userId,
        [NotNull] string loginProvider,
        [NotNull] string providerKey,
        string? providerDisplayName,
        Guid? tenantId)
    {
        Check.NotNull(loginProvider, nameof(loginProvider));
        Check.NotNull(providerKey, nameof(providerKey));

        UserId = userId;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        TenantId = tenantId;
        ProviderDisplayName = providerDisplayName;
    }

    public override object[] GetKeys()
    {
        return new object[] { UserId, LoginProvider };
    }
}
