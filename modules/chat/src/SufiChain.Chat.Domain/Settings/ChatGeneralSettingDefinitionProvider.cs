using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatGeneralSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                ChatSettingNames.General.MaxConcurrentOpenSessions,
                ChatSettingDefaults.MaxConcurrentOpenSessions.ToString(),
                isVisibleToClients: false,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.General.MaxMessagesPerTenantPerDay,
                ChatSettingDefaults.MaxMessagesPerTenantPerDay.ToString(),
                isVisibleToClients: false,
                isInherited: true),
            new SettingDefinition(
                ChatSettingNames.General.EnableFileAttachments,
                true.ToString().ToLowerInvariant(),
                isVisibleToClients: true,
                isInherited: true));
    }
}
