using SufiChain.SufiAbp.TagsManagement.Features;
using SufiChain.SufiAbp.TagsManagement.Localization;
using SufiChain.SufiAbp.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.TagsManagement.Features;

/// <summary>
/// Defines Tags Management edition features.
/// </summary>
public class TagsManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAbpTagsManagementFeatures.GroupName, L("TagsManagement"));

        AddToggle(group, SufiAbpTagsManagementFeatures.Enable);
        AddToggle(group, SufiAbpTagsManagementFeatures.Tags);
        AddToggle(group, SufiAbpTagsManagementFeatures.TagLinks);
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
        return LocalizableString.Create<SufiAbpTagsManagementResource>(name);
    }
}
