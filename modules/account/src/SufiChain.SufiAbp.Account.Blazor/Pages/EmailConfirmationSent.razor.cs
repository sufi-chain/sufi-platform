using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Identity.Localization;

namespace SufiChain.SufiAbp.Account.Blazor.Pages;

public partial class EmailConfirmationSent
{
    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpAccountResource> AccountL { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? Email { get; set; }

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    protected string ResendEmail { get; set; } = string.Empty;

    protected string? CaptchaChallengeId { get; set; }

    protected string? CaptchaAnswer { get; set; }

    protected string? CaptchaToken { get; set; }

    protected string? SuccessMessage { get; set; }

    protected string? ErrorMessage { get; set; }

    protected bool IsSubmitting { get; set; }

    protected override void OnInitialized()
    {
        ResendEmail = Email ?? string.Empty;
    }

    protected virtual async Task OnResendAsync()
    {
        if (string.IsNullOrWhiteSpace(ResendEmail))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsSubmitting = true;
        SuccessMessage = null;
        ErrorMessage = null;

        try
        {
            await AccountAppService.SendEmailConfirmationTokenAsync(new SendEmailConfirmationTokenDto
            {
                Email = ResendEmail,
                AppName = "DemoApp",
                ReturnUrl = ReturnUrl,
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            SuccessMessage = AccountL["EmailConfirmationResent"];
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
}
