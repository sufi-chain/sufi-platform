using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Account.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account.Blazor.Components.Profile;

public partial class LinkedAccountsSettings
{
    [Inject]
    protected IIdentityLinkUserAppService LinkUserAppService { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected ICurrentUser CurrentUser { get; set; } = default!;

    [Inject]
    protected ICurrentTenant CurrentTenant { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected ILogger<LinkedAccountsSettings> Logger { get; set; } = default!;

    private List<LinkUserDto> _links = new();
    private bool _loading = true;
    private bool _startingLink;
    private string? _errorMessage;
    private Guid? _busyUserId;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _errorMessage = null;
        try
        {
            var result = await LinkUserAppService.GetAllListAsync();
            _links = result.Items?.ToList() ?? new List<LinkUserDto>();
        }
        catch (Exception ex)
        {
            _errorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "LinkedAccountsLoadFailed");
            _links = new List<LinkUserDto>();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task StartLinkAsync()
    {
        if (CurrentUser.Id == null)
        {
            _errorMessage = AccountL["LinkAccountFailed"];
            return;
        }

        _startingLink = true;
        _errorMessage = null;
        try
        {
            var token = await LinkUserAppService.GenerateLinkTokenAsync();
            var completionPath = LinkAccountHandshake.BuildCompletionPath(
                CurrentUser.Id.Value,
                CurrentTenant.Id,
                token);
            var loginPath = LinkAccountHandshake.BuildSecondaryLoginPath(completionPath);
            Navigation.NavigateTo(LinkAccountHandshake.BuildLogoutPath(loginPath), forceLoad: true);
        }
        catch (Exception ex)
        {
            _errorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "LinkAccountFailed");
            _startingLink = false;
        }
    }

    private async Task LoginAsAsync(LinkUserDto link)
    {
        _busyUserId = link.TargetUserId;
        try
        {
            var token = await LinkUserAppService.GenerateLinkLoginTokenAsync();
            var url =
                $"/account/link-login" +
                $"?sourceLinkUserId={CurrentUser.Id}" +
                $"&sourceLinkTenantId={CurrentTenant.Id}" +
                $"&sourceLinkToken={Uri.EscapeDataString(token)}" +
                $"&targetLinkUserId={link.TargetUserId}" +
                $"&targetLinkTenantId={link.TargetTenantId}" +
                $"&returnUrl={Uri.EscapeDataString("/panel/dashboard")}";

            Navigation.NavigateTo(url, forceLoad: true);
        }
        catch (Exception ex)
        {
            _errorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "LinkLoginFailed");
            _busyUserId = null;
        }
    }

    private async Task UnlinkAsync(LinkUserDto link)
    {
        _busyUserId = link.TargetUserId;
        _errorMessage = null;
        try
        {
            await LinkUserAppService.UnlinkAsync(new UnLinkUserInput
            {
                UserId = link.TargetUserId,
                TenantId = link.TargetTenantId
            });
            await LoadAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = AccountUiErrors.LocalizedFailure(
                Logger,
                AccountL,
                ex,
                "LinkedAccountsLoadFailed");
        }
        finally
        {
            _busyUserId = null;
        }
    }

    private string GetTargetUserLabel(LinkUserDto link)
    {
        return link.TargetUserName ?? link.TargetUserId.ToString();
    }

    private string GetTargetTenantLabel(LinkUserDto link)
    {
        if (!string.IsNullOrEmpty(link.TargetTenantName))
        {
            return link.TargetTenantName;
        }

        if (link.TargetTenantId.HasValue)
        {
            return link.TargetTenantId.Value.ToString();
        }

        return AccountL["HostTenant"];
    }
}
