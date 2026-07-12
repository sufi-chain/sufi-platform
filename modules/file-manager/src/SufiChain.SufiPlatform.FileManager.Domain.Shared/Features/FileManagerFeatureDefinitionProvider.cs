using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.FileManager.Features;

/// <summary>
/// Defines File Manager edition features.
/// </summary>
public class FileManagerFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiFileManagerFeatures.GroupName, L("Menu:SufiFileManager"));

        AddToggle(group, SufiFileManagerFeatures.Enable);
        AddToggle(group, SufiFileManagerFeatures.FileItems);
        AddToggle(group, SufiFileManagerFeatures.FileStructures);
        AddToggle(group, SufiFileManagerFeatures.StorageSettings);
        AddToggle(group, SufiFileManagerFeatures.Archiving);
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
        return LocalizableString.Create<SufiFileManagerResource>(name);
    }
}