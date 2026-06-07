using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Composer;

[Authorize]
public class ChatComposerCapabilitiesAppService : ChatAppService, IChatComposerCapabilitiesAppService
{
    protected IFeatureChecker FeatureChecker { get; }

    protected IPermissionChecker PermissionChecker { get; }

    public ChatComposerCapabilitiesAppService(
        IFeatureChecker featureChecker,
        IPermissionChecker permissionChecker)
    {
        FeatureChecker = featureChecker;
        PermissionChecker = permissionChecker;
    }

    public virtual async Task<ChatComposerCapabilitiesDto> GetAsync(Guid? sessionId = null)
    {
        var attachmentsEnabled = await IsAttachmentFeatureEnabledAsync();
        var richComposer = await CanUseRichComposerAsync();
        var operatorCopilot = await CanUseOperatorCopilotAsync(sessionId);

        var maxFiles = await SettingProvider.GetAsync<int>(ChatSettingNames.Attachments.MaxFilesPerMessage);
        if (maxFiles <= 0)
        {
            maxFiles = ChatSettingDefaults.MaxFilesPerMessage;
        }

        var maxVoiceSeconds = await SettingProvider.GetAsync<int>(ChatSettingNames.Attachments.MaxVoiceRecordingSeconds);
        if (maxVoiceSeconds <= 0)
        {
            maxVoiceSeconds = ChatSettingDefaults.MaxVoiceRecordingSeconds;
        }

        var allowedFileTypes = ParseAllowedFileTypes(
            await SettingProvider.GetOrNullAsync(ChatSettingNames.Attachments.AllowedFileTypes));

        var enableFileAttachments = await SettingProvider.IsTrueAsync(ChatSettingNames.General.EnableFileAttachments);
        var enableLocation = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableLocationSharing);
        var enableVoice = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableVoiceMessages);
        var enableGallery = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableOperatorGallery);

        var canPickGallery = richComposer &&
                             operatorCopilot &&
                             enableGallery &&
                             attachmentsEnabled &&
                             enableFileAttachments &&
                             await PermissionChecker.IsGrantedAsync(FileManagerPermissions.FileItems.Default);

        return new ChatComposerCapabilitiesDto
        {
            CanUseRichComposer = richComposer,
            CanAttachFiles = richComposer && attachmentsEnabled && enableFileAttachments,
            CanShareLocation = richComposer && enableLocation,
            CanRecordVoice = richComposer && enableVoice && attachmentsEnabled && enableFileAttachments,
            CanPickFromGallery = canPickGallery,
            CanUseOperatorCopilot = operatorCopilot,
            MaxFilesPerMessage = maxFiles,
            MaxVoiceRecordingSeconds = maxVoiceSeconds,
            AllowedFileTypes = allowedFileTypes
        };
    }

    protected virtual async Task<bool> IsAttachmentFeatureEnabledAsync()
    {
        return await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable) &&
               await FeatureChecker.IsEnabledAsync(ChatFeatures.Attachments);
    }

    protected virtual Task<bool> CanUseRichComposerAsync()
    {
        return Task.FromResult(CurrentUser.IsAuthenticated);
    }

    protected virtual async Task<bool> CanUseOperatorCopilotAsync(Guid? sessionId)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return false;
        }

        if (!await PermissionChecker.IsGrantedAsync(ChatPermissions.Inbox.Reply))
        {
            return false;
        }

        if (!await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled))
        {
            return false;
        }

        return await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat);
    }

    protected virtual ChatAttachmentAllowedFileTypes ParseAllowedFileTypes(string? value)
    {
        return int.TryParse(value, out var flags)
            ? (ChatAttachmentAllowedFileTypes)flags
            : ChatAttachmentAllowedFileTypes.All;
    }
}
