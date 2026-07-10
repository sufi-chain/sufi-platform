using Volo.Abp.Localization;
using SufiChain.SufiAbp.Communications.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Communications.Settings;

public class CommunicationsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                CommunicationsSettingNames.Email.DefaultFromAddress,
                "noreply@example.com",
                L("DisplayName:SufiAbp.Communication.Email.DefaultFromAddress"),
                L("Description:SufiAbp.Communication.Email.DefaultFromAddress"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.DefaultFromDisplayName,
                "Application",
                L("DisplayName:SufiAbp.Communication.Email.DefaultFromDisplayName"),
                L("Description:SufiAbp.Communication.Email.DefaultFromDisplayName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpHost,
                "",
                L("DisplayName:SufiAbp.Communication.Email.SmtpHost"),
                L("Description:SufiAbp.Communication.Email.SmtpHost"),
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpPort,
                "25",
                L("DisplayName:SufiAbp.Communication.Email.SmtpPort"),
                L("Description:SufiAbp.Communication.Email.SmtpPort"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpUserName,
                "",
                L("DisplayName:SufiAbp.Communication.Email.SmtpUserName"),
                L("Description:SufiAbp.Communication.Email.SmtpUserName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpPassword,
                "",
                L("DisplayName:SufiAbp.Communication.Email.SmtpPassword"),
                L("Description:SufiAbp.Communication.Email.SmtpPassword"),
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpDomain,
                "",
                L("DisplayName:SufiAbp.Communication.Email.SmtpDomain"),
                L("Description:SufiAbp.Communication.Email.SmtpDomain"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpEnableSsl,
                "false",
                L("DisplayName:SufiAbp.Communication.Email.SmtpEnableSsl"),
                L("Description:SufiAbp.Communication.Email.SmtpEnableSsl"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Email.SmtpUseDefaultCredentials,
                "true",
                L("DisplayName:SufiAbp.Communication.Email.SmtpUseDefaultCredentials"),
                L("Description:SufiAbp.Communication.Email.SmtpUseDefaultCredentials"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Sms.DefaultFromNumber,
                "",
                L("DisplayName:SufiAbp.Communication.Sms.DefaultFromNumber"),
                L("Description:SufiAbp.Communication.Sms.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.Sms.ProviderName,
                "",
                L("DisplayName:SufiAbp.Communication.Sms.ProviderName"),
                L("Description:SufiAbp.Communication.Sms.ProviderName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.VoiceCall.DefaultFromNumber,
                "",
                L("DisplayName:SufiAbp.Communication.VoiceCall.DefaultFromNumber"),
                L("Description:SufiAbp.Communication.VoiceCall.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.VoiceCall.DefaultLanguage,
                "en-US",
                L("DisplayName:SufiAbp.Communication.VoiceCall.DefaultLanguage"),
                L("Description:SufiAbp.Communication.VoiceCall.DefaultLanguage"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.VoiceCall.DefaultVoiceGender,
                "Female",
                L("DisplayName:SufiAbp.Communication.VoiceCall.DefaultVoiceGender"),
                L("Description:SufiAbp.Communication.VoiceCall.DefaultVoiceGender"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                CommunicationsSettingNames.VoiceCall.ProviderName,
                "",
                L("DisplayName:SufiAbp.Communication.VoiceCall.ProviderName"),
                L("Description:SufiAbp.Communication.VoiceCall.ProviderName"),
                isVisibleToClients: false
            )
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CommunicationsResource>(name);
    }
}
