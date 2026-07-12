using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Tags.Features;

/// <summary>
/// Defines Tags Management edition features.
/// </summary>
public class TagsFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiTagsFeatures.GroupName, L("Menu:SufiTags"));

        AddToggle(group, SufiTagsFeatures.Enable);
        AddToggle(group, SufiTagsFeatures.Tags);
        AddToggle(group, SufiTagsFeatures.TagLinks);
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
        return LocalizableString.Create<SufiTagsResource>(name);
    }
}