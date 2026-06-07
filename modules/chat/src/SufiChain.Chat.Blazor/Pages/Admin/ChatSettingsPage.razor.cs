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

    protected IChatAssistantConfigurationAppService AssistantConfigurationAppService =>
        LazyGetRequiredService(ref _assistantConfigurationAppService);
    private IChatAssistantConfigurationAppService? _assistantConfigurationAppService;

    protected IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    protected NavigationManager NavigationManager => LazyGetRequiredService(ref _navigationManager);
    private NavigationManager? _navigationManager;

    protected ChatSettingsDto Settings { get; set; } = new();

    protected ChatEmailConnectorSettingsDto EmailSettings { get; set; } = new();

    protected string? InboundPassword { get; set; }

    protected string? SmtpPassword { get; set; }

    protected ChatAssistantConfigurationDto? AssistantConfiguration { get; set; }

    protected List<ChatAssistantMappingDto> AssistantMappings { get; set; } = new();

    protected List<ChatAiWorkspaceOptionDto> WorkspaceOptions { get; set; } = new();

    protected string? SelectedWorkspaceName { get; set; }

    protected int ActiveTabIndex { get; set; }

    protected bool ShowAssistantsTab { get; set; }

    protected bool ShowEmailConnectorTab { get; set; }

    protected bool IsAssistantMappingDialogOpen { get; set; }

    protected bool IsCreatingAssistantMapping { get; set; }

    protected ChatAssistantMappingDto? EditingAssistantMapping { get; set; }

    protected int? EditingAssistantMappingIndex { get; set; }

    protected int AssistantMappingsPageIndex { get; set; }

    protected int AssistantMappingsPageSize { get; set; } = 10;

    protected sealed record TierEditor(string LabelKey, string HintKey, ChatUsageTierSettingsDto Dto);

    protected IReadOnlyList<TierEditor> TierEditors => new[]
    {
        new TierEditor("Tier:PublicAnonymous", "Tier:PublicAnonymous:Hint", Settings.PublicAnonymous),
        new TierEditor("Tier:PublicAuthenticated", "Tier:PublicAuthenticated:Hint", Settings.PublicAuthenticated),
        new TierEditor("Tier:Internal", "Tier:Internal:Hint", Settings.Internal)
    };

    protected static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
        public const string SaveAssistants = "save-assistants";
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
            AssistantConfiguration = await AssistantConfigurationAppService.GetAsync();
            ShowAssistantsTab =
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable) &&
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces) &&
                await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat);

            ShowEmailConnectorTab =
                await FeatureChecker.IsEnabledAsync(ChatFeatures.EmailConnector) &&
                EmailConnectorSettingsAppService != null;

            if (ShowEmailConnectorTab)
            {
                EmailSettings = await EmailConnectorSettingsAppService!.GetAsync();
                InboundPassword = null;
                SmtpPassword = null;
            }

            if (ShowAssistantsTab)
            {
                WorkspaceOptions = AssistantConfiguration?.WorkspaceOptions ?? new List<ChatAiWorkspaceOptionDto>();
                AssistantMappings = AssistantConfiguration?.Mappings
                    .Select(item => new ChatAssistantMappingDto
                    {
                        Key = item.Key,
                        DisplayName = item.DisplayName,
                        WorkspaceName = item.WorkspaceName,
                        IsEnabled = item.IsEnabled,
                        IsPublic = item.IsPublic,
                        IsWorkspaceHealthy = item.IsWorkspaceHealthy
                    })
                    .ToList() ?? new List<ChatAssistantMappingDto>();
            }

            SelectedWorkspaceName = AssistantConfiguration?.DefaultWorkspaceName;
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
                RealtimeEnabled = Settings.RealtimeEnabled,
                Attachments = Settings.Attachments
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

    protected virtual Task SaveAssistantsAsync()
    {
        return ExecuteWithLoadingAsync(async () =>
        {
            await AssistantConfigurationAppService.UpdateAsync(new UpdateChatAssistantConfigurationInput
            {
                DefaultWorkspaceName = SelectedWorkspaceName,
                Mappings = AssistantMappings
            });

            await Message.SuccessAsync(L["SettingsSavedSuccessfully"]);
            await LoadAsync();
        }, LoadingKeys.SaveAssistants);
    }

    protected IReadOnlyList<string> AssistantMappingExistingKeys =>
        AssistantMappings
            .Where((_, index) => !EditingAssistantMappingIndex.HasValue || index != EditingAssistantMappingIndex.Value)
            .Select(item => item.Key)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToList();

    protected virtual void OpenCreateAssistantMappingDialog()
    {
        IsCreatingAssistantMapping = true;
        EditingAssistantMapping = null;
        EditingAssistantMappingIndex = null;
        IsAssistantMappingDialogOpen = true;
    }

    protected virtual void OpenEditAssistantMappingDialog(ChatAssistantMappingDto mapping)
    {
        IsCreatingAssistantMapping = false;
        EditingAssistantMappingIndex = AssistantMappings.IndexOf(mapping);
        EditingAssistantMapping = new ChatAssistantMappingDto
        {
            Key = mapping.Key,
            DisplayName = mapping.DisplayName,
            WorkspaceName = mapping.WorkspaceName,
            IsEnabled = mapping.IsEnabled,
            IsPublic = mapping.IsPublic,
            IsWorkspaceHealthy = mapping.IsWorkspaceHealthy
        };
        IsAssistantMappingDialogOpen = true;
    }

    protected virtual Task OnAssistantMappingSavedAsync(ChatAssistantMappingDto mapping)
    {
        if (EditingAssistantMappingIndex.HasValue &&
            EditingAssistantMappingIndex.Value >= 0 &&
            EditingAssistantMappingIndex.Value < AssistantMappings.Count)
        {
            AssistantMappings[EditingAssistantMappingIndex.Value] = mapping;
        }
        else
        {
            AssistantMappings.Add(mapping);
        }

        IsAssistantMappingDialogOpen = false;
        EditingAssistantMapping = null;
        EditingAssistantMappingIndex = null;
        return Task.CompletedTask;
    }

    protected virtual async Task RemoveAssistantMappingAsync(ChatAssistantMappingDto mapping)
    {
        if (string.IsNullOrWhiteSpace(mapping.Key))
        {
            AssistantMappings.Remove(mapping);
            return;
        }

        var confirmed = await Message.ConfirmAsync(L["AssistantMappings:DeleteConfirm", mapping.Key]);
        if (!confirmed)
        {
            return;
        }

        AssistantMappings.Remove(mapping);
    }

    protected virtual Task OnAssistantMappingDialogOpenChangedAsync(bool open)
    {
        IsAssistantMappingDialogOpen = open;

        if (!open)
        {
            EditingAssistantMapping = null;
            EditingAssistantMappingIndex = null;
        }

        return Task.CompletedTask;
    }

    protected virtual Task OnAssistantMappingsPageIndexChangedAsync(int pageIndex)
    {
        AssistantMappingsPageIndex = pageIndex;
        return Task.CompletedTask;
    }

    protected virtual void OpenAiManagementWorkspaces()
    {
        NavigationManager.NavigateTo("/admin/ai-management/workspaces");
    }

    protected bool IsFileTypeAllowed(ChatAttachmentAllowedFileTypes type)
    {
        return Settings.Attachments.AllowedFileTypes.HasFlag(type);
    }

    protected void SetFileTypeAllowed(ChatAttachmentAllowedFileTypes type, bool enabled)
    {
        if (enabled)
        {
            Settings.Attachments.AllowedFileTypes |= type;
        }
        else
        {
            Settings.Attachments.AllowedFileTypes &= ~type;
        }
    }
}
