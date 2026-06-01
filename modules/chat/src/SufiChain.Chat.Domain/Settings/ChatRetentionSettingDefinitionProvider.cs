using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatRetentionSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ChatSettingNames.Retention.MessageRetentionDays, "365", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Retention.ClosedSessionRetentionDays, "365", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Retention.UsageRecordRetentionDays, "730", isVisibleToClients: false, isInherited: true));
    }
}
