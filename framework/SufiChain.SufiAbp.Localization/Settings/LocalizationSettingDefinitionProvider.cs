using SufiChain.SufiAbp.UI.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Localization.Settings;

public class LocalizationSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                LocalizationSettingNames.DefaultLanguage,
                "en",
                L("DisplayName:SufiAbp.Localization.DefaultLanguage"),
                L("Description:SufiAbp.Localization.DefaultLanguage"),
                isVisibleToClients: true
            )
        );
    }

    private static Volo.Abp.Localization.LocalizableString L(string name)
    {
        return Volo.Abp.Localization.LocalizableString.Create<SufiAbpLocalizationResource>(name);
    }
}
