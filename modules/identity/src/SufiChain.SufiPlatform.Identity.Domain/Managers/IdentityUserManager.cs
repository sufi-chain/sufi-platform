using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserManager : UserManager<IdentityUser>, IDomainService
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }
    
    protected override CancellationToken CancellationToken => CancellationTokenProvider.Token;

    public IdentityUserManager(
        IdentityUserStore store,
        IIdentityRoleRepository roleRepository,
        IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IdentityUser> passwordHasher,
        IEnumerable<IUserValidator<IdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<IdentityUserManager> logger,
        ICancellationTokenProvider cancellationTokenProvider,
        IOrganizationUnitRepository organizationUnitRepository)
        : base(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
        OrganizationUnitRepository = organizationUnitRepository;
        RoleRepository = roleRepository;
        UserRepository = userRepository;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    public virtual async Task<IdentityResult> CreateAsync(IdentityUser user, string password, bool validatePassword)
    {
        var result = await UpdatePasswordHash(user, password, validatePassword);
        if (!result.Succeeded)
        {
            return result;
        }

        return await CreateAsync(user);
    }

    public async override Task<IdentityResult> DeleteAsync(IdentityUser user)
    {
        user.Claims.Clear();
        user.Roles.Clear();
        user.Tokens.Clear();
        user.Logins.Clear();
        user.OrganizationUnits.Clear();
        
        await UpdateAsync(user);

        return await base.DeleteAsync(user);
    }

    public virtual async Task<IdentityUser> GetByIdAsync(Guid id)
    {
        var user = await Store.FindByIdAsync(id.ToString(), CancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), id);
        }

        return user;
    }

    public virtual async Task<IdentityResult> SetRolesAsync(
        [NotNull] IdentityUser user,
        [NotNull] IEnumerable<string> roleNames)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(roleNames, nameof(roleNames));

        var currentRoleNames = await GetRolesAsync(user);

        var result = await RemoveFromRolesAsync(user, currentRoleNames.Except(roleNames).Distinct());
        if (!result.Succeeded)
        {
            return result;
        }

        result = await AddToRolesAsync(user, roleNames.Except(currentRoleNames).Distinct());
        if (!result.Succeeded)
        {
            return result;
        }

        return IdentityResult.Success;
    }

    public virtual async Task<bool> IsInOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        var user = await UserRepository.GetAsync(userId, cancellationToken: CancellationToken);
        return user.IsInOrganizationUnit(ouId);
    }

    public virtual async Task<bool> IsInOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits, CancellationToken);
        return user.IsInOrganizationUnit(ou.Id);
    }

    public virtual async Task AddToOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        await AddToOrganizationUnitAsync(
            await UserRepository.GetAsync(userId, cancellationToken: CancellationToken),
            await OrganizationUnitRepository.GetAsync(ouId, cancellationToken: CancellationToken)
        );
    }

    public virtual async Task AddToOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits, CancellationToken);

        if (user.OrganizationUnits.Any(cou => cou.OrganizationUnitId == ou.Id))
        {
            return;
        }

        user.AddOrganizationUnit(ou.Id);
    }

    public virtual async Task RemoveFromOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        var user = await UserRepository.GetAsync(userId, cancellationToken: CancellationToken);
        user.RemoveOrganizationUnit(ouId);
    }

    public virtual async Task RemoveFromOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits, CancellationToken);
        user.RemoveOrganizationUnit(ou.Id);
    }

    public virtual async Task SetOrganizationUnitsAsync(Guid userId, params Guid[] organizationUnitIds)
    {
        await SetOrganizationUnitsAsync(
            await UserRepository.GetAsync(userId, cancellationToken: CancellationToken),
            organizationUnitIds
        );
    }

    public virtual async Task SetOrganizationUnitsAsync(IdentityUser user, params Guid[] organizationUnitIds)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(organizationUnitIds, nameof(organizationUnitIds));

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits, CancellationToken);

        // Remove from removed OUs
        foreach (var ouId in user.OrganizationUnits.Select(uou => uou.OrganizationUnitId).ToArray())
        {
            if (!organizationUnitIds.Contains(ouId))
            {
                user.RemoveOrganizationUnit(ouId);
            }
        }

        // Add to added OUs
        foreach (var organizationUnitId in organizationUnitIds)
        {
            if (!user.IsInOrganizationUnit(organizationUnitId))
            {
                user.AddOrganizationUnit(organizationUnitId);
            }
        }
    }

    public virtual async Task<bool> ShouldPeriodicallyChangePasswordAsync(IdentityUser user)
    {
        Check.NotNull(user, nameof(user));

        if (user.PasswordHash.IsNullOrWhiteSpace())
        {
            return false;
        }

        var forceUsersToPeriodicallyChangePassword = false; // Could be from settings
        if (!forceUsersToPeriodicallyChangePassword)
        {
            return false;
        }

        var lastPasswordChangeTime = user.LastPasswordChangeTime ?? user.CreationTime;
        var passwordChangePeriodDays = 90; // Could be from settings

        return lastPasswordChangeTime.AddDays(passwordChangePeriodDays) < DateTime.UtcNow;
    }

    public virtual async Task<IdentityResult> AddDefaultRolesAsync([NotNull] IdentityUser user)
    {
        Check.NotNull(user, nameof(user));

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Roles, CancellationToken);

        foreach (var role in await RoleRepository.GetDefaultOnesAsync(cancellationToken: CancellationToken))
        {
            if (!user.IsInRole(role.Id))
            {
                user.AddRole(role.Id);
            }
        }

        return await UpdateAsync(user);
    }

    public virtual async Task<string> GetUserNameFromEmailAsync(string email)
    {
        Check.NotNullOrWhiteSpace(email, nameof(email));

        const int maxTryCount = 20;
        var userName = email.Split('@')[0];

        if (await IsAvailableUserNameAsync(userName))
        {
            return userName;
        }

        for (var i = 0; i < maxTryCount; i++)
        {
            var candidate = $"{SanitizeUserName(userName)}{Random.Shared.Next(1000, 9999)}";
            if (await IsAvailableUserNameAsync(candidate))
            {
                return candidate;
            }
        }

        Logger.LogError("Could not get a valid user name for the given email address: {Email}", email);
        throw new AbpIdentityResultException(IdentityResult.Failed(ErrorDescriber.InvalidUserName(userName)));
    }

    protected virtual async Task<bool> IsAvailableUserNameAsync(string userName)
    {
        if (userName.IsNullOrWhiteSpace())
        {
            return false;
        }

        var allowedCharacters = Options.User.AllowedUserNameCharacters;
        if (!allowedCharacters.IsNullOrWhiteSpace() && userName.Any(c => !allowedCharacters.Contains(c)))
        {
            return false;
        }

        return await FindByNameAsync(userName) == null;
    }

    protected virtual string SanitizeUserName(string userName)
    {
        var allowedCharacters = Options.User.AllowedUserNameCharacters;
        if (allowedCharacters.IsNullOrWhiteSpace())
        {
            return userName.IsNullOrWhiteSpace() ? "user" : userName;
        }

        var sanitized = new string(userName.Where(allowedCharacters.Contains).ToArray());
        return sanitized.IsNullOrWhiteSpace() ? "user" : sanitized;
    }

    public virtual async Task<IdentityResult> ChangePasswordAsync(
        IdentityUser user,
        string newPassword)
    {
        var result = await UpdatePasswordHash(user, newPassword, validatePassword: true);
        if (!result.Succeeded)
        {
            return result;
        }

        user.SetLastPasswordChangeTime(DateTimeOffset.UtcNow);

        return await UpdateAsync(user);
    }

    public virtual async Task<IdentityResult> ResetPasswordAsync(
        IdentityUser user,
        string newPassword)
    {
        var result = await UpdatePasswordHash(user, newPassword, validatePassword: true);
        if (!result.Succeeded)
        {
            return result;
        }

        user.SetLastPasswordChangeTime(DateTimeOffset.UtcNow);
        user.SetShouldChangePasswordOnNextLogin(false);

        return await UpdateAsync(user);
    }

    protected virtual async Task<IdentityResult> UpdatePasswordHash(
        IdentityUser user,
        string newPassword,
        bool validatePassword = true)
    {
        if (validatePassword)
        {
            var validate = await ValidatePasswordAsync(user, newPassword);
            if (!validate.Succeeded)
            {
                return validate;
            }
        }

        user.PasswordHash = PasswordHasher.HashPassword(user, newPassword);
        return IdentityResult.Success;
    }
}
