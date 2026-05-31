using SufiChain.SufiAbp.TagsManagement.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiAbp.TagsManagement.Features;

/// <summary>
/// Defines Tags Management edition features.
/// </summary>
public class TagsManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(TagsManagementFeatures.GroupName, L("TagsManagement"));

        group.AddFeature(
            TagsManagementFeatures.Names.Enable,
            defaultValue: "true",
            displayName: L("Feature:TagsManagement.Enable"),
            description: L("Feature:TagsManagement.Enable.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpTagsManagementResource>(name);
    }
}
