using SufiChain.SufiAbp.TagsManagement.Localization;
using SufiChain.SufiAbp.TagsManagement.Settings;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.TagsManagement.Settings;

/// <summary>
/// Defines Tags Management tenant settings.
/// </summary>
public class TagsManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                TagsManagementSettings.MaxTagsPerEntity,
                "10",
                displayName: L("Setting:TagsManagement.MaxTagsPerEntity"),
                description: L("Setting:TagsManagement.MaxTagsPerEntity.Description"),
                isVisibleToClients: true,
                isInherited: true),

            new SettingDefinition(
                TagsManagementSettings.MaxTagNameLength,
                Tags.TagConsts.MaxNameLength.ToString(),
                displayName: L("Setting:TagsManagement.MaxTagNameLength"),
                description: L("Setting:TagsManagement.MaxTagNameLength.Description"),
                isVisibleToClients: false,
                isInherited: true)
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpTagsManagementResource>(name);
    }
}
