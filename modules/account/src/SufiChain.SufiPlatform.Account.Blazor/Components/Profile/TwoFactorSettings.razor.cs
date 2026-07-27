using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Components.Profile;

public partial class TwoFactorSettings
{
    [Inject]
    protected IAccountTwoFactorAppService TwoFactorAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// Optional return URL after post-login two-factor setup.
    /// </summary>
    [Parameter]
    public string? ReturnUrl { get; set; }

    protected TwoFactorInfoDto? Info { get; set; }

    protected AuthenticatorSetupDto? Setup { get; set; }

    protected string VerificationCode { get; set; } = string.Empty;

    protected string DisablePassword { get; set; } = string.Empty;

    protected string[] RecoveryCodes { get; set; } = [];

    protected bool IsLoading { get; set; } = true;

    protected bool IsBusy { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SuccessMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected virtual async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Info = await TwoFactorAppService.GetTwoFactorInfoAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task OnGenerateSetupAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            Setup = await TwoFactorAppService.GenerateAuthenticatorSetupAsync();
            SuccessMessage = AccountL["AuthenticatorSetupGenerated"];
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task OnEnableAsync()
    {
        if (string.IsNullOrWhiteSpace(VerificationCode))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var result = await TwoFactorAppService.EnableTwoFactorAsync(new EnableTwoFactorInput
            {
                Code = VerificationCode
            });

            RecoveryCodes = result.RecoveryCodes;
            Setup = null;
            VerificationCode = string.Empty;
            SuccessMessage = AccountL["TwoFactorEnabledSuccess"];
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task OnDisableAsync()
    {
        if (string.IsNullOrWhiteSpace(DisablePassword))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await TwoFactorAppService.DisableTwoFactorAsync(new DisableTwoFactorInput
            {
                Password = DisablePassword
            });

            DisablePassword = string.Empty;
            RecoveryCodes = [];
            Setup = null;
            SuccessMessage = AccountL["TwoFactorDisabledSuccess"];
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task OnRegenerateRecoveryCodesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await TwoFactorAppService.GenerateRecoveryCodesAsync();
            RecoveryCodes = result.RecoveryCodes;
            SuccessMessage = AccountL["RecoveryCodesRegenerated"];
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual void ContinueAfterSetup()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl))
        {
            Navigation.NavigateTo(ReturnUrl, forceLoad: true);
            return;
        }

        Navigation.NavigateTo("/panel/dashboard", forceLoad: true);
    }
}
