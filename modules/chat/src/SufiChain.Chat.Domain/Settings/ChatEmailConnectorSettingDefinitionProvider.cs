using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatEmailConnectorSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ChatSettingNames.EmailConnector.Enabled, "false", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.DefaultFromAddress, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.ReplyToAddress, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundProtocol, ChatInboundEmailProtocol.None.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundHost, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundPort, "993", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundUseSsl, "true", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundUserName, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.InboundPassword, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.SmtpHost, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.SmtpPort, "587", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.SmtpUseSsl, "true", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.SmtpUserName, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.EmailConnector.SmtpPassword, string.Empty, isVisibleToClients: false, isInherited: true));
    }
}
