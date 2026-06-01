using SufiChain.Chat;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatUsageSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerUserPerDay, ChatSettingDefaults.PublicAnonymousMaxSessionsPerUserPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerSession, ChatSettingDefaults.PublicAnonymousMaxMessagesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentsPerSession, ChatSettingDefaults.MaxAttachmentsPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentBytesPerSession, ChatSettingDefaults.MaxAttachmentBytesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.EnableIpGuard, "true", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerIpPerDay, ChatSettingDefaults.MaxSessionsPerIpPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerIpPerDay, ChatSettingDefaults.MaxMessagesPerIpPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxAiSessionsPerIpPerHour, ChatSettingDefaults.MaxAiSessionsPerIpPerHour.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired, ChatSettingDefaults.MaxMessagesBeforeSignupRequired.ToString(), isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired, ChatSettingDefaults.MaxAiQuestionsBeforeSignupRequired.ToString(), isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAnonymous.LimitExceededAction, LimitExceededAction.RequireAuthentication.ToString(), isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAuthenticated.MaxSessionsPerUserPerDay, ChatSettingDefaults.PublicAuthenticatedMaxSessionsPerUserPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAuthenticated.MaxMessagesPerSession, ChatSettingDefaults.PublicAuthenticatedMaxMessagesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentsPerSession, ChatSettingDefaults.MaxAttachmentsPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentBytesPerSession, ChatSettingDefaults.MaxAttachmentBytesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.PublicAuthenticated.LimitExceededAction, LimitExceededAction.BlockSend.ToString(), isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.Internal.MaxSessionsPerUserPerDay, ChatSettingDefaults.InternalMaxSessionsPerUserPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.Internal.MaxMessagesPerSession, ChatSettingDefaults.InternalMaxMessagesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.Internal.MaxAttachmentsPerSession, ChatSettingDefaults.MaxAttachmentsPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.Internal.MaxAttachmentBytesPerSession, ChatSettingDefaults.MaxAttachmentBytesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.Internal.LimitExceededAction, LimitExceededAction.BlockSend.ToString(), isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.GlobalFloor.MaxMessagesPerSession, "1", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Usage.GlobalFloor.MaxAttachmentBytesPerSession, "0", isVisibleToClients: false, isInherited: true));
    }
}
