using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class ConfirmPhone
{
    private const string PanelPath = "/panel";

    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected ILoginCompletionTokenStore TokenStore { get; set; } = default!;

    [Inject]
    protected SignInManager<IdentityUser> SignInManager { get; set; } = default!;

    [Inject]
    protected IdentityUserManager UserManager { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected ILogger<ConfirmPhone> Logger { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromQuery]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery]
    public bool Sent { get; set; }

    protected bool IsLoading { get; set; } = true;

    protected bool IsBusy { get; set; }

    protected bool NeedsPhoneNumber { get; set; }

    protected string? PhoneNumber { get; set; }

    protected string? Code { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SuccessMessage { get; set; }

    protected DateTimeOffset? ResendAvailableAt { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!UserId.HasValue)
        {
            IsLoading = false;
            ErrorMessage = AccountL["ConfirmPhoneInvalid"];
            return;
        }

        try
        {
            var state = await AccountAppService.GetPhoneConfirmationStateAsync(UserId.Value);
            if (state.PhoneNumberConfirmed)
            {
                await FinishAsync(state.EmailConfirmationRequired, state.Email);
                return;
            }

            PhoneNumber = state.PhoneNumber;
            NeedsPhoneNumber = string.IsNullOrWhiteSpace(state.PhoneNumber);
            if (!NeedsPhoneNumber && !Sent)
            {
                var result = await AccountAppService.SendPhoneConfirmationCodeAsync(new SendPhoneConfirmationCodeDto
                {
                    UserId = UserId.Value
                });
                ResendAvailableAt = AccountUiErrors.ToResendAvailableAt(result);
                SuccessMessage = AccountL["ConfirmPhoneCodeSent"];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.OtpSendFailure(Logger, AccountL, ex, "ConfirmPhoneInvalid", out var resendAvailableAt);
            ResendAvailableAt = resendAvailableAt ?? ResendAvailableAt;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task OnSendCodeAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await AccountAppService.SendPhoneConfirmationCodeAsync(new SendPhoneConfirmationCodeDto
            {
                UserId = UserId.Value,
                PhoneNumber = PhoneNumber
            });
            ResendAvailableAt = AccountUiErrors.ToResendAvailableAt(result);
            NeedsPhoneNumber = false;
            SuccessMessage = AccountL["ConfirmPhoneCodeSent"];
        }
        catch (Exception ex)
        {
            SuccessMessage = null;
            ErrorMessage = AccountUiErrors.OtpSendFailure(Logger, AccountL, ex, "ConfirmPhoneInvalid", out var resendAvailableAt);
            ResendAvailableAt = resendAvailableAt ?? ResendAvailableAt;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual Task OnResendAsync()
    {
        return OnSendCodeAsync();
    }

    protected virtual async Task OnConfirmAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await AccountAppService.ConfirmPhoneNumberAsync(new ConfirmPhoneNumberDto
            {
                UserId = UserId.Value,
                Code = Code
            });
            await FinishAsync(result.EmailConfirmationStillRequired, result.Email);
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.LocalizedFailure(Logger, AccountL, ex, "ConfirmPhoneInvalid");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task FinishAsync(bool emailConfirmationStillRequired, string? email)
    {
        if (emailConfirmationStillRequired)
        {
            var address = Uri.EscapeDataString(email ?? string.Empty);
            Navigation.NavigateTo($"/account/email-confirmation-sent?email={address}", forceLoad: true);
            return;
        }

        if (UserId.HasValue && TokenStore.IsSupported)
        {
            var token = await TokenStore.CreateAsync(UserId.Value, PanelPath, rememberMe: false);
            Navigation.NavigateTo(
                $"/account/complete-login?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(PanelPath)}",
                forceLoad: true);
            return;
        }

        if (UserId.HasValue && HttpContext != null)
        {
            var user = await UserManager.FindByIdAsync(UserId.Value.ToString());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
                HttpContext.Response.Redirect(PanelPath);
                return;
            }
        }

        Navigation.NavigateTo("/account/login?phoneConfirmed=true", forceLoad: true);
    }
}
