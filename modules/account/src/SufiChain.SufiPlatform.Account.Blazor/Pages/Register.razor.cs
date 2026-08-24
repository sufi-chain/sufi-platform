using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Reflection;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;
using SufiChain.SufiPlatform;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.Identity.Settings;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

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
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

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

    protected int CaptchaResetVersion { get; set; }

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
            ErrorMessage = GetRegistrationErrorMessage(ex);
        }
        finally
        {
            CaptchaResetVersion++;
        }
    }

    protected virtual string GetRegistrationErrorMessage(Exception exception)
    {
        if (exception.InnerException != null &&
            IsGenericExceptionMessage(exception.Message))
        {
            var innerMessage = GetRegistrationErrorMessage(exception.InnerException);
            if (!string.Equals(innerMessage, AccountL["RegistrationFailed"].Value, StringComparison.Ordinal))
            {
                return innerMessage;
            }
        }

        if (exception is AbpIdentityResultException identityException)
        {
            return LocalizeIdentityErrors(identityException.IdentityResult.Errors);
        }

        var code = GetPropertyValue<string>(exception, "Code");
        var remoteError = GetPropertyValue<object>(exception, "Error");
        code ??= GetPropertyValue<string>(remoteError, "Code");
        code ??= exception.Data["Code"]?.ToString();

        if (!string.IsNullOrWhiteSpace(code))
        {
            var localized = L[code];
            if (!localized.ResourceNotFound)
            {
                return localized.Value;
            }
        }

        var remoteMessage = GetPropertyValue<string>(remoteError, "Message");
        if (!string.IsNullOrWhiteSpace(remoteMessage) && !IsGenericExceptionMessage(remoteMessage))
        {
            return remoteMessage;
        }

        var dataMessage = exception.Data["Message"]?.ToString();
        if (!string.IsNullOrWhiteSpace(dataMessage) && !IsGenericExceptionMessage(dataMessage))
        {
            return dataMessage;
        }

        if (!IsGenericExceptionMessage(exception.Message))
        {
            return exception.Message;
        }

        return AccountL["RegistrationFailed"];
    }

    private string LocalizeIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var messages = errors
            .Select(error =>
            {
                var localized = L[$"IdentityError:{error.Code}"];
                return localized.ResourceNotFound ? error.Description : localized.Value;
            })
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return messages.Length > 0
            ? string.Join(" ", messages)
            : AccountL["RegistrationFailed"];
    }

    private static T? GetPropertyValue<T>(object? instance, string propertyName)
    {
        if (instance == null)
        {
            return default;
        }

        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        return property?.GetValue(instance) is T value ? value : default;
    }

    private static bool IsGenericExceptionMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message) ||
               (message.StartsWith("Exception of type '", StringComparison.Ordinal) &&
                message.EndsWith("' was thrown.", StringComparison.Ordinal));
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
