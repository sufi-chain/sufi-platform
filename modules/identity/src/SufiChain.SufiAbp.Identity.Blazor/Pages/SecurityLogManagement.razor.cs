using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiAbp.Identity.Dtos;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.Identity.Blazor.Pages;

public partial class SecurityLogManagement : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadSecurityLogs = "load-security-logs";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IIdentitySecurityLogAppService SecurityLogAppService { get; set; } = default!;

    private List<SecurityLogListItemDto> _securityLogs = new();
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    // Filters
    private DateOnly? _startDate;
    private DateOnly? _endDate;
    private string? _userName;
    private string? _action;
    private string? _clientIpAddress;
    private string? _applicationName;

    protected override void OnInitialized()
    {
        SetupPageLayout();
        _startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        _endDate = DateOnly.FromDateTime(DateTime.Today);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadSecurityLogsAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["SecurityLogs"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadSecurityLogsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new GetSecurityLogListInput
        {
            StartTime = _startDate?.ToDateTime(TimeOnly.MinValue),
            EndTime = _endDate?.ToDateTime(TimeOnly.MaxValue),
            UserName = _userName,
            Action = _action,
            ClientIpAddress = _clientIpAddress,
            ApplicationName = _applicationName,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize,
            Sorting = "CreationTime DESC"
        };

        var result = await SecurityLogAppService.GetListAsync(input);
        _securityLogs = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadSecurityLogs);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadSecurityLogsAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        _pageIndex = 0;
        await LoadSecurityLogsAsync();
    }

    private async Task ClearFiltersAsync()
    {
        _startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        _endDate = DateOnly.FromDateTime(DateTime.Today);
        _userName = null;
        _action = null;
        _clientIpAddress = null;
        _applicationName = null;
        _pageIndex = 0;
        await LoadSecurityLogsAsync();
    }

    private SbColor GetActionColor(string? action)
    {
        return action switch
        {
            SecurityLogActions.LoginSucceeded => SbColor.Success,
            SecurityLogActions.LoginFailed or 
            SecurityLogActions.LoginInvalidUserNameOrPassword or 
            SecurityLogActions.LoginInvalidUserName => SbColor.Danger,
            SecurityLogActions.Logout => SbColor.Info,
            SecurityLogActions.LoginLockedout => SbColor.Warning,
            SecurityLogActions.ChangePassword or 
            SecurityLogActions.ChangeEmail or 
            SecurityLogActions.ChangePhoneNumber or 
            SecurityLogActions.ChangeUserName => SbColor.Primary,
            SecurityLogActions.TwoFactorEnabled or 
            SecurityLogActions.TwoFactorDisabled => SbColor.Secondary,
            _ => SbColor.Default
        };
    }

    private string GetActionDisplayName(string? action)
    {
        return action switch
        {
            SecurityLogActions.LoginSucceeded => L["LoginSucceeded"],
            SecurityLogActions.LoginFailed => L["LoginFailed"],
            SecurityLogActions.LoginInvalidUserName => L["LoginInvalidUserName"],
            SecurityLogActions.LoginInvalidUserNameOrPassword => L["LoginInvalidCredentials"],
            SecurityLogActions.LoginLockedout => L["LoginLockedout"],
            SecurityLogActions.LoginRequiresTwoFactor => L["LoginRequiresTwoFactor"],
            SecurityLogActions.LoginNotAllowed => L["LoginNotAllowed"],
            SecurityLogActions.Logout => L["Logout"],
            SecurityLogActions.ChangePassword => L["ChangePassword"],
            SecurityLogActions.ChangeEmail => L["ChangeEmail"],
            SecurityLogActions.ChangePhoneNumber => L["ChangePhoneNumber"],
            SecurityLogActions.ChangeUserName => L["ChangeUserName"],
            SecurityLogActions.TwoFactorEnabled => L["TwoFactorEnabled"],
            SecurityLogActions.TwoFactorDisabled => L["TwoFactorDisabled"],
            _ => action ?? L["Unknown"]
        };
    }
}

/// <summary>
/// Security log action constants used for filtering and display.
/// These match ABP's IdentitySecurityLogActionConsts.
/// </summary>
public static class SecurityLogActions
{
    public const string LoginSucceeded = "LoginSucceeded";
    public const string LoginFailed = "LoginFailed";
    public const string LoginInvalidUserName = "LoginInvalidUserName";
    public const string LoginInvalidUserNameOrPassword = "LoginInvalidUserNameOrPassword";
    public const string LoginLockedout = "LoginLockedout";
    public const string LoginRequiresTwoFactor = "LoginRequiresTwoFactor";
    public const string LoginNotAllowed = "LoginNotAllowed";
    public const string Logout = "Logout";
    public const string ChangePassword = "ChangePassword";
    public const string ChangeEmail = "ChangeEmail";
    public const string ChangePhoneNumber = "ChangePhoneNumber";
    public const string ChangeUserName = "ChangeUserName";
    public const string TwoFactorEnabled = "TwoFactorEnabled";
    public const string TwoFactorDisabled = "TwoFactorDisabled";
}
