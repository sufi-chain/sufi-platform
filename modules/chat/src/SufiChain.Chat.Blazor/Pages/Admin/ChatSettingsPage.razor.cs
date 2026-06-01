using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.Features;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Settings.Manage)]
public partial class ChatSettingsPage : ChatComponentBase
{
    protected IChatSettingsAppService SettingsAppService => LazyGetRequiredService(ref _settingsAppService);
    private IChatSettingsAppService? _settingsAppService;

    protected IChatEmailConnectorSettingsAppService? EmailConnectorSettingsAppService =>
        LazyGetService(ref _emailConnectorSettingsAppService);
    private IChatEmailConnectorSettingsAppService? _emailConnectorSettingsAppService;

    protected IChatAiWorkspaceSelectionAppService WorkspaceSelectionAppService =>
        LazyGetRequiredService(ref _workspaceSelectionAppService);
    private IChatAiWorkspaceSelectionAppService? _workspaceSelectionAppService;

    protected IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    protected NavigationManager NavigationManager => LazyGetRequiredService(ref _navigationManager);
    private NavigationManager? _navigationManager;

    protected ChatSettingsDto Settings { get; set; } = new();

    protected ChatEmailConnectorSettingsDto EmailSettings { get; set; } = new();

    protected string? InboundPassword { get; set; }

    protected string? SmtpPassword { get; set; }

    protected ChatAiWorkspaceSelectionDto? WorkspaceSelection { get; set; }

    protected List<ChatAiWorkspaceOptionDto> WorkspaceOptions { get; set; } = new();

    protected string? SelectedWorkspaceName { get; set; }

    protected int ActiveTabIndex { get; set; }

    protected bool ShowWorkspaceSelector { get; set; }

    protected bool ShowEmailConnectorTab { get; set; }

    protected static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
        public const string SaveWorkspace = "save-workspace";
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected virtual async Task LoadAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            Settings = await SettingsAppService.GetAsync();
            WorkspaceSelection = await WorkspaceSelectionAppService.GetAsync();
            ShowWorkspaceSelector =
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable) &&
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces) &&
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat) &&
                WorkspaceSelection.IsAvailable;

            ShowEmailConnectorTab =
                await FeatureChecker.IsEnabledAsync(ChatFeatures.EmailConnector) &&
                EmailConnectorSettingsAppService != null;

            if (ShowEmailConnectorTab)
            {
                EmailSettings = await EmailConnectorSettingsAppService!.GetAsync();
                InboundPassword = null;
                SmtpPassword = null;
            }

            if (ShowWorkspaceSelector)
            {
                WorkspaceOptions = await WorkspaceSelectionAppService.GetOptionsAsync();
            }

            SelectedWorkspaceName = WorkspaceSelection.DefaultWorkspaceName;
        }, LoadingKeys.Load);
    }

    protected virtual Task SaveAsync()
    {
        return ExecuteWithLoadingAsync(async () =>
        {
            await SettingsAppService.UpdateAsync(new UpdateChatSettingsInput
            {
                MaxConcurrentOpenSessions = Settings.MaxConcurrentOpenSessions,
                MaxMessagesPerTenantPerDay = Settings.MaxMessagesPerTenantPerDay,
                PublicAnonymous = Settings.PublicAnonymous,
                PublicAuthenticated = Settings.PublicAuthenticated,
                Internal = Settings.Internal,
                AiEnabled = Settings.AiEnabled,
                AiUsageGuardEnabled = Settings.AiUsageGuardEnabled,
                MaxRepliesPerSession = Settings.MaxRepliesPerSession,
                MaxTokensPerSession = Settings.MaxTokensPerSession,
                MaxTokensPerTenantPerDay = Settings.MaxTokensPerTenantPerDay,
                MaxAnonymousAiSessionsPerHour = Settings.MaxAnonymousAiSessionsPerHour,
                MessageRetentionDays = Settings.MessageRetentionDays,
                ClosedSessionRetentionDays = Settings.ClosedSessionRetentionDays,
                UsageRecordRetentionDays = Settings.UsageRecordRetentionDays,
                RealtimeEnabled = Settings.RealtimeEnabled
            });

            if (ShowEmailConnectorTab && EmailConnectorSettingsAppService != null)
            {
                await EmailConnectorSettingsAppService.UpdateAsync(new UpdateChatEmailConnectorSettingsInput
                {
                    Enabled = EmailSettings.Enabled,
                    DefaultFromAddress = EmailSettings.DefaultFromAddress,
                    ReplyToAddress = EmailSettings.ReplyToAddress,
                    InboundProtocol = EmailSettings.InboundProtocol,
                    InboundHost = EmailSettings.InboundHost,
                    InboundPort = EmailSettings.InboundPort,
                    InboundUseSsl = EmailSettings.InboundUseSsl,
                    InboundUserName = EmailSettings.InboundUserName,
                    InboundPassword = InboundPassword,
                    SmtpHost = EmailSettings.SmtpHost,
                    SmtpPort = EmailSettings.SmtpPort,
                    SmtpUseSsl = EmailSettings.SmtpUseSsl,
                    SmtpUserName = EmailSettings.SmtpUserName,
                    SmtpPassword = SmtpPassword
                });
            }

            await Message.SuccessAsync(L["SettingsSavedSuccessfully"]);
            await LoadAsync();
        }, LoadingKeys.Save);
    }

    protected virtual Task SaveWorkspaceSelectionAsync()
    {
        return ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceSelectionAppService.UpdateDefaultAsync(new UpdateChatAiWorkspaceSelectionInput
            {
                DefaultWorkspaceName = SelectedWorkspaceName
            });

            await Message.SuccessAsync(L["SettingsSavedSuccessfully"]);
            await LoadAsync();
        }, LoadingKeys.SaveWorkspace);
    }

    protected virtual void OpenAiManagementWorkspaces()
    {
        NavigationManager.NavigateTo("/admin/ai-management/workspaces");
    }
}
