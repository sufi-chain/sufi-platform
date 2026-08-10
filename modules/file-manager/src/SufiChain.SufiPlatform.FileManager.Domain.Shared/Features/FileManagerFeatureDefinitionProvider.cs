using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
using NumericValueValidator = Volo.Abp.Validation.StringValues.NumericValueValidator;
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

        group.AddFeature(
            SufiFileManagerFeatures.Storage.Provider,
            defaultValue: SufiFileManagerFeatures.Storage.DefaultProvider,
            displayName: L($"Feature:{SufiFileManagerFeatures.Storage.Provider}"),
            description: L($"Feature:{SufiFileManagerFeatures.Storage.Provider}.Description"),
            valueType: new SelectionStringValueType
            {
                ItemSource = new StaticSelectionStringValueItemSource(
                    ProviderItem(SufiFileManagerFeatures.Storage.Providers.Database),
                    ProviderItem(SufiFileManagerFeatures.Storage.Providers.FileSystem),
                    ProviderItem(SufiFileManagerFeatures.Storage.Providers.MinIO),
                    ProviderItem(SufiFileManagerFeatures.Storage.Providers.S3Provider))
            });

        group.AddFeature(
            SufiFileManagerFeatures.Storage.MaxBytes,
            defaultValue: SufiFileManagerFeatures.Storage.DefaultMaxBytes,
            displayName: L($"Feature:{SufiFileManagerFeatures.Storage.MaxBytes}"),
            description: L($"Feature:{SufiFileManagerFeatures.Storage.MaxBytes}.Description"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(0)));
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

    private static LocalizableSelectionStringValueItem ProviderItem(string provider)
    {
        return new LocalizableSelectionStringValueItem
        {
            Value = provider,
            DisplayText = new LocalizableStringInfo(
                "SufiFileManager",
                $"Feature:{SufiFileManagerFeatures.Storage.Provider}.{provider}")
        };
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiFileManagerResource>(name);
    }
}
