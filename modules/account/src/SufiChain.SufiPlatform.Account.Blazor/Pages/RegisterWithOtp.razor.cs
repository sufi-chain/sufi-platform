using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.Identity.Settings;
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
            await OtpAppService.SendRegistrationOtpAsync(new SendOtpInput
            {
                Identifier = Identifier,
                Channel = SelectedChannel,
                AppName = "DemoApp",
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            Step = 2;
            SuccessMessage = AccountL["OtpCodeSent"];
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
            ErrorMessage = ex.Message;
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
            await OtpAppService.RegisterWithOtpAsync(new RegisterWithOtpDto
            {
                RegistrationToken = RegistrationToken,
                UserName = UserName,
                EmailAddress = SelectedChannel.IsPhoneChannel() ? EmailAddress : Identifier,
                Password = Password,
                AppName = "DemoApp",
                ReturnUrl = ReturnUrl
            });

            if (await RequiresEmailConfirmationBeforeSignInAsync())
            {
                Navigation.NavigateTo(
                    $"/account/email-confirmation-sent?email={Uri.EscapeDataString(EmailAddress)}",
                    forceLoad: true);
                return;
            }

            Navigation.NavigateTo("/account/login?registered=true", forceLoad: true);
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

    protected virtual async Task<bool> RequiresEmailConfirmationBeforeSignInAsync()
    {
        return await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.RequireConfirmedAccount)
               || await SettingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail);
    }
}
