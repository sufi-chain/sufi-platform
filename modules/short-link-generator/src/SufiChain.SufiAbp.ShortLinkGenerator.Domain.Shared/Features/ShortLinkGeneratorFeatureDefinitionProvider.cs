using SufiChain.SufiAbp.ShortLinkGenerator.Features;
using SufiChain.SufiAbp.ShortLinkGenerator.Localization;
using SufiChain.SufiAbp.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.ShortLinkGenerator.Features;

/// <summary>
/// Defines Short Link Generator edition features.
/// </summary>
public class ShortLinkGeneratorFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAbpShortLinkGeneratorFeatures.GroupName, L("Menu:ShortLinkGenerator"));

        AddToggle(group, SufiAbpShortLinkGeneratorFeatures.Enable);
        AddToggle(group, SufiAbpShortLinkGeneratorFeatures.ShortLinks);
        AddToggle(group, SufiAbpShortLinkGeneratorFeatures.Analytics);
        AddToggle(group, SufiAbpShortLinkGeneratorFeatures.PublicRedirect);
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
        return LocalizableString.Create<SufiAbpShortLinkGeneratorResource>(name);
    }
}
