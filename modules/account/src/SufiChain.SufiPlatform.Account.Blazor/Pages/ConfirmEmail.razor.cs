using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp.Settings;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class ConfirmEmail : IDisposable
{
    private const string PanelPath = "/panel";
    private static readonly TimeSpan RedirectDelay = TimeSpan.FromSeconds(3);

    private readonly CancellationTokenSource _redirectCancellation = new();
    private int _redirectStarted;

    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected ILoginCompletionTokenStore TokenStore { get; set; } = default!;

    [Inject]
    protected IPhoneConfirmationSessionStore PhoneConfirmationSessions { get; set; } = default!;

    [Inject]
    protected SignInManager<IdentityUser> SignInManager { get; set; } = default!;

    [Inject]
    protected IdentityUserManager UserManager { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected ILogger<ConfirmEmail> Logger { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected ISettingProvider SettingProvider { get; set; } = default!;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    [SupplyParameterFromQuery]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery]
    public string? Token { get; set; }

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    protected bool IsProcessing { get; set; } = true;

    protected bool IsSuccess { get; set; }

    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(Token))
        {
            IsProcessing = false;
            ErrorMessage = AccountL["ConfirmEmailInvalidLink"];
            return;
        }

        try
        {
            var isValid = await AccountAppService.VerifyEmailConfirmationTokenAsync(
                new VerifyEmailConfirmationTokenInput
                {
                    UserId = UserId.Value,
                    ConfirmationToken = Token
                });

            if (!isValid)
            {
                ErrorMessage = AccountL["ConfirmEmailInvalidLink"];
                return;
            }

            await AccountAppService.ConfirmEmailAsync(new ConfirmEmailDto
            {
                UserId = UserId.Value,
                ConfirmationToken = Token
            });

            IsSuccess = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "ConfirmEmailFailed");
        }
        finally
        {
            IsProcessing = false;
        }

        if (!IsSuccess)
        {
            return;
        }

        if (await RedirectToPhoneConfirmationIfNeededAsync())
        {
            return;
        }

        await InvokeAsync(StateHasChanged);

        try
        {
            await Task.Delay(RedirectDelay, _redirectCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await ContinueAfterConfirmationAsync();
    }

    protected virtual async Task ContinueAfterConfirmationAsync()
    {
        if (!UserId.HasValue || Interlocked.Exchange(ref _redirectStarted, 1) == 1)
        {
            return;
        }

        if (await RedirectToPhoneConfirmationIfNeededAsync())
        {
            return;
        }

        if (TokenStore.IsSupported)
        {
            var token = await TokenStore.CreateAsync(UserId.Value, PanelPath, rememberMe: false);
            Navigation.NavigateTo(
                $"/account/complete-login?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(PanelPath)}",
                forceLoad: true);
            return;
        }

        if (HttpContext != null)
        {
            var user = await UserManager.FindByIdAsync(UserId.Value.ToString());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false);
                HttpContext.Response.Redirect(PanelPath);
                return;
            }
        }

        Navigation.NavigateTo("/account/login?emailConfirmed=true", forceLoad: true);
    }

    protected virtual async Task<bool> RedirectToPhoneConfirmationIfNeededAsync()
    {
        if (!UserId.HasValue || !await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(SettingProvider))
        {
            return false;
        }

        var user = await UserManager.FindByIdAsync(UserId.Value.ToString());
        if (user == null || user.PhoneNumberConfirmed)
        {
            return false;
        }

        if (!PhoneConfirmationSessions.IsSupported)
        {
            ErrorMessage = AccountL["ConfirmPhoneSessionInvalid"];
            return true;
        }

        var sessionToken = await PhoneConfirmationSessions.CreateAsync(user.Id);
        Navigation.NavigateTo(
            "/account/confirm-phone?token=" + Uri.EscapeDataString(sessionToken),
            forceLoad: true);
        return true;
    }

    public void Dispose()
    {
        _redirectCancellation.Cancel();
        _redirectCancellation.Dispose();
    }
}
