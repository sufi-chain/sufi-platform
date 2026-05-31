using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using IdentityUser = SufiChain.SufiAbp.Identity.IdentityUser;
using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.Identity.Settings;
using SufiChain.SufiAbp.UI.Abstractions.Account;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Account.Blazor.Pages;

public partial class Register
{
    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected SignInManager<IdentityUser> SignInManager { get; set; } = default!;

    [Inject]
    protected IdentityUserManager UserManager { get; set; } = default!;

    [Inject]
    protected IAuthenticationSchemeProvider SchemeProvider { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected ILoginCompletionTokenStore TokenStore { get; set; } = default!;

    [Inject]
    protected ISettingProvider SettingProvider { get; set; } = default!;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "isExternalLogin")]
    public bool IsExternalLogin { get; set; }

    [SupplyParameterFromQuery(Name = "externalLoginAuthSchema")]
    public string? ExternalLoginAuthSchema { get; set; }

    [SupplyParameterFromQuery(Name = "email")]
    public string? EmailFromQuery { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? ErrorFromQuery { get; set; }

    [SupplyParameterFromForm]
    public RegisterInputModel Input { get; set; } = new();

    protected string? ErrorMessage { get; set; }

    protected IList<AuthenticationScheme> ExternalSchemes { get; set; } = Array.Empty<AuthenticationScheme>();

    protected string? CaptchaChallengeId { get; set; }

    protected string? CaptchaAnswer { get; set; }

    protected string? CaptchaToken { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (IsExternalLogin && !string.IsNullOrEmpty(EmailFromQuery))
        {
            Input ??= new RegisterInputModel();
            Input.EmailAddress = EmailFromQuery;
        }

        if (!string.IsNullOrEmpty(ErrorFromQuery))
        {
            ErrorMessage = ErrorFromQuery;
        }

        var allSchemes = await SchemeProvider.GetAllSchemesAsync();
        ExternalSchemes = allSchemes.Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
    }

    protected virtual async Task OnRegisterAsync()
    {
        Input ??= new RegisterInputModel();

        if (string.IsNullOrWhiteSpace(Input.UserName) ||
            string.IsNullOrWhiteSpace(Input.EmailAddress) ||
            string.IsNullOrWhiteSpace(Input.Password))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        try
        {
            await AccountAppService.RegisterAsync(new RegisterDto
            {
                UserName = Input.UserName,
                EmailAddress = Input.EmailAddress,
                Password = Input.Password,
                AppName = "DemoApp",
                ReturnUrl = ReturnUrl,
                CaptchaChallengeId = CaptchaChallengeId,
                CaptchaAnswer = CaptchaAnswer,
                CaptchaToken = CaptchaToken
            });

            if (await RequiresEmailConfirmationBeforeSignInAsync())
            {
                Navigation.NavigateTo(
                    $"/account/email-confirmation-sent?email={Uri.EscapeDataString(Input.EmailAddress)}",
                    forceLoad: true);
                return;
            }

            var user = await UserManager.FindByNameAsync(Input.UserName);
            if (user != null)
            {
                if (TokenStore.IsSupported)
                {
                    var token = await TokenStore.CreateAsync(user.Id, ReturnUrl, rememberMe: false);
                    var returnUrlEnc = Uri.EscapeDataString(ReturnUrl ?? "/");
                    Navigation.NavigateTo(
                        $"/account/complete-login?token={Uri.EscapeDataString(token)}&returnUrl={returnUrlEnc}",
                        forceLoad: true);
                    return;
                }

                if (HttpContext != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                    HttpContext.Response.Redirect(ReturnUrl ?? "/");
                    return;
                }
            }

            Navigation.NavigateTo("/account/login?registered=true", forceLoad: true);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    protected virtual async Task<bool> RequiresEmailConfirmationBeforeSignInAsync()
    {
        return await SettingProvider.IsTrueAsync(IdentitySettingNames.Registration.RequireConfirmedAccount)
               || await SettingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail);
    }

    public class RegisterInputModel
    {
        public string UserName { get; set; } = string.Empty;

        public string EmailAddress { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
