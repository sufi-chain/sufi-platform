using SufiChain.SufiPlatform.ShortLinks.Features;
using SufiChain.SufiPlatform.ShortLinks.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.ShortLinks.Features;

/// <summary>
/// Defines Short Link Generator edition features.
/// </summary>
public class ShortLinksFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiShortLinksFeatures.GroupName, L("Menu:SufiShortLinks"));

        AddToggle(group, SufiShortLinksFeatures.Enable);
        AddToggle(group, SufiShortLinksFeatures.ShortLinks);
        AddToggle(group, SufiShortLinksFeatures.Analytics);
        AddToggle(group, SufiShortLinksFeatures.PublicRedirect);
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
        return LocalizableString.Create<SufiShortLinksResource>(name);
    }
}