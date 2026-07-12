using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class ResetPassword
{
    [Inject]
    protected IAccountAppService AccountAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiIdentityResource> L { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery]
    public Guid? UserId { get; set; }

    [SupplyParameterFromQuery]
    public string? Token { get; set; }

    [SupplyParameterFromForm]
    public ResetPasswordInputModel Input { get; set; } = new();

    protected bool IsTokenValid { get; set; }

    protected bool IsCheckingToken { get; set; } = true;

    protected string? ErrorMessage { get; set; }

    protected bool IsSubmitting { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(Token))
        {
            IsCheckingToken = false;
            ErrorMessage = AccountL["ResetPasswordInvalidLink"];
            return;
        }

        try
        {
            IsTokenValid = await AccountAppService.VerifyPasswordResetTokenAsync(
                new VerifyPasswordResetTokenInput
                {
                    UserId = UserId.Value,
                    ResetToken = Token
                });

            if (!IsTokenValid)
            {
                ErrorMessage = AccountL["ResetPasswordInvalidLink"];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsCheckingToken = false;
        }
    }

    protected virtual async Task OnSubmitAsync()
    {
        if (!UserId.HasValue || string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = AccountL["ResetPasswordInvalidLink"];
            return;
        }

        if (string.IsNullOrWhiteSpace(Input.Password) || string.IsNullOrWhiteSpace(Input.ConfirmPassword))
        {
            ErrorMessage = L["PleaseEnterAllFields"];
            return;
        }

        if (!string.Equals(Input.Password, Input.ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = AccountL["PasswordsDoNotMatch"];
            return;
        }

        IsSubmitting = true;
        ErrorMessage = null;

        try
        {
            await AccountAppService.ResetPasswordAsync(new ResetPasswordDto
            {
                UserId = UserId.Value,
                ResetToken = Token,
                Password = Input.Password
            });

            Navigation.NavigateTo("/account/login?passwordReset=true", forceLoad: true);
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

    public class ResetPasswordInputModel
    {
        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
