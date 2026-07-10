using SufiChain.SufiAbp.LocalizationManagement.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.LocalizationManagement.Settings;

public class LocalizationManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            LocalizationManagementSettingNames.DefaultCulture,
            defaultValue: "fa",
            displayName: L("Setting:Localization.DefaultCulture"),
            description: L("Setting:Localization.DefaultCulture.Description"),
            isVisibleToClients: true,
            isInherited: true));
    }

    private static LocalizableString L(string name) =>
        LocalizableString.Create<SufiAbpLocalizationManagementResource>(name);
}
