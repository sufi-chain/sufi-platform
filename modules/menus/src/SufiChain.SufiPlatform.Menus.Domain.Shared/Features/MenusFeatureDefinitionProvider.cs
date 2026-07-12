using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Menus.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Menus.Features;

/// <summary>
/// Defines Menu Management edition features.
/// </summary>
public class MenusFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiMenusFeatures.GroupName, L("Menu:SufiMenus"));

        AddToggle(group, SufiMenusFeatures.Enable);
        AddToggle(group, SufiMenusFeatures.Menus);
        AddToggle(group, SufiMenusFeatures.PublicMenus);
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
        return LocalizableString.Create<SufiMenusResource>(name);
    }
}