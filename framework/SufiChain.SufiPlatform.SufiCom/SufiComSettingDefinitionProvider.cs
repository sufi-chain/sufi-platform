using Volo.Abp.Localization;
using SufiChain.SufiPlatform.SufiCom.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiCom.Settings;

public class SufiComSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                SufiComSenderSettingNames.Email.DefaultFromAddress,
                "noreply@example.com",
                L("DisplayName:SufiCom.Email.DefaultFromAddress"),
                L("Description:SufiCom.Email.DefaultFromAddress"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.DefaultFromDisplayName,
                "Application",
                L("DisplayName:SufiCom.Email.DefaultFromDisplayName"),
                L("Description:SufiCom.Email.DefaultFromDisplayName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpHost,
                "",
                L("DisplayName:SufiCom.Email.SmtpHost"),
                L("Description:SufiCom.Email.SmtpHost"),
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpPort,
                "25",
                L("DisplayName:SufiCom.Email.SmtpPort"),
                L("Description:SufiCom.Email.SmtpPort"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpUserName,
                "",
                L("DisplayName:SufiCom.Email.SmtpUserName"),
                L("Description:SufiCom.Email.SmtpUserName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpPassword,
                "",
                L("DisplayName:SufiCom.Email.SmtpPassword"),
                L("Description:SufiCom.Email.SmtpPassword"),
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpDomain,
                "",
                L("DisplayName:SufiCom.Email.SmtpDomain"),
                L("Description:SufiCom.Email.SmtpDomain"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpEnableSsl,
                "false",
                L("DisplayName:SufiCom.Email.SmtpEnableSsl"),
                L("Description:SufiCom.Email.SmtpEnableSsl"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Email.SmtpUseDefaultCredentials,
                "true",
                L("DisplayName:SufiCom.Email.SmtpUseDefaultCredentials"),
                L("Description:SufiCom.Email.SmtpUseDefaultCredentials"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Sms.DefaultFromNumber,
                "",
                L("DisplayName:SufiCom.Sms.DefaultFromNumber"),
                L("Description:SufiCom.Sms.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.Sms.ProviderName,
                "",
                L("DisplayName:SufiCom.Sms.ProviderName"),
                L("Description:SufiCom.Sms.ProviderName"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.VoiceCall.DefaultFromNumber,
                "",
                L("DisplayName:SufiCom.VoiceCall.DefaultFromNumber"),
                L("Description:SufiCom.VoiceCall.DefaultFromNumber"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.VoiceCall.DefaultLanguage,
                "en-US",
                L("DisplayName:SufiCom.VoiceCall.DefaultLanguage"),
                L("Description:SufiCom.VoiceCall.DefaultLanguage"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.VoiceCall.DefaultVoiceGender,
                "Female",
                L("DisplayName:SufiCom.VoiceCall.DefaultVoiceGender"),
                L("Description:SufiCom.VoiceCall.DefaultVoiceGender"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                SufiComSenderSettingNames.VoiceCall.ProviderName,
                "",
                L("DisplayName:SufiCom.VoiceCall.ProviderName"),
                L("Description:SufiCom.VoiceCall.ProviderName"),
                isVisibleToClients: false
            )
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiComResource>(name);
    }
}
