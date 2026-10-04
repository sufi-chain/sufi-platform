using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Account.Blazor;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiPlatform.Account.Blazor.Components.Profile;

public partial class ProfileSettings
{
    public const string ProfileFormName = "account-profile";

    private bool _loading = true;
    private bool _saving;
    private string? _successMessage;
    private string? _errorMessage;

    [SupplyParameterFromForm(FormName = ProfileFormName)]
    private ProfileDto? Profile { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Profile != null && !string.IsNullOrWhiteSpace(Profile.UserName))
        {
            _loading = false;
            await OnSaveAsync();
            return;
        }

        try
        {
            Profile = await ProfileAppService.GetAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnSaveAsync()
    {
        if (Profile == null)
        {
            return;
        }

        _saving = true;
        _successMessage = null;
        _errorMessage = null;

        try
        {
            var input = new UpdateProfileDto
            {
                UserName = Profile.UserName,
                Name = Profile.Name,
                Surname = Profile.Surname,
                Email = Profile.Email,
                PhoneNumber = Profile.PhoneNumber,
                ConcurrencyStamp = Profile.ConcurrencyStamp
            };

            try
            {
                Profile.MapExtraPropertiesTo(input);
            }
            catch (Exception mapException)
            {
                Logger.LogWarning(mapException, "Profile extra properties were skipped on save.");
            }

            Profile = await ProfileAppService.UpdateAsync(input);
            _successMessage = L["ProfileUpdatedSuccessfully"];
        }
        catch (Exception ex)
        {
            try
            {
                _errorMessage = AccountUiErrors.LocalizedFailure(
                    Logger,
                    AccountL,
                    ex,
                    "ProfileSaveFailed");
            }
            catch (Exception informException)
            {
                Logger.LogError(informException, "Profile save failed and the message could not be shown.");
                _errorMessage = AccountL["ProfileSaveFailed"];
            }
        }
        finally
        {
            _saving = false;
        }
    }
}
