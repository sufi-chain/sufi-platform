using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Localization;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;
using AbpIStringValueType = Volo.Abp.Validation.StringValues.IStringValueType;

namespace SufiChain.SufiPlatform.Features;

public class FeatureDefinitionSerializer : IFeatureDefinitionSerializer, ITransientDependency
{
    protected IGuidGenerator GuidGenerator { get; }
    protected ILocalizableStringSerializer LocalizableStringSerializer { get; }
    protected StringValueTypeSerializer StringValueTypeSerializer { get; }

    public FeatureDefinitionSerializer(
        IGuidGenerator guidGenerator,
        ILocalizableStringSerializer localizableStringSerializer,
        StringValueTypeSerializer stringValueTypeSerializer)
    {
        GuidGenerator = guidGenerator;
        LocalizableStringSerializer = localizableStringSerializer;
        StringValueTypeSerializer = stringValueTypeSerializer;
    }

    public async Task<(FeatureGroupDefinitionRecord[], FeatureDefinitionRecord[])> SerializeAsync(
        IEnumerable<AbpFeatureGroupDefinition> featureGroups)
    {
        var featureGroupRecords = new List<FeatureGroupDefinitionRecord>();
        var featureRecords = new List<FeatureDefinitionRecord>();

        foreach (var featureGroup in featureGroups)
        {
            featureGroupRecords.Add(await SerializeAsync(featureGroup));

            foreach (var feature in featureGroup.GetFeaturesWithChildren())
            {
                featureRecords.Add(await SerializeAsync(feature, featureGroup));
            }
        }

        return (featureGroupRecords.ToArray(), featureRecords.ToArray());
    }

    public Task<FeatureGroupDefinitionRecord> SerializeAsync(AbpFeatureGroupDefinition featureGroup)
    {
        using (CultureHelper.Use(CultureInfo.InvariantCulture))
        {
            var featureGroupRecord = new FeatureGroupDefinitionRecord(
                GuidGenerator.Create(),
                featureGroup.Name,
                LocalizableStringSerializer.Serialize(featureGroup.DisplayName)
            );

            foreach (var property in featureGroup.Properties)
            {
                featureGroupRecord.SetProperty(property.Key, property.Value);
            }

            return Task.FromResult(featureGroupRecord);
        }
    }

    public Task<FeatureDefinitionRecord> SerializeAsync(
        AbpFeatureDefinition feature,
        AbpFeatureGroupDefinition? featureGroup)
    {
        using (CultureHelper.Use(CultureInfo.InvariantCulture))
        {
            var featureRecord = new FeatureDefinitionRecord(
                GuidGenerator.Create(),
                featureGroup?.Name,
                feature.Name,
                feature.Parent?.Name,
                LocalizableStringSerializer.Serialize(feature.DisplayName),
                LocalizableStringSerializer.Serialize(feature.Description),
                feature.DefaultValue,
                feature.IsVisibleToClients,
                feature.IsAvailableToHost,
                SerializeProviders(feature.AllowedProviders),
                SerializeStringValueType(feature.ValueType)
            );

            foreach (var property in feature.Properties)
            {
                featureRecord.SetProperty(property.Key, property.Value);
            }

            return Task.FromResult(featureRecord);
        }
    }

    protected virtual string? SerializeProviders(ICollection<string> providers)
    {
        return providers.Any()
            ? providers.JoinAsString(",")
            : null;
    }

    protected virtual string SerializeStringValueType(AbpIStringValueType? stringValueType)
    {
        return StringValueTypeSerializer.Serialize(stringValueType!);
    }
}
