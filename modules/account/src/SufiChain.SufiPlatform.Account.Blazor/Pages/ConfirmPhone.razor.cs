using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp;
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

    [SupplyParameterFromQuery(Name = "token")]
    public string? SessionToken { get; set; }

    [SupplyParameterFromQuery]
    public bool Sent { get; set; }

    protected Guid? ConfirmedUserId { get; set; }

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
        if (string.IsNullOrWhiteSpace(SessionToken))
        {
            IsLoading = false;
            ErrorMessage = AccountL["ConfirmPhoneSessionInvalid"];
            return;
        }

        try
        {
            var state = await AccountAppService.GetPhoneConfirmationStateAsync(SessionToken);
            ConfirmedUserId = state.UserId;
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
                    SessionToken = SessionToken
                });
                ResendAvailableAt = AccountUiErrors.ToResendAvailableAt(result);
                SuccessMessage = AccountL["ConfirmPhoneCodeSent"];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = PhoneConfirmationFailure(ex, out var resendAvailableAt);
            ResendAvailableAt = resendAvailableAt ?? ResendAvailableAt;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task OnSendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(SessionToken) || string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = string.IsNullOrWhiteSpace(SessionToken)
                ? AccountL["ConfirmPhoneSessionInvalid"]
                : L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await AccountAppService.SendPhoneConfirmationCodeAsync(new SendPhoneConfirmationCodeDto
            {
                SessionToken = SessionToken,
                PhoneNumber = PhoneNumber
            });
            ResendAvailableAt = AccountUiErrors.ToResendAvailableAt(result);
            NeedsPhoneNumber = false;
            SuccessMessage = AccountL["ConfirmPhoneCodeSent"];
        }
        catch (Exception ex)
        {
            SuccessMessage = null;
            ErrorMessage = PhoneConfirmationFailure(ex, out var resendAvailableAt);
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
        if (string.IsNullOrWhiteSpace(SessionToken) || string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = string.IsNullOrWhiteSpace(SessionToken)
                ? AccountL["ConfirmPhoneSessionInvalid"]
                : L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var result = await AccountAppService.ConfirmPhoneNumberAsync(new ConfirmPhoneNumberDto
            {
                SessionToken = SessionToken,
                Code = Code
            });
            await FinishAsync(result.EmailConfirmationStillRequired, result.Email);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex is BusinessException { Code: IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid }
                ? AccountL["ConfirmPhoneSessionInvalid"]
                : AccountUiErrors.LocalizedFailure(Logger, AccountL, ex, "ConfirmPhoneInvalid");
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

        if (ConfirmedUserId.HasValue && TokenStore.IsSupported)
        {
            var token = await TokenStore.CreateAsync(ConfirmedUserId.Value, PanelPath, rememberMe: false);
            Navigation.NavigateTo(
                $"/account/complete-login?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(PanelPath)}",
                forceLoad: true);
            return;
        }

        if (ConfirmedUserId.HasValue && HttpContext != null)
        {
            var user = await UserManager.FindByIdAsync(ConfirmedUserId.Value.ToString());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
                HttpContext.Response.Redirect(PanelPath);
                return;
            }
        }

        Navigation.NavigateTo("/account/login?phoneConfirmed=true", forceLoad: true);
    }

    protected virtual string PhoneConfirmationFailure(Exception exception, out DateTimeOffset? resendAvailableAt)
    {
        if (exception is BusinessException { Code: IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid })
        {
            resendAvailableAt = null;
            Logger.LogWarning(exception, "Phone confirmation session was rejected.");
            return AccountL["ConfirmPhoneSessionInvalid"];
        }

        return AccountUiErrors.OtpSendFailure(Logger, AccountL, exception, "ConfirmPhoneInvalid", out resendAvailableAt);
    }
}
