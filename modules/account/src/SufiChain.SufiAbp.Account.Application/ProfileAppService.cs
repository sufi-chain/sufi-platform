using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Identity;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Users;

namespace SufiChain.SufiAbp.Account;

[Authorize]
public class ProfileAppService : ApplicationService, IProfileAppService
{
    protected IdentityUserManager UserManager { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }

    public ProfileAppService(
        IdentityUserManager userManager,
        IOptions<IdentityOptions> identityOptions)
    {
        UserManager = userManager;
        IdentityOptions = identityOptions;
    }

    public virtual async Task<ProfileDto> GetAsync()
    {
        var currentUser = await UserManager.GetByIdAsync(CurrentUser.GetId());

        return MapToProfileDto(currentUser);
    }

    public virtual async Task<ProfileDto> UpdateAsync(UpdateProfileDto input)
    {
        await IdentityOptions.SetAsync();

        var user = await UserManager.GetByIdAsync(CurrentUser.GetId());

        user.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

        if (!string.Equals(user.UserName, input.UserName, StringComparison.InvariantCultureIgnoreCase))
        {
            (await UserManager.SetUserNameAsync(user, input.UserName)).CheckErrors();
        }

        if (!string.Equals(user.Email, input.Email, StringComparison.InvariantCultureIgnoreCase))
        {
            (await UserManager.SetEmailAsync(user, input.Email)).CheckErrors();
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber) && string.IsNullOrWhiteSpace(input.PhoneNumber))
        {
            input.PhoneNumber = user.PhoneNumber;
        }

        if (!string.Equals(user.PhoneNumber, input.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))
        {
            (await UserManager.SetPhoneNumberAsync(user, input.PhoneNumber)).CheckErrors();
        }

        user.Name = input.Name?.Trim();
        user.Surname = input.Surname?.Trim();

        input.MapExtraPropertiesTo(user);

        (await UserManager.UpdateAsync(user)).CheckErrors();

        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToProfileDto(user);
    }

    public virtual async Task ChangePasswordAsync(ChangePasswordInput input)
    {
        await IdentityOptions.SetAsync();

        var currentUser = await UserManager.GetByIdAsync(CurrentUser.GetId());

        if (currentUser.IsExternal)
        {
            throw new UserFriendlyException("External users cannot change password.");
        }

        if (currentUser.PasswordHash == null)
        {
            (await UserManager.AddPasswordAsync(currentUser, input.NewPassword)).CheckErrors();
            return;
        }

        (await UserManager.ChangePasswordAsync(currentUser, input.CurrentPassword, input.NewPassword)).CheckErrors();
    }

    protected virtual ProfileDto MapToProfileDto(IdentityUser user)
    {
        var dto = new ProfileDto
        {
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            PhoneNumber = user.PhoneNumber,
            IsExternal = user.IsExternal,
            HasPassword = !string.IsNullOrWhiteSpace(user.PasswordHash),
            ConcurrencyStamp = user.ConcurrencyStamp
        };

        user.MapExtraPropertiesTo(dto);

        return dto;
    }
}
