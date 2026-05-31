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
        var group = context.AddGroup(MenuManagementFeatures.GroupName, L("MenuManagement"));

        group.AddFeature(
            MenuManagementFeatures.Names.Enable,
            defaultValue: "true",
            displayName: L("Feature:MenuManagement.Enable"),
            description: L("Feature:MenuManagement.Enable.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpMenuManagementResource>(name);
    }
}
