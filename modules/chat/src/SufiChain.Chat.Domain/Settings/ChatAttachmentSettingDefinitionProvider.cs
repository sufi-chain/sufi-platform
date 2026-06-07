using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatAttachmentSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                ChatSettingNames.Attachments.EnableLocationSharing,
                true.ToString().ToLowerInvariant(),
                isVisibleToClients: true,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.Attachments.EnableVoiceMessages,
                true.ToString().ToLowerInvariant(),
                isVisibleToClients: true,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.Attachments.MaxFilesPerMessage,
                ChatSettingDefaults.MaxFilesPerMessage.ToString(),
                isVisibleToClients: true,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.Attachments.MaxVoiceRecordingSeconds,
                ChatSettingDefaults.MaxVoiceRecordingSeconds.ToString(),
                isVisibleToClients: true,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.Attachments.AllowedFileTypes,
                ChatSettingDefaults.AllowedAttachmentFileTypes.ToString(),
                isVisibleToClients: true,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.Attachments.EnableOperatorGallery,
                true.ToString().ToLowerInvariant(),
                isVisibleToClients: true,
                isInherited: true));
    }
}
