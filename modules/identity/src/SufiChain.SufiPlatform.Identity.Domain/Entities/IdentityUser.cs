using System.Collections.ObjectModel;
using System.Security.Claims;
using JetBrains.Annotations;
using SufiChain.SufiPlatform.Users;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Represents a user in the Sufi Identity system.
/// </summary>
public class IdentityUser : FullAuditedAggregateRoot<Guid>, IUser, IMultiTenant, IHasEntityVersion
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string UserName { get; protected internal set; } = null!;

    [DisableAuditing]
    public virtual string NormalizedUserName { get; protected internal set; } = null!;

    [CanBeNull]
    public virtual string? Name { get; set; }

    [CanBeNull]
    public virtual string? Surname { get; set; }

    public virtual string Email { get; protected internal set; } = null!;

    [DisableAuditing]
    public virtual string NormalizedEmail { get; protected internal set; } = null!;

    public virtual bool EmailConfirmed { get; protected internal set; }

    [DisableAuditing]
    public virtual string? PasswordHash { get; protected internal set; }

    [DisableAuditing]
    public virtual string SecurityStamp { get; protected internal set; } = null!;

    public virtual bool IsExternal { get; set; }

    [CanBeNull]
    public virtual string? PhoneNumber { get; protected internal set; }

    public virtual bool PhoneNumberConfirmed { get; protected internal set; }

    public virtual bool IsActive { get; protected internal set; }

    public virtual bool TwoFactorEnabled { get; protected internal set; }

    public virtual DateTimeOffset? LockoutEnd { get; protected internal set; }

    public virtual bool LockoutEnabled { get; protected internal set; }

    public virtual int AccessFailedCount { get; protected internal set; }

    public virtual bool ShouldChangePasswordOnNextLogin { get; protected internal set; }

    public virtual int EntityVersion { get; protected set; }

    public virtual DateTimeOffset? LastPasswordChangeTime { get; protected set; }

    public virtual DateTimeOffset? LastSignInTime { get; protected set; }

    public virtual ICollection<IdentityUserRole> Roles { get; protected set; }

    public virtual ICollection<IdentityUserClaim> Claims { get; protected set; }

    public virtual ICollection<IdentityUserLogin> Logins { get; protected set; }

    public virtual ICollection<IdentityUserToken> Tokens { get; protected set; }

    public virtual ICollection<IdentityUserOrganizationUnit> OrganizationUnits { get; protected set; }

    protected IdentityUser()
    {
        Roles = new Collection<IdentityUserRole>();
        Claims = new Collection<IdentityUserClaim>();
        Logins = new Collection<IdentityUserLogin>();
        Tokens = new Collection<IdentityUserToken>();
        OrganizationUnits = new Collection<IdentityUserOrganizationUnit>();
    }

    public IdentityUser(
        Guid id,
        [NotNull] string userName,
        [NotNull] string email,
        Guid? tenantId = null)
        : base(id)
    {
        Check.NotNull(userName, nameof(userName));
        Check.NotNull(email, nameof(email));

        TenantId = tenantId;
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
        SecurityStamp = Guid.NewGuid().ToString();
        IsActive = true;

        Roles = new Collection<IdentityUserRole>();
        Claims = new Collection<IdentityUserClaim>();
        Logins = new Collection<IdentityUserLogin>();
        Tokens = new Collection<IdentityUserToken>();
        OrganizationUnits = new Collection<IdentityUserOrganizationUnit>();
    }

    public virtual void AddRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        if (IsInRole(roleId))
        {
            return;
        }

        Roles.Add(new IdentityUserRole(Id, roleId, TenantId));
    }

    public virtual void RemoveRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        Roles.RemoveAll(r => r.RoleId == roleId);
    }

    public virtual bool IsInRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        return Roles.Any(r => r.RoleId == roleId);
    }

    public virtual void AddClaim([NotNull] IGuidGenerator guidGenerator, [NotNull] Claim claim)
    {
        Check.NotNull(guidGenerator, nameof(guidGenerator));
        Check.NotNull(claim, nameof(claim));

        Claims.Add(new IdentityUserClaim(guidGenerator.Create(), Id, claim, TenantId));
    }

    public virtual void AddClaims([NotNull] IGuidGenerator guidGenerator, [NotNull] IEnumerable<Claim> claims)
    {
        Check.NotNull(guidGenerator, nameof(guidGenerator));
        Check.NotNull(claims, nameof(claims));

        foreach (var claim in claims)
        {
            AddClaim(guidGenerator, claim);
        }
    }

    public virtual IdentityUserClaim? FindClaim([NotNull] Claim claim)
    {
        Check.NotNull(claim, nameof(claim));

        return Claims.FirstOrDefault(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value);
    }

    public virtual void ReplaceClaim([NotNull] Claim claim, [NotNull] Claim newClaim)
    {
        Check.NotNull(claim, nameof(claim));
        Check.NotNull(newClaim, nameof(newClaim));

        var userClaims = Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type);
        foreach (var userClaim in userClaims)
        {
            userClaim.SetClaim(newClaim);
        }
    }

    public virtual void RemoveClaims([NotNull] IEnumerable<Claim> claims)
    {
        Check.NotNull(claims, nameof(claims));

        foreach (var claim in claims)
        {
            RemoveClaim(claim);
        }
    }

    public virtual void RemoveClaim([NotNull] Claim claim)
    {
        Check.NotNull(claim, nameof(claim));

        Claims.RemoveAll(c => c.ClaimValue == claim.Value && c.ClaimType == claim.Type);
    }

    public virtual void AddLogin([NotNull] IdentityUserLogin login)
    {
        Check.NotNull(login, nameof(login));

        Logins.Add(login);
    }

    public virtual void RemoveLogin([NotNull] string loginProvider, [NotNull] string providerKey)
    {
        Check.NotNull(loginProvider, nameof(loginProvider));
        Check.NotNull(providerKey, nameof(providerKey));

        Logins.RemoveAll(userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey);
    }

    [CanBeNull]
    public virtual IdentityUserToken? FindToken(string loginProvider, string name)
    {
        return Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
    }

    public virtual void SetToken(string loginProvider, string name, string? value)
    {
        var token = FindToken(loginProvider, name);
        if (token == null)
        {
            Tokens.Add(new IdentityUserToken(Id, loginProvider, name, value, TenantId));
        }
        else
        {
            token.Value = value;
        }
    }

    public virtual void RemoveToken(string loginProvider, string name)
    {
        Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
    }

    public virtual void AddOrganizationUnit(Guid organizationUnitId)
    {
        if (IsInOrganizationUnit(organizationUnitId))
        {
            return;
        }

        OrganizationUnits.Add(
            new IdentityUserOrganizationUnit(
                Id,
                organizationUnitId,
                TenantId
            )
        );
    }

    public virtual void RemoveOrganizationUnit(Guid organizationUnitId)
    {
        OrganizationUnits.RemoveAll(ou => ou.OrganizationUnitId == organizationUnitId);
    }

    public virtual bool IsInOrganizationUnit(Guid organizationUnitId)
    {
        return OrganizationUnits.Any(ou => ou.OrganizationUnitId == organizationUnitId);
    }

    public virtual void SetEmailConfirmed(bool confirmed)
    {
        EmailConfirmed = confirmed;
    }

    public virtual void SetPhoneNumberConfirmed(bool confirmed)
    {
        PhoneNumberConfirmed = confirmed;
    }

    public virtual void SetPhoneNumber(string? phoneNumber, bool confirmed)
    {
        PhoneNumber = phoneNumber;
        PhoneNumberConfirmed = !phoneNumber.IsNullOrWhiteSpace() && confirmed;
    }

    public virtual void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }

    public virtual void SetShouldChangePasswordOnNextLogin(bool shouldChangePasswordOnNextLogin)
    {
        ShouldChangePasswordOnNextLogin = shouldChangePasswordOnNextLogin;
    }

    public virtual void SetLastPasswordChangeTime(DateTimeOffset? lastPasswordChangeTime)
    {
        LastPasswordChangeTime = lastPasswordChangeTime;
    }

    public virtual void SetLastSignInTime(DateTimeOffset? lastSignInTime)
    {
        LastSignInTime = lastSignInTime;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, UserName = {UserName}";
    }
}
