using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

[Authorize(ChatPermissions.Settings.Manage)]
public class ChatSettingsAppService : ChatAppService, IChatSettingsAppService
{
    protected ISettingManager SettingManager { get; }

    public ChatSettingsAppService(ISettingManager settingManager)
    {
        SettingManager = settingManager;
    }

    public virtual async Task<ChatSettingsDto> GetAsync()
    {
        return new ChatSettingsDto
        {
            MaxConcurrentOpenSessions = await GetIntAsync(ChatSettingNames.General.MaxConcurrentOpenSessions),
            MaxMessagesPerTenantPerDay = await GetIntAsync(ChatSettingNames.General.MaxMessagesPerTenantPerDay),
            PublicAnonymous = await GetAnonymousTierAsync(),
            PublicAuthenticated = await GetTierAsync(
                ChatSettingNames.Usage.PublicAuthenticated.MaxSessionsPerUserPerDay,
                ChatSettingNames.Usage.PublicAuthenticated.MaxMessagesPerSession,
                ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentsPerSession,
                ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentBytesPerSession,
                ChatSettingNames.Usage.PublicAuthenticated.LimitExceededAction),
            Internal = await GetTierAsync(
                ChatSettingNames.Usage.Internal.MaxSessionsPerUserPerDay,
                ChatSettingNames.Usage.Internal.MaxMessagesPerSession,
                ChatSettingNames.Usage.Internal.MaxAttachmentsPerSession,
                ChatSettingNames.Usage.Internal.MaxAttachmentBytesPerSession,
                ChatSettingNames.Usage.Internal.LimitExceededAction),
            AiEnabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled),
            AiUsageGuardEnabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.UsageGuard),
            MaxRepliesPerSession = await GetIntAsync(ChatSettingNames.Ai.MaxRepliesPerSession),
            MaxTokensPerSession = await GetIntAsync(ChatSettingNames.Ai.MaxTokensPerSession),
            MaxTokensPerTenantPerDay = await GetIntAsync(ChatSettingNames.Ai.MaxTokensPerTenantPerDay),
            MaxAnonymousAiSessionsPerHour = await GetIntAsync(ChatSettingNames.Ai.MaxAnonymousAiSessionsPerHour),
            MessageRetentionDays = await GetIntAsync(ChatSettingNames.Retention.MessageRetentionDays),
            ClosedSessionRetentionDays = await GetIntAsync(ChatSettingNames.Retention.ClosedSessionRetentionDays),
            UsageRecordRetentionDays = await GetIntAsync(ChatSettingNames.Retention.UsageRecordRetentionDays),
            RealtimeEnabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Realtime.Enabled),
            Attachments = await GetAttachmentSettingsAsync()
        };
    }

    public virtual async Task UpdateAsync(UpdateChatSettingsInput input)
    {
        await SetIntAsync(ChatSettingNames.General.MaxConcurrentOpenSessions, input.MaxConcurrentOpenSessions);
        await SetIntAsync(ChatSettingNames.General.MaxMessagesPerTenantPerDay, input.MaxMessagesPerTenantPerDay);

        await SetAnonymousTierAsync(input.PublicAnonymous);
        await SetTierAsync(
            ChatSettingNames.Usage.PublicAuthenticated.MaxSessionsPerUserPerDay,
            ChatSettingNames.Usage.PublicAuthenticated.MaxMessagesPerSession,
            ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentsPerSession,
            ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentBytesPerSession,
            ChatSettingNames.Usage.PublicAuthenticated.LimitExceededAction,
            input.PublicAuthenticated);

        await SetTierAsync(
            ChatSettingNames.Usage.Internal.MaxSessionsPerUserPerDay,
            ChatSettingNames.Usage.Internal.MaxMessagesPerSession,
            ChatSettingNames.Usage.Internal.MaxAttachmentsPerSession,
            ChatSettingNames.Usage.Internal.MaxAttachmentBytesPerSession,
            ChatSettingNames.Usage.Internal.LimitExceededAction,
            input.Internal);

        await SetBoolAsync(ChatSettingNames.Ai.Enabled, input.AiEnabled);
        await SetBoolAsync(ChatSettingNames.Ai.UsageGuard, input.AiUsageGuardEnabled);
        await SetIntAsync(ChatSettingNames.Ai.MaxRepliesPerSession, input.MaxRepliesPerSession);
        await SetIntAsync(ChatSettingNames.Ai.MaxTokensPerSession, input.MaxTokensPerSession);
        await SetIntAsync(ChatSettingNames.Ai.MaxTokensPerTenantPerDay, input.MaxTokensPerTenantPerDay);
        await SetIntAsync(ChatSettingNames.Ai.MaxAnonymousAiSessionsPerHour, input.MaxAnonymousAiSessionsPerHour);

        await SetIntAsync(ChatSettingNames.Retention.MessageRetentionDays, input.MessageRetentionDays);
        await SetIntAsync(ChatSettingNames.Retention.ClosedSessionRetentionDays, input.ClosedSessionRetentionDays);
        await SetIntAsync(ChatSettingNames.Retention.UsageRecordRetentionDays, input.UsageRecordRetentionDays);
        await SetBoolAsync(ChatSettingNames.Realtime.Enabled, input.RealtimeEnabled);
        await SetAttachmentSettingsAsync(input.Attachments);
    }

    protected virtual async Task<ChatAttachmentSettingsDto> GetAttachmentSettingsAsync()
    {
        return new ChatAttachmentSettingsDto
        {
            EnableFileAttachments = await SettingProvider.IsTrueAsync(ChatSettingNames.General.EnableFileAttachments),
            EnableLocationSharing = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableLocationSharing),
            EnableVoiceMessages = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableVoiceMessages),
            EnableOperatorGallery = await SettingProvider.IsTrueAsync(ChatSettingNames.Attachments.EnableOperatorGallery),
            MaxFilesPerMessage = await GetIntAsync(ChatSettingNames.Attachments.MaxFilesPerMessage),
            MaxVoiceRecordingSeconds = await GetIntAsync(ChatSettingNames.Attachments.MaxVoiceRecordingSeconds),
            AllowedFileTypes = ParseAllowedFileTypes(await SettingProvider.GetOrNullAsync(ChatSettingNames.Attachments.AllowedFileTypes))
        };
    }

    protected virtual async Task SetAttachmentSettingsAsync(ChatAttachmentSettingsDto dto)
    {
        await SetBoolAsync(ChatSettingNames.General.EnableFileAttachments, dto.EnableFileAttachments);
        await SetBoolAsync(ChatSettingNames.Attachments.EnableLocationSharing, dto.EnableLocationSharing);
        await SetBoolAsync(ChatSettingNames.Attachments.EnableVoiceMessages, dto.EnableVoiceMessages);
        await SetBoolAsync(ChatSettingNames.Attachments.EnableOperatorGallery, dto.EnableOperatorGallery);
        await SetIntAsync(ChatSettingNames.Attachments.MaxFilesPerMessage, dto.MaxFilesPerMessage);
        await SetIntAsync(ChatSettingNames.Attachments.MaxVoiceRecordingSeconds, dto.MaxVoiceRecordingSeconds);
        await SetIntAsync(ChatSettingNames.Attachments.AllowedFileTypes, (int)dto.AllowedFileTypes);
    }

    protected virtual ChatAttachmentAllowedFileTypes ParseAllowedFileTypes(string? value)
    {
        return int.TryParse(value, out var flags)
            ? (ChatAttachmentAllowedFileTypes)flags
            : ChatAttachmentAllowedFileTypes.All;
    }

    protected virtual async Task<ChatUsageTierSettingsDto> GetAnonymousTierAsync()
    {
        var dto = await GetTierAsync(
            ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerUserPerDay,
            ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerSession,
            ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentsPerSession,
            ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentBytesPerSession,
            ChatSettingNames.Usage.PublicAnonymous.LimitExceededAction);

        dto.EnableIpGuard = await SettingProvider.IsTrueAsync(ChatSettingNames.Usage.PublicAnonymous.EnableIpGuard);
        dto.MaxSessionsPerIpPerDay = await GetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerIpPerDay);
        dto.MaxMessagesPerIpPerDay = await GetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerIpPerDay);
        dto.MaxAiSessionsPerIpPerHour = await GetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxAiSessionsPerIpPerHour);
        dto.MaxMessagesBeforeSignupRequired = await GetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired);
        dto.MaxAiQuestionsBeforeSignupRequired = await GetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired);

        return dto;
    }

    protected virtual async Task SetAnonymousTierAsync(ChatUsageTierSettingsDto dto)
    {
        await SetTierAsync(
            ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerUserPerDay,
            ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerSession,
            ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentsPerSession,
            ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentBytesPerSession,
            ChatSettingNames.Usage.PublicAnonymous.LimitExceededAction,
            dto);

        await SetBoolAsync(ChatSettingNames.Usage.PublicAnonymous.EnableIpGuard, dto.EnableIpGuard);
        await SetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerIpPerDay, dto.MaxSessionsPerIpPerDay);
        await SetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerIpPerDay, dto.MaxMessagesPerIpPerDay);
        await SetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxAiSessionsPerIpPerHour, dto.MaxAiSessionsPerIpPerHour);
        await SetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired, dto.MaxMessagesBeforeSignupRequired);
        await SetIntAsync(ChatSettingNames.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired, dto.MaxAiQuestionsBeforeSignupRequired);
    }

    protected virtual async Task<ChatUsageTierSettingsDto> GetTierAsync(
        string maxSessionsKey,
        string maxMessagesKey,
        string maxAttachmentsKey,
        string maxAttachmentBytesKey,
        string limitExceededActionKey)
    {
        return new ChatUsageTierSettingsDto
        {
            MaxSessionsPerUserPerDay = await GetIntAsync(maxSessionsKey),
            MaxMessagesPerSession = await GetIntAsync(maxMessagesKey),
            MaxAttachmentsPerSession = await GetIntAsync(maxAttachmentsKey),
            MaxAttachmentBytesPerSession = await GetLongAsync(maxAttachmentBytesKey),
            LimitExceededAction = Enum.TryParse<LimitExceededAction>(
                await SettingProvider.GetOrNullAsync(limitExceededActionKey),
                out var action)
                ? action
                : LimitExceededAction.BlockSend
        };
    }

    protected virtual async Task SetTierAsync(
        string maxSessionsKey,
        string maxMessagesKey,
        string maxAttachmentsKey,
        string maxAttachmentBytesKey,
        string limitExceededActionKey,
        ChatUsageTierSettingsDto dto)
    {
        await SetIntAsync(maxSessionsKey, dto.MaxSessionsPerUserPerDay);
        await SetIntAsync(maxMessagesKey, dto.MaxMessagesPerSession);
        await SetIntAsync(maxAttachmentsKey, dto.MaxAttachmentsPerSession);
        await SetLongAsync(maxAttachmentBytesKey, dto.MaxAttachmentBytesPerSession);
        await SetAsync(limitExceededActionKey, dto.LimitExceededAction.ToString());
    }

    protected virtual async Task<int> GetIntAsync(string name)
    {
        return int.TryParse(await SettingProvider.GetOrNullAsync(name), out var value) ? value : 0;
    }

    protected virtual async Task<long> GetLongAsync(string name)
    {
        return long.TryParse(await SettingProvider.GetOrNullAsync(name), out var value) ? value : 0L;
    }

    protected virtual Task SetIntAsync(string name, int value)
    {
        return SetAsync(name, value.ToString());
    }

    protected virtual Task SetLongAsync(string name, long value)
    {
        return SetAsync(name, value.ToString());
    }

    protected virtual Task SetBoolAsync(string name, bool value)
    {
        return SetAsync(name, value.ToString().ToLowerInvariant());
    }

    protected virtual Task SetAsync(string name, string? value)
    {
        return SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value);
    }
}
