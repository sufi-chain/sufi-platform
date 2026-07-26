using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using AbpFeatureDefinition = Volo.Abp.Features.FeatureDefinition;
using AbpFeatureGroupDefinition = Volo.Abp.Features.FeatureGroupDefinition;

namespace SufiChain.SufiPlatform.Features;

public interface IFeatureDefinitionSerializer
{
    Task<(FeatureGroupDefinitionRecord[], FeatureDefinitionRecord[])> SerializeAsync(
        IEnumerable<AbpFeatureGroupDefinition> featureGroups);

    Task<FeatureGroupDefinitionRecord> SerializeAsync(AbpFeatureGroupDefinition featureGroup);

    Task<FeatureDefinitionRecord> SerializeAsync(
        AbpFeatureDefinition feature,
        [CanBeNull] AbpFeatureGroupDefinition featureGroup);
}
