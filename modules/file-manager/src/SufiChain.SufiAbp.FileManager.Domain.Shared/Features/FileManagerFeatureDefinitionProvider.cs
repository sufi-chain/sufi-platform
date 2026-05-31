using SufiChain.SufiAbp.FileManager.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiAbp.FileManager.Features;

/// <summary>
/// Defines File Manager edition features.
/// </summary>
public class FileManagerFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(FileManagerFeatures.GroupName, L("Menu:FileManager"));

        group.AddFeature(
            FileManagerFeatures.Names.Enable,
            defaultValue: "true",
            displayName: L("Feature:FileManager.Enable"),
            description: L("Feature:FileManager.Enable.Description"),
            valueType: new ToggleStringValueType());

        group.AddFeature(
            FileManagerFeatures.Names.Archiving,
            defaultValue: "true",
            displayName: L("Feature:FileManager.Archiving"),
            description: L("Feature:FileManager.Archiving.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpFileManagerResource>(name);
    }
}
