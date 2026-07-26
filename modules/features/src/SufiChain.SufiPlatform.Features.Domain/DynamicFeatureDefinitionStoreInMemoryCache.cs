using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureDefinitionContext = Volo.Abp.Features.FeatureDefinitionContext;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;
using AbpICanCreateChildFeature = Volo.Abp.Features.ICanCreateChildFeature;

namespace SufiChain.SufiPlatform.Features;

public class DynamicFeatureDefinitionStoreInMemoryCache :
    IDynamicFeatureDefinitionStoreInMemoryCache,
    ISingletonDependency
{
    public string CacheStamp { get; set; } = default!;

    protected IDictionary<string, AbpFeatureGroupDefinition> FeatureGroupDefinitions { get; }
    protected IDictionary<string, AbpFeatureDefinition> FeatureDefinitions { get; }
    protected StringValueTypeSerializer StateCheckerSerializer { get; }
    protected ILocalizableStringSerializer LocalizableStringSerializer { get; }

    public SemaphoreSlim SyncSemaphore { get; } = new(1, 1);

    public DateTime? LastCheckTime { get; set; }

    public DynamicFeatureDefinitionStoreInMemoryCache(
        StringValueTypeSerializer stateCheckerSerializer,
        ILocalizableStringSerializer localizableStringSerializer)
    {
        StateCheckerSerializer = stateCheckerSerializer;
        LocalizableStringSerializer = localizableStringSerializer;

        FeatureGroupDefinitions = new Dictionary<string, AbpFeatureGroupDefinition>();
        FeatureDefinitions = new Dictionary<string, AbpFeatureDefinition>();
    }

    public Task FillAsync(
        List<FeatureGroupDefinitionRecord> featureGroupRecords,
        List<FeatureDefinitionRecord> featureRecords)
    {
        FeatureGroupDefinitions.Clear();
        FeatureDefinitions.Clear();

        var context = new AbpFeatureDefinitionContext();

        foreach (var featureGroupRecord in featureGroupRecords)
        {
            var featureGroup = context.AddGroup(
                featureGroupRecord.Name,
                featureGroupRecord.DisplayName != null
                    ? LocalizableStringSerializer.Deserialize(featureGroupRecord.DisplayName)
                    : null
            );

            FeatureGroupDefinitions[featureGroup.Name] = featureGroup;

            foreach (var property in featureGroupRecord.ExtraProperties)
            {
                featureGroup[property.Key] = property.Value;
            }

            var featureRecordsInThisGroup = featureRecords
                .Where(p => p.GroupName == featureGroup.Name);

            foreach (var featureRecord in featureRecordsInThisGroup.Where(x => x.ParentName == null))
            {
                AddFeatureRecursively(featureGroup, featureRecord, featureRecords);
            }
        }

        return Task.CompletedTask;
    }

    public AbpFeatureDefinition? GetFeatureOrNull(string name)
    {
        return FeatureDefinitions.GetOrDefault(name);
    }

    public IReadOnlyList<AbpFeatureDefinition> GetFeatures()
    {
        return FeatureDefinitions.Values.ToList();
    }

    public IReadOnlyList<AbpFeatureGroupDefinition> GetGroups()
    {
        return FeatureGroupDefinitions.Values.ToList();
    }

    private void AddFeatureRecursively(
        AbpICanCreateChildFeature featureContainer,
        FeatureDefinitionRecord featureRecord,
        List<FeatureDefinitionRecord> allFeatureRecords)
    {
        var feature = featureContainer.CreateChildFeature(
            featureRecord.Name,
            featureRecord.DefaultValue,
            featureRecord.DisplayName != null
                ? LocalizableStringSerializer.Deserialize(featureRecord.DisplayName)
                : null,
            featureRecord.Description != null
                ? LocalizableStringSerializer.Deserialize(featureRecord.Description)
                : null,
            StateCheckerSerializer.Deserialize(featureRecord.ValueType),
            featureRecord.IsVisibleToClients,
            featureRecord.IsAvailableToHost
        );

        FeatureDefinitions[feature.Name] = feature;

        if (!featureRecord.AllowedProviders.IsNullOrWhiteSpace())
        {
            feature.AllowedProviders.AddRange(featureRecord.AllowedProviders.Split(','));
        }

        foreach (var property in featureRecord.ExtraProperties)
        {
            feature[property.Key] = property.Value;
        }

        foreach (var subFeature in allFeatureRecords.Where(p => p.ParentName == featureRecord.Name))
        {
            AddFeatureRecursively(feature, subFeature, allFeatureRecords);
        }
    }
}
