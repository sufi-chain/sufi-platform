using SufiChain.SufiAbp.MenuManagement.Features;
using SufiChain.SufiAbp.MenuManagement.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiAbp.MenuManagement.Features;

/// <summary>
/// Defines Menu Management edition features.
/// </summary>
public class MenuManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAbpMenuManagementFeatures.GroupName, L("MenuManagement"));

        AddToggle(group, SufiAbpMenuManagementFeatures.Enable);
        AddToggle(group, SufiAbpMenuManagementFeatures.Menus);
        AddToggle(group, SufiAbpMenuManagementFeatures.PublicMenus);
    }

    private static void AddToggle(FeatureGroupDefinition group, string name)
    {
        group.AddFeature(
            name,
            defaultValue: "true",
            displayName: L($"Feature:{name}"),
            description: L($"Feature:{name}.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpMenuManagementResource>(name);
    }
}
