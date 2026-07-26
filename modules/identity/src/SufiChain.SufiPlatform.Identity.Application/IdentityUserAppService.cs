using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserAppService : SufiApplicationService, IIdentityUserAppService
{
    protected IIdentityUserRepository UserRepository { get; }
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IdentityUserManager UserManager { get; }

    public IdentityUserAppService(
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IdentityUserManager userManager)
    {
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        UserManager = userManager;
    }

    public virtual async Task<IdentityUserDto> GetAsync(Guid id)
    {
        return MapToDto(await UserRepository.GetAsync(id));
    }

    public virtual async Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        var count = await UserRepository.GetCountAsync(filter: input.Filter);
        var users = await UserRepository.GetListAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            filter: input.Filter);

        return new PagedResultDto<IdentityUserDto>(count, users.Select(MapToDto).ToList());
    }

    public virtual async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
       var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, CurrentTenant.Id)
       {
           Name = input.Name,
           Surname = input.Surname
       };
       user.SetIsActive(input.IsActive);

        // Persist the user first. UserManager.SetPhoneNumberAsync/SetLockoutEnabledAsync internally
        // call UpdateAsync (an UPDATE); running them before the INSERT exists in the database throws
        // DbUpdateConcurrencyException ("expected to affect 1 row(s), but actually affected 0").
        var result = await UserManager.CreateAsync(user, input.Password, validatePassword: true);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await UserManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        await UserManager.SetLockoutEnabledAsync(user, input.LockoutEnabled);

        if (input.RoleNames?.Any() == true)
        {
            result = await UserManager.SetRolesAsync(user, input.RoleNames);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        return MapToDto(user);
    }

    public virtual async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        var user = await UserManager.GetByIdAsync(id);
        user.ConcurrencyStamp = input.ConcurrencyStamp;
        await UserManager.SetUserNameAsync(user, input.UserName);
        await UserManager.SetEmailAsync(user, input.Email);
        await UserManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        await UserManager.SetLockoutEnabledAsync(user, input.LockoutEnabled);
        user.Name = input.Name;
        user.Surname = input.Surname;
        user.SetIsActive(input.IsActive);

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            var token = await UserManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await UserManager.ResetPasswordAsync(user, token, input.Password);
            if (!passwordResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", passwordResult.Errors.Select(e => e.Description)));
            }
        }

        var result = await UserManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (input.RoleNames != null)
        {
            result = await UserManager.SetRolesAsync(user, input.RoleNames);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        return MapToDto(user);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var user = await UserManager.GetByIdAsync(id);
        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        var roles = await UserRepository.GetRolesAsync(id);
        return new ListResultDto<IdentityRoleDto>(roles.Select(MapRoleToDto).ToList());
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        var roles = await RoleRepository.GetListAsync();
        return new ListResultDto<IdentityRoleDto>(roles.Select(MapRoleToDto).ToList());
    }

    public virtual async Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        var user = await UserManager.GetByIdAsync(id);
        var result = await UserManager.SetRolesAsync(user, input.RoleNames);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public virtual async Task<IdentityUserDto> FindByUsernameAsync(string userName)
    {
        var user = await UserRepository.FindByNormalizedUserNameAsync(userName.ToUpperInvariant());
        if (user == null)
        {
            throw new InvalidOperationException($"User not found: {userName}");
        }

        return MapToDto(user);
    }

    public virtual async Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        var user = await UserRepository.FindByNormalizedEmailAsync(email.ToUpperInvariant());
        if (user == null)
        {
            throw new InvalidOperationException($"User not found: {email}");
        }

        return MapToDto(user);
    }

    protected virtual IdentityUserDto MapToDto(IdentityUser user)
    {
        return new IdentityUserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            IsActive = user.IsActive,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount,
            LockoutEnd = user.LockoutEnd,
            ConcurrencyStamp = user.ConcurrencyStamp,
            EntityVersion = user.EntityVersion,
            LastPasswordChangeTime = user.LastPasswordChangeTime
        };
    }

    protected virtual IdentityRoleDto MapRoleToDto(IdentityRole role)
    {
        return new IdentityRoleDto
        {
            Id = role.Id,
            Name = role.Name,
            IsDefault = role.IsDefault,
            IsStatic = role.IsStatic,
            IsPublic = role.IsPublic,
            ConcurrencyStamp = role.ConcurrencyStamp,
            CreationTime = role.CreationTime
        };
    }
}
