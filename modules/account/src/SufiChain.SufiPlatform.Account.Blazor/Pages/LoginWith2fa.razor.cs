using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;
using SufiChain.SufiPlatform.Identity.AspNetCore;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class LoginWith2fa
{
    [Inject]
    protected IAccountTwoFactorAppService TwoFactorAppService { get; set; } = default!;

    [Inject]
    protected SignInManager<IdentityUser> SignInManager { get; set; } = default!;

    [Inject]
    protected IAccountSecurityLogAppService SecurityLogAppService { get; set; } = default!;

    [Inject]
    protected ITwoFactorPendingLoginStore PendingLoginStore { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    public string? PendingToken { get; set; }

    [SupplyParameterFromForm]
    public LoginWith2faInputModel Input { get; set; } = new();

    protected TwoFactorLoginOptionsDto? Options { get; set; }

    protected bool UseInteractiveFlow { get; set; }

    protected bool RememberMe { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SuccessMessage { get; set; }

    protected bool IsSubmitting { get; set; }

    protected bool IsSendingCode { get; set; }

    protected string SelectedProvider { get; set; } = TwoFactorProviderNames.Authenticator;

    protected VerificationDeliveryChannel SelectedCodeChannel { get; set; } = VerificationDeliveryChannel.Email;

    protected bool ShowCodeChannelPicker =>
        Options?.AvailableCodeChannels.Count > 1;

    protected override async Task OnInitializedAsync()
    {
        UseInteractiveFlow = !string.IsNullOrWhiteSpace(PendingToken) && PendingLoginStore.IsSupported;

        if (UseInteractiveFlow)
        {
            var pending = await PendingLoginStore.GetAsync(PendingToken!);
            if (pending == null)
            {
                ErrorMessage = AccountL["TwoFactorSessionExpired"];
                return;
            }

            RememberMe = pending.Value.rememberMe;
            ReturnUrl ??= pending.Value.returnUrl;
        }
        else if (await SignInManager.GetTwoFactorAuthenticationUserAsync() == null)
        {
            ErrorMessage = AccountL["TwoFactorSessionExpired"];
            return;
        }

        Options = await TwoFactorAppService.GetLoginOptionsAsync();

        if (Options.AllowAuthenticatorApp)
        {
            SelectedProvider = TwoFactorProviderNames.Authenticator;
        }
        else if (Options.AllowCodeDelivery)
        {
            SelectedCodeChannel = Options.AvailableCodeChannels.FirstOrDefault();
            SelectedProvider = GetProviderForChannel(SelectedCodeChannel);
        }
    }

    protected virtual string GetChannelLabel(VerificationDeliveryChannel channel) => channel switch
    {
        VerificationDeliveryChannel.Sms => AccountL["ChannelSms"],
        VerificationDeliveryChannel.Voice => AccountL["ChannelVoice"],
        _ => AccountL["ChannelEmail"]
    };

    protected virtual string GetProviderForChannel(VerificationDeliveryChannel channel) =>
        channel == VerificationDeliveryChannel.Email
            ? TwoFactorProviderNames.Email
            : TwoFactorProviderNames.Phone;

    protected virtual async Task OnSendCodeAsync()
    {
        if (Options?.AllowCodeDelivery != true)
        {
            return;
        }

        IsSendingCode = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await TwoFactorAppService.SendTwoFactorCodeAsync(new SendTwoFactorCodeInput
            {
                PendingToken = PendingToken,
                PreferredChannel = SelectedCodeChannel,
                AppName = "DemoApp"
            });

            SuccessMessage = AccountL["TwoFactorCodeSent"];
            SelectedProvider = GetProviderForChannel(SelectedCodeChannel);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSendingCode = false;
        }
    }

    protected virtual async Task OnVerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            if (UseInteractiveFlow)
            {
                await CompleteInteractiveLoginAsync();
                return;
            }

            await CompleteSsrLoginAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    protected virtual async Task CompleteInteractiveLoginAsync()
    {
        var result = await TwoFactorAppService.CompleteTwoFactorLoginAsync(new CompleteTwoFactorLoginInput
        {
            PendingToken = PendingToken!,
            Code = Input.Code,
            Provider = SelectedProvider,
            RecoveryCode = Input.RecoveryCode
        });

        var returnUrlEnc = Uri.EscapeDataString(result.ReturnUrl ?? ReturnUrl ?? "/");
        Navigation.NavigateTo(
            $"/account/complete-login?token={Uri.EscapeDataString(result.LoginCompletionToken)}&returnUrl={returnUrlEnc}",
            forceLoad: true);
    }

    protected virtual async Task CompleteSsrLoginAsync()
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            ErrorMessage = AccountL["TwoFactorSessionExpired"];
            return;
        }

        SignInResult result;

        if (!string.IsNullOrWhiteSpace(Input.RecoveryCode))
        {
            result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(Input.RecoveryCode);
        }
        else if (string.Equals(SelectedProvider, TwoFactorProviderNames.Email, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(SelectedProvider, TwoFactorProviderNames.Phone, StringComparison.OrdinalIgnoreCase))
        {
            result = await SignInManager.TwoFactorSignInAsync(
                SelectedProvider,
                Input.Code,
                RememberMe,
                rememberClient: false);
        }
        else
        {
            result = await SignInManager.TwoFactorAuthenticatorSignInAsync(Input.Code, RememberMe, rememberClient: false);
        }

        if (result.Succeeded)
        {
            await SecurityLogAppService.SaveLoginEventAsync(
                IdentitySecurityLogIdentityConsts.IdentityTwoFactor,
                IdentitySecurityLogActionConsts.LoginSucceeded,
                user.UserName);

            var enforceUrl = await TwoFactorAppService.GetPostLoginRedirectUrlAsync(user.Id, ReturnUrl);
            var target = !string.IsNullOrEmpty(enforceUrl) ? enforceUrl : (ReturnUrl ?? "/");

            if (HttpContext != null)
            {
                HttpContext.Response.Redirect(target);
                return;
            }

            Navigation.NavigateTo(target, forceLoad: true);
            return;
        }

        await SecurityLogAppService.SaveLoginEventAsync(
            IdentitySecurityLogIdentityConsts.IdentityTwoFactor,
            result.ToIdentitySecurityLogAction(),
            user.UserName);

        if (result.IsLockedOut)
        {
            ErrorMessage = L["AccountLockedOut"];
        }
        else
        {
            ErrorMessage = AccountL["TwoFactorCodeInvalid"];
        }
    }

    public class LoginWith2faInputModel
    {
        public string Code { get; set; } = string.Empty;

        public string? RecoveryCode { get; set; }
    }
}
