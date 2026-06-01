using SufiChain.Chat.Features;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.Features;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Connectors.Email.Settings;

public class ChatEmailConnectorSettingsReader : IChatEmailConnectorSettingsReader, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }
    protected IFeatureChecker FeatureChecker { get; }
    protected IChatEmailConnectorSettingsEncryption SettingsEncryption { get; }

    public ChatEmailConnectorSettingsReader(
        ISettingProvider settingProvider,
        IFeatureChecker featureChecker,
        IChatEmailConnectorSettingsEncryption settingsEncryption)
    {
        SettingProvider = settingProvider;
        FeatureChecker = featureChecker;
        SettingsEncryption = settingsEncryption;
    }

    public virtual async Task<ChatEmailConnectorRuntimeSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = new ChatEmailConnectorRuntimeSettings
        {
            Enabled = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.Enabled),
            DefaultFromAddress = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.DefaultFromAddress),
            ReplyToAddress = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.ReplyToAddress),
            InboundHost = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundHost),
            InboundPort = await GetIntAsync(ChatSettingNames.EmailConnector.InboundPort),
            InboundUseSsl = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.InboundUseSsl),
            InboundUserName = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundUserName),
            SmtpHost = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpHost),
            SmtpPort = await GetIntAsync(ChatSettingNames.EmailConnector.SmtpPort),
            SmtpUseSsl = await SettingProvider.IsTrueAsync(ChatSettingNames.EmailConnector.SmtpUseSsl),
            SmtpUserName = await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpUserName)
        };

        if (Enum.TryParse<ChatInboundEmailProtocol>(
                await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundProtocol),
                out var protocol))
        {
            settings.InboundProtocol = protocol;
        }

        if (await FeatureChecker.IsEnabledAsync(ChatFeatures.EmailConnector))
        {
            settings.InboundPassword = SettingsEncryption.Decrypt(
                await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.InboundPassword));
            settings.SmtpPassword = SettingsEncryption.Decrypt(
                await SettingProvider.GetOrNullAsync(ChatSettingNames.EmailConnector.SmtpPassword));
        }

        return settings;
    }

    protected virtual async Task<int> GetIntAsync(string name)
    {
        return int.TryParse(await SettingProvider.GetOrNullAsync(name), out var value) ? value : 0;
    }
}
