using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class LoginWithOtp
{
    [Inject]
    protected IAccountOtpAppService OtpAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    protected OtpOptionsDto? Options { get; set; }

    protected string Identifier { get; set; } = string.Empty;

    protected VerificationDeliveryChannel SelectedChannel { get; set; } = VerificationDeliveryChannel.Email;

    protected bool ShowChannelPicker =>
        Options?.AvailableChannels.Count > 1;

    protected string IdentifierLabel =>
        SelectedChannel.IsPhoneChannel()
            ? L["PhoneNumber"]
            : L["Email"];

    protected string IdentifierPlaceholder =>
        SelectedChannel.IsPhoneChannel()
            ? L["PhoneNumber"]
            : L["EnterEmail"];

    protected string IdentifierInputType =>
        SelectedChannel.IsPhoneChannel() ? "tel" : "email";

    protected string IdentifierAutoComplete =>
        SelectedChannel.IsPhoneChannel() ? "tel" : "email";

    protected string Code { get; set; } = string.Empty;

    protected bool RememberMe { get; set; }

    protected string? CaptchaChallengeId { get; set; }

    protected string? CaptchaAnswer { get; set; }

    protected string? CaptchaToken { get; set; }

    protected bool CodeSent { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string? SuccessMessage { get; set; }

    protected bool IsBusy { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Options = await OtpAppService.GetOtpOptionsAsync();
        if (Options is { IsEnabled: true, AllowLogin: false })
        {
            ErrorMessage = AccountL["OtpLoginDisabled"];
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
            await OtpAppService.SendLoginOtpAsync(new SendOtpInput
            {
                Identifier = Identifier,
                Channel = SelectedChannel,
                AppName = "DemoApp",
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            CodeSent = true;
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

    protected virtual async Task OnVerifyAsync()
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
            var result = await OtpAppService.VerifyLoginOtpAsync(new VerifyLoginOtpInput
            {
                Identifier = Identifier,
                Channel = SelectedChannel,
                Code = Code,
                ReturnUrl = ReturnUrl,
                RememberMe = RememberMe
            });

            var returnUrlEnc = Uri.EscapeDataString(result.ReturnUrl ?? ReturnUrl ?? "/");
            Navigation.NavigateTo(
                $"/account/complete-login?token={Uri.EscapeDataString(result.LoginCompletionToken)}&returnUrl={returnUrlEnc}",
                forceLoad: true);
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
}
