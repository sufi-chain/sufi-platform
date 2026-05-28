using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Identity.Localization;

namespace SufiChain.SufiAbp.Account.Blazor.Pages;

public partial class ConfirmEmail
{
    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAbpAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

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
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected virtual void NavigateToLogin()
    {
        var url = string.IsNullOrWhiteSpace(ReturnUrl)
            ? "/account/login?emailConfirmed=true"
            : $"/account/login?emailConfirmed=true&returnUrl={Uri.EscapeDataString(ReturnUrl)}";
        Navigation.NavigateTo(url, forceLoad: true);
    }
}
