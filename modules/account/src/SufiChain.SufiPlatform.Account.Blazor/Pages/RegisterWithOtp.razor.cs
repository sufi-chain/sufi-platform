using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class RegisterWithOtp
{
    [Inject]
    protected IAccountOtpAppService OtpAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected ILogger<RegisterWithOtp> Logger { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected ISettingProvider SettingProvider { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    protected OtpOptionsDto? Options { get; set; }

    protected string Identifier { get; set; } = string.Empty;

    protected VerificationDeliveryChannel SelectedChannel { get; set; } = VerificationDeliveryChannel.Email;

    protected bool ShowChannelPicker =>
        Options?.AvailableChannels.Count > 1;

    protected string IdentifierLabel =>
        SelectedChannel.IsPhoneChannel() ? L["PhoneNumber"] : L["Email"];

    protected string IdentifierPlaceholder =>
        SelectedChannel.IsPhoneChannel() ? L["PhoneNumber"] : L["EnterEmail"];

    protected string IdentifierInputType =>
        SelectedChannel.IsPhoneChannel() ? "tel" : "email";

    protected string IdentifierAutoComplete =>
        SelectedChannel.IsPhoneChannel() ? "tel" : "email";

    protected string Code { get; set; } = string.Empty;

    protected string UserName { get; set; } = string.Empty;

    protected string EmailAddress { get; set; } = string.Empty;

    protected string Password { get; set; } = string.Empty;

    protected string? RegistrationToken { get; set; }

    protected string? CaptchaChallengeId { get; set; }

    protected string? CaptchaAnswer { get; set; }

    protected string? CaptchaToken { get; set; }

    protected int Step { get; set; } = 1;

    protected string? ErrorMessage { get; set; }

    protected string? SuccessMessage { get; set; }

    protected bool IsBusy { get; set; }

    protected DateTimeOffset? ResendAvailableAt { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Options = await OtpAppService.GetOtpOptionsAsync();
        if (Options is { IsEnabled: true, AllowRegistration: false })
        {
            ErrorMessage = AccountL["OtpRegistrationDisabled"];
        }

        if (Options is { IsEnabled: true })
        {
            SelectedChannel = Options.DefaultChannel;
        }
    }

    protected virtual string GetChannelLabel(VerificationDeliveryChannel channel) => channel switch
    {
        VerificationDeliveryChannel.Sms => AccountL["ChannelSms"],
        VerificationDeliveryChannel.Voice => AccountL["ChannelVoice"],
        _ => AccountL["ChannelEmail"]
    };

    protected virtual string GetChannelIcon(VerificationDeliveryChannel channel) => channel switch
    {
        VerificationDeliveryChannel.Sms => "chat",
        VerificationDeliveryChannel.Voice => "phone",
        _ => "mail"
    };

    protected Task OnChannelChangedAsync(VerificationDeliveryChannel channel)
    {
        SelectedChannel = channel;
        return Task.CompletedTask;
    }

    protected virtual async Task OnSendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Identifier))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var result = await OtpAppService.SendRegistrationOtpAsync(new SendOtpInput
            {
                Identifier = Identifier,
                Channel = SelectedChannel,
                AppName = "DemoApp",
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            ResendAvailableAt = AccountUiErrors.ToResendAvailableAt(result);
            Step = 2;
            SuccessMessage = AccountL["OtpCodeSent"];
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.OtpSendFailure(
                Logger,
                AccountL,
                ex,
                "OtpSendFailed",
                out var resendAvailableAt);
            ResendAvailableAt = resendAvailableAt ?? ResendAvailableAt;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Returns to the send step so a fresh captcha is solved before the next code is requested.
    /// </summary>
    protected virtual Task OnRequestNewCodeAsync()
    {
        Step = 1;
        Code = string.Empty;
        SuccessMessage = null;
        ErrorMessage = null;
        return Task.CompletedTask;
    }

    protected virtual async Task OnVerifyCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await OtpAppService.VerifyRegistrationOtpAsync(new VerifyOtpInput
            {
                Identifier = Identifier,
                Channel = SelectedChannel,
                Code = Code
            });

            RegistrationToken = result.RegistrationToken;
            Step = 3;
            SuccessMessage = AccountL["OtpVerifiedContinueRegistration"];
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "OtpVerifyFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual async Task OnRegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(RegistrationToken) ||
            string.IsNullOrWhiteSpace(UserName) ||
            string.IsNullOrWhiteSpace(Password) ||
            (SelectedChannel.IsPhoneChannel() && string.IsNullOrWhiteSpace(EmailAddress)))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var registered = await OtpAppService.RegisterWithOtpAsync(new RegisterWithOtpDto
            {
                RegistrationToken = RegistrationToken,
                UserName = UserName,
                EmailAddress = SelectedChannel.IsPhoneChannel() ? EmailAddress : Identifier,
                Password = Password,
                AppName = "DemoApp",
                ReturnUrl = ReturnUrl
            });

            if (!SelectedChannel.IsPhoneChannel() &&
                await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(SettingProvider))
            {
                if (string.IsNullOrWhiteSpace(registered.PhoneConfirmationToken))
                {
                    ErrorMessage = AccountL["ConfirmPhoneSessionInvalid"];
                    return;
                }

                Navigation.NavigateTo(
                    "/account/confirm-phone?token=" + Uri.EscapeDataString(registered.PhoneConfirmationToken),
                    forceLoad: true);
                return;
            }

            Navigation.NavigateTo("/account/login?registered=true", forceLoad: true);
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "RegistrationFailed");
        }
        finally
        {
            IsBusy = false;
        }
    }

}
