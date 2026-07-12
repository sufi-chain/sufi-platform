using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Uow;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserStore :
    IUserStore<IdentityUser>,
    IUserLoginStore<IdentityUser>,
    IUserRoleStore<IdentityUser>,
    IUserPasswordStore<IdentityUser>,
    IUserEmailStore<IdentityUser>,
    IUserPhoneNumberStore<IdentityUser>,
    IUserTwoFactorStore<IdentityUser>,
    IUserLockoutStore<IdentityUser>,
    IUserSecurityStampStore<IdentityUser>,
    IUserClaimStore<IdentityUser>,
    IUserAuthenticationTokenStore<IdentityUser>
{
    protected IIdentityUserRepository UserRepository { get; }
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public virtual bool AutoSaveChanges { get; set; } = true;

    public IdentityUserStore(
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IGuidGenerator guidGenerator,
        ICancellationTokenProvider cancellationTokenProvider,
        IUnitOfWorkManager unitOfWorkManager)
    {
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        GuidGenerator = guidGenerator;
        CancellationTokenProvider = cancellationTokenProvider;
        UnitOfWorkManager = unitOfWorkManager;
    }

    protected virtual CancellationToken GetCancellationToken(CancellationToken cancellationToken)
    {
        return CancellationTokenProvider.FallbackToProvider(cancellationToken);
    }

    public void Dispose()
    {
    }

    protected virtual async Task<TResult> ExecuteInUnitOfWorkAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        if (UnitOfWorkManager.Current != null)
        {
            return await action();
        }

        using var unitOfWork = UnitOfWorkManager.Begin(requiresNew: true);
        var result = await action();
        await unitOfWork.CompleteAsync(GetCancellationToken(cancellationToken));
        return result;
    }

    protected virtual async Task ExecuteInUnitOfWorkAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (UnitOfWorkManager.Current != null)
        {
            await action();
            return;
        }

        using var unitOfWork = UnitOfWorkManager.Begin(requiresNew: true);
        await action();
        await unitOfWork.CompleteAsync(GetCancellationToken(cancellationToken));
    }

    #region IUserStore

    public virtual async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        await UserRepository.InsertAsync(user, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        await UserRepository.UpdateAsync(user, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        await UserRepository.DeleteAsync(user, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await UserRepository.FindAsync(Guid.Parse(userId), cancellationToken: GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        return await UserRepository.FindByNormalizedUserNameAsync(normalizedUserName, cancellationToken: GetCancellationToken(cancellationToken));
    }

    public virtual Task<string?> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.Id.ToString());
    }

    public virtual Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.UserName);
    }

    public virtual Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken = default)
    {
        user.UserName = userName!;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.NormalizedUserName);
    }

    public virtual Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken = default)
    {
        user.NormalizedUserName = normalizedName!;
        return Task.CompletedTask;
    }

    #endregion

    #region IUserLoginStore

    public virtual async Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Logins, GetCancellationToken(cancellationToken));
        user.AddLogin(new IdentityUserLogin(user.Id, login.LoginProvider, login.ProviderKey, 
            login.ProviderDisplayName, user.TenantId));
    }

    public virtual async Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Logins, GetCancellationToken(cancellationToken));
        user.RemoveLogin(loginProvider, providerKey);
    }

    public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Logins, GetCancellationToken(cancellationToken));
        return user.Logins
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
            .ToList();
    }

    public virtual async Task<IdentityUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        return await UserRepository.FindByLoginAsync(loginProvider, providerKey, cancellationToken: GetCancellationToken(cancellationToken));
    }

    #endregion

    #region IUserRoleStore

    public virtual async Task AddToRoleAsync([NotNull] IdentityUser user, [NotNull] string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(normalizedRoleName, nameof(normalizedRoleName));

        if (await IsInRoleAsync(user, normalizedRoleName, cancellationToken))
        {
            return;
        }

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Roles, GetCancellationToken(cancellationToken));
        
        var role = await RoleRepository.FindByNormalizedNameAsync(normalizedRoleName, cancellationToken: GetCancellationToken(cancellationToken));
        if (role == null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist!", normalizedRoleName));
        }

        user.AddRole(role.Id);
    }

    public virtual async Task RemoveFromRoleAsync([NotNull] IdentityUser user, [NotNull] string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(normalizedRoleName, nameof(normalizedRoleName));

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Roles, GetCancellationToken(cancellationToken));
        
        var role = await RoleRepository.FindByNormalizedNameAsync(normalizedRoleName, cancellationToken: GetCancellationToken(cancellationToken));
        if (role != null)
        {
            user.RemoveRole(role.Id);
        }
    }

    public virtual Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return ExecuteInUnitOfWorkAsync<IList<string>>(
            async () => await UserRepository.GetRoleNamesAsync(user.Id, GetCancellationToken(cancellationToken)),
            cancellationToken);
    }

    public virtual async Task<bool> IsInRoleAsync(IdentityUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        var roles = await GetRolesAsync(user, cancellationToken);
        return roles.Any(r => string.Equals(r, normalizedRoleName, StringComparison.OrdinalIgnoreCase));
    }

    public virtual async Task<IList<IdentityUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        return await UserRepository.GetListByNormalizedRoleNameAsync(normalizedRoleName, cancellationToken: GetCancellationToken(cancellationToken));
    }

    #endregion

    #region IUserPasswordStore

    public virtual Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken = default)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public virtual Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.PasswordHash != null);
    }

    #endregion

    #region IUserEmailStore

    public virtual Task SetEmailAsync(IdentityUser user, string? email, CancellationToken cancellationToken = default)
    {
        user.Email = email!;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.Email);
    }

    public virtual Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public virtual Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken = default)
    {
        user.SetEmailConfirmed(confirmed);
        return Task.CompletedTask;
    }

    public virtual async Task<IdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await UserRepository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken: GetCancellationToken(cancellationToken));
    }

    public virtual Task<string?> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public virtual Task SetNormalizedEmailAsync(IdentityUser user, string? normalizedEmail, CancellationToken cancellationToken = default)
    {
        user.NormalizedEmail = normalizedEmail!;
        return Task.CompletedTask;
    }

    #endregion

    #region IUserPhoneNumberStore

    public virtual Task SetPhoneNumberAsync(IdentityUser user, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public virtual Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public virtual Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken = default)
    {
        user.SetPhoneNumberConfirmed(confirmed);
        return Task.CompletedTask;
    }

    #endregion

    #region IUserTwoFactorStore

    public virtual Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken = default)
    {
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public virtual Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }

    #endregion

    #region IUserLockoutStore

    public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public virtual Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
    {
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public virtual Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public virtual Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public virtual Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public virtual Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public virtual Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken = default)
    {
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    #endregion

    #region IUserSecurityStampStore

    public virtual Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken cancellationToken = default)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetSecurityStampAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(user.SecurityStamp);
    }

    #endregion

    #region IUserClaimStore

    public virtual Task<IList<Claim>> GetClaimsAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        return ExecuteInUnitOfWorkAsync<IList<Claim>>(async () =>
        {
            await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Claims, GetCancellationToken(cancellationToken));
            return user.Claims.Select(c => c.ToClaim()).ToList();
        }, cancellationToken);
    }

    public virtual async Task AddClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Claims, GetCancellationToken(cancellationToken));
        user.AddClaims(GuidGenerator, claims);
    }

    public virtual async Task ReplaceClaimAsync(IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Claims, GetCancellationToken(cancellationToken));
        user.ReplaceClaim(claim, newClaim);
    }

    public virtual async Task RemoveClaimsAsync(IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Claims, GetCancellationToken(cancellationToken));
        user.RemoveClaims(claims);
    }

    public virtual async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        return await UserRepository.GetListByClaimAsync(claim, cancellationToken: GetCancellationToken(cancellationToken));
    }

    #endregion

    #region IUserAuthenticationTokenStore

    public virtual async Task SetTokenAsync(IdentityUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, GetCancellationToken(cancellationToken));
        user.SetToken(loginProvider, name, value);
    }

    public virtual async Task RemoveTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, GetCancellationToken(cancellationToken));
        user.RemoveToken(loginProvider, name);
    }

    public virtual async Task<string?> GetTokenAsync(IdentityUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens, GetCancellationToken(cancellationToken));
        return user.FindToken(loginProvider, name)?.Value;
    }

    #endregion
}
