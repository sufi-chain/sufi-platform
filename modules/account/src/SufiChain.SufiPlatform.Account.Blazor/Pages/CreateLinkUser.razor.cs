using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Blazor.Menus;
using SufiChain.SufiPlatform.Account.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

public partial class CreateLinkUser
{
    [Inject]
    protected IIdentityLinkUserAppService LinkUserAppService { get; set; } = default!;

    [Inject]
    protected ICurrentUser CurrentUser { get; set; } = default!;

    [Inject]
    protected ICurrentTenant CurrentTenant { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected ILogger<CreateLinkUser> Logger { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery]
    public Guid? LinkUserId { get; set; }

    [SupplyParameterFromQuery]
    public Guid? LinkTenantId { get; set; }

    [SupplyParameterFromQuery]
    public string? LinkToken { get; set; }

    protected bool IsProcessing { get; set; } = true;

    protected bool IsSuccess { get; set; }

    protected bool IsSameUser { get; set; }

    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (!LinkUserId.HasValue || string.IsNullOrWhiteSpace(LinkToken))
            {
                ErrorMessage = AccountL["InvalidLinkUserToken"];
                return;
            }

            if (CurrentUser.Id == null)
            {
                ErrorMessage = AccountL["LinkAccountFailed"];
                return;
            }

            if (CurrentUser.Id == LinkUserId && CurrentTenant.Id == LinkTenantId)
            {
                IsSameUser = true;
                return;
            }

            var verified = await LinkUserAppService.VerifyLinkTokenAsync(new VerifyLinkTokenInput
            {
                UserId = LinkUserId.Value,
                TenantId = LinkTenantId,
                Token = LinkToken
            });

            if (!verified)
            {
                ErrorMessage = AccountL["InvalidLinkUserToken"];
                return;
            }

            await LinkUserAppService.LinkAsync(new LinkUserInput
            {
                UserId = LinkUserId.Value,
                TenantId = LinkTenantId,
                Token = LinkToken
            });

            IsSuccess = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "LinkAccountFailed");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected virtual void SignInAsOtherAccount()
    {
        if (!LinkUserId.HasValue || string.IsNullOrWhiteSpace(LinkToken))
        {
            return;
        }

        var completionPath = LinkAccountHandshake.BuildCompletionPath(
            LinkUserId.Value,
            LinkTenantId,
            LinkToken);
        var loginPath = LinkAccountHandshake.BuildSecondaryLoginPath(completionPath);
        Navigation.NavigateTo(LinkAccountHandshake.BuildLogoutPath(loginPath), forceLoad: true);
    }

    protected virtual void GoToLinkedAccounts()
    {
        Navigation.NavigateTo(
            $"{AccountMenuContributor.ProfileBaseUrl}?tab=linked-accounts",
            forceLoad: true);
    }
}
