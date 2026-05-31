using Volo.Abp.Localization;
using SufiChain.SufiAbp.Messaging.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Messaging.Settings;

public class MessagingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                MessagingSettingNames.Email.DefaultFromAddress,
                "noreply@example.com",
                L("DisplayName:SufiAbp.Messaging.Email.DefaultFromAddress"),
                L("Description:SufiAbp.Messaging.Email.DefaultFromAddress"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.DefaultFromDisplayName,
                "Application",
                L("DisplayName:SufiAbp.Messaging.Email.DefaultFromDisplayName"),
                L("Description:SufiAbp.Messaging.Email.DefaultFromDisplayName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpHost,
                "",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpHost"),
                L("Description:SufiAbp.Messaging.Email.SmtpHost"),
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpPort,
                "25",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpPort"),
                L("Description:SufiAbp.Messaging.Email.SmtpPort"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpUserName,
                "",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpUserName"),
                L("Description:SufiAbp.Messaging.Email.SmtpUserName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpPassword,
                "",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpPassword"),
                L("Description:SufiAbp.Messaging.Email.SmtpPassword"),
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpDomain,
                "",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpDomain"),
                L("Description:SufiAbp.Messaging.Email.SmtpDomain"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpEnableSsl,
                "false",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpEnableSsl"),
                L("Description:SufiAbp.Messaging.Email.SmtpEnableSsl"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Email.SmtpUseDefaultCredentials,
                "true",
                L("DisplayName:SufiAbp.Messaging.Email.SmtpUseDefaultCredentials"),
                L("Description:SufiAbp.Messaging.Email.SmtpUseDefaultCredentials"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Sms.DefaultFromNumber,
                "",
                L("DisplayName:SufiAbp.Messaging.Sms.DefaultFromNumber"),
                L("Description:SufiAbp.Messaging.Sms.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.Sms.ProviderName,
                "",
                L("DisplayName:SufiAbp.Messaging.Sms.ProviderName"),
                L("Description:SufiAbp.Messaging.Sms.ProviderName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.VoiceCall.DefaultFromNumber,
                "",
                L("DisplayName:SufiAbp.Messaging.VoiceCall.DefaultFromNumber"),
                L("Description:SufiAbp.Messaging.VoiceCall.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.VoiceCall.DefaultLanguage,
                "en-US",
                L("DisplayName:SufiAbp.Messaging.VoiceCall.DefaultLanguage"),
                L("Description:SufiAbp.Messaging.VoiceCall.DefaultLanguage"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.VoiceCall.DefaultVoiceGender,
                "Female",
                L("DisplayName:SufiAbp.Messaging.VoiceCall.DefaultVoiceGender"),
                L("Description:SufiAbp.Messaging.VoiceCall.DefaultVoiceGender"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                MessagingSettingNames.VoiceCall.ProviderName,
                "",
                L("DisplayName:SufiAbp.Messaging.VoiceCall.ProviderName"),
                L("Description:SufiAbp.Messaging.VoiceCall.ProviderName"),
                isVisibleToClients: false
            )
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MessagingResource>(name);
    }
}
