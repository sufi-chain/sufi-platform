using SufiChain.SufiAbp.FileManager.Features;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.FileManager.Features;

/// <summary>
/// Defines File Manager edition features.
/// </summary>
public class FileManagerFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(SufiAbpFileManagerFeatures.GroupName, L("Menu:FileManager"));

        AddToggle(group, SufiAbpFileManagerFeatures.Enable);
        AddToggle(group, SufiAbpFileManagerFeatures.FileItems);
        AddToggle(group, SufiAbpFileManagerFeatures.FileStructures);
        AddToggle(group, SufiAbpFileManagerFeatures.StorageSettings);
        AddToggle(group, SufiAbpFileManagerFeatures.Archiving);
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
        return LocalizableString.Create<SufiAbpFileManagerResource>(name);
    }
}
