using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.Tags.Settings;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Tags.Settings;

/// <summary>
/// Defines Tags Management tenant settings.
/// </summary>
public class TagsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                TagsSettings.MaxTagsPerEntity,
                "10",
                displayName: L("Setting:Tags.MaxTagsPerEntity"),
                description: L("Setting:Tags.MaxTagsPerEntity.Description"),
                isVisibleToClients: true,
                isInherited: true),

            new SettingDefinition(
                TagsSettings.MaxTagNameLength,
                Tags.TagConsts.MaxNameLength.ToString(),
                displayName: L("Setting:Tags.MaxTagNameLength"),
                description: L("Setting:Tags.MaxTagNameLength.Description"),
                isVisibleToClients: false,
                isInherited: true)
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiTagsResource>(name);
    }
}