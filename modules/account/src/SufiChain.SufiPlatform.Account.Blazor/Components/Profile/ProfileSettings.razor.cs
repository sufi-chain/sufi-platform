using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Account.Blazor;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Account.Blazor.Components.Profile;

public partial class ProfileSettings : IDisposable
{
    public const string ProfileFormName = "account-profile";

    private const string SavedProfileKey = "account-profile-saved";

    private bool _loading = true;
    private bool _saving;
    private bool _savedThisRender;
    private string? _successMessage;
    private string? _errorMessage;
    private string? _phoneError;

    [SupplyParameterFromForm(FormName = ProfileFormName)]
    private ProfileDto? Profile { get; set; }

    [Inject]
    private PersistentComponentState PersistentState { get; set; } = default!;

    private PersistingComponentStateSubscription? _persist;

    protected override void OnInitialized()
    {
        _persist = PersistentState.RegisterOnPersisting(() =>
        {
            if (_savedThisRender && Profile != null)
            {
                PersistentState.PersistAsJson(SavedProfileKey, Profile);
            }

            return Task.CompletedTask;
        });
    }

    protected override async Task OnInitializedAsync()
    {
        if (PersistentState.TryTakeFromJson<ProfileDto>(SavedProfileKey, out var saved))
        {
            Profile = ProfileSaveBinding.Shown(saved, null);
            _successMessage = L["ProfileUpdatedSuccessfully"];
            _loading = false;
            return;
        }

        if (Profile != null && !string.IsNullOrWhiteSpace(Profile.UserName))
        {
            _loading = false;
            await OnSaveAsync();
            return;
        }

        try
        {
            Profile = ProfileSaveBinding.Shown(null, await ProfileAppService.GetAsync());
        }
        finally
        {
            _loading = false;
        }
    }

    private Task HandleSubmitAsync(EditContext context)
    {
        context.Validate();
        return OnSaveAsync();
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
        _phoneError = null;

        try
        {
            if (!OptionalPhoneAttribute.IsValidPhone(Profile.PhoneNumber))
            {
                _phoneError = AccountL["PhoneNumber:Invalid", L["PhoneNumber"]];
                return;
            }

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

            Profile = ProfileSaveBinding.Shown(await ProfileAppService.UpdateAsync(input), null);
            _savedThisRender = true;
            _successMessage = L["ProfileUpdatedSuccessfully"];
        }
        catch (AbpValidationException ex) when (ex.ValidationErrors.Any(error =>
                     error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber))))
        {
            _phoneError = AccountL["PhoneNumber:Invalid", L["PhoneNumber"]];
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

    public void Dispose() => _persist?.Dispose();
}
