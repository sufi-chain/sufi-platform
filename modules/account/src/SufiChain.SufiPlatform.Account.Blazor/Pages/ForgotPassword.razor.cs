using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class ForgotPassword
{
    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromForm]
    public ForgotPasswordInputModel Input { get; set; } = new();

    protected string? CaptchaChallengeId { get; set; }

    protected string? CaptchaAnswer { get; set; }

    protected string? CaptchaToken { get; set; }

    protected string? SuccessMessage { get; set; }

    protected string? ErrorMessage { get; set; }

    protected bool IsSubmitting { get; set; }

    protected virtual async Task OnSubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Email))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        IsSubmitting = true;
        SuccessMessage = null;
        ErrorMessage = null;

        try
        {
            await AccountAppService.SendPasswordResetCodeAsync(new SendPasswordResetCodeDto
            {
                Email = Input.Email,
                AppName = "DemoApp",
                ReturnUrl = ReturnUrl,
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            SuccessMessage = AccountL["PasswordResetEmailSent"];
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

    public class ForgotPasswordInputModel
    {
        public string Email { get; set; } = string.Empty;
    }
}
