using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatRealtimeSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ChatSettingNames.Realtime.Enabled, "true", isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Realtime.TypingIndicatorTtlSeconds, "15", isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Realtime.PresenceTtlSeconds, "60", isVisibleToClients: true, isInherited: true));
    }
}
