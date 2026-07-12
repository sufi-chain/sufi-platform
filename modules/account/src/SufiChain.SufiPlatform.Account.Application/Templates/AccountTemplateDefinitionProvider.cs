using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Account.Templates;
using SufiChain.SufiPlatform.SufiCom.Templates;
using SufiChain.SufiPlatform.TextTemplating;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Account;

public class AccountTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                AccountTemplates.Layout,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:AccountLayout"),
                layout: StandardMessageTemplates.Layout,
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/Layout.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.EmailConfirmation,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:EmailConfirmation"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/EmailConfirmation", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.PasswordReset,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:PasswordReset"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/PasswordReset", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.VerificationCode,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:VerificationCode"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCode", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.VerificationCodeSms,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:VerificationCodeSms"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCodeSms", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.VerificationCodeVoice,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:VerificationCodeVoice"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCodeVoice", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.OtpCodeSms,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:OtpCodeSms"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/OtpCodeSms", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.OtpCodeVoice,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:OtpCodeVoice"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/OtpCodeVoice", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.TwoFactorCodeSms,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:TwoFactorCodeSms"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/TwoFactorCodeSms", isInlineLocalized: true),

            new TemplateDefinition(
                AccountTemplates.TwoFactorCodeVoice,
                displayName: LocalizableString.Create<SufiAccountResource>("TextTemplate:TwoFactorCodeVoice"),
                localizationResource: typeof(SufiAccountResource)
            ).WithVirtualFilePath("/Templates/TwoFactorCodeVoice", isInlineLocalized: true)
        );
    }
}
