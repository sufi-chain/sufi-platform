using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Account.Templates;
using SufiChain.SufiAbp.Messaging.Templates;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.Account;

public class AccountTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                AccountTemplates.Layout,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:AccountLayout"),
                layout: StandardMessageTemplates.Layout,
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/Layout.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.EmailConfirmation,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:EmailConfirmation"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/EmailConfirmation.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.PasswordReset,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:PasswordReset"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/PasswordReset.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.VerificationCode,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:VerificationCode"),
                layout: AccountTemplates.Layout,
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCode.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.VerificationCodeSms,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:VerificationCodeSms"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCodeSms.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.VerificationCodeVoice,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:VerificationCodeVoice"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/VerificationCodeVoice.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.OtpCodeSms,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:OtpCodeSms"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/OtpCodeSms.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.OtpCodeVoice,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:OtpCodeVoice"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/OtpCodeVoice.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.TwoFactorCodeSms,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:TwoFactorCodeSms"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/TwoFactorCodeSms.tpl", isInlineLocalized: false),

            new TemplateDefinition(
                AccountTemplates.TwoFactorCodeVoice,
                displayName: LocalizableString.Create<SufiAbpAccountResource>("TextTemplate:TwoFactorCodeVoice"),
                localizationResource: typeof(SufiAbpAccountResource)
            ).WithVirtualFilePath("/Templates/TwoFactorCodeVoice.tpl", isInlineLocalized: false)
        );
    }
}
