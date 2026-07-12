using SufiChain.SufiPlatform.Localization.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Localization.Settings;

public class LocalizationSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            LocalizationSettingNames.DefaultCulture,
            defaultValue: "fa",
            displayName: L("Setting:Localization.DefaultCulture"),
            description: L("Setting:Localization.DefaultCulture.Description"),
            isVisibleToClients: true,
            isInherited: true));
    }

    private static LocalizableString L(string name) =>
        LocalizableString.Create<SufiLocalizationResource>(name);
}
