using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Features;

public interface IDynamicFeatureDefinitionStore
{
    Task<FeatureGroupDefinition> GetGroupOrNullAsync(string name);

    Task<IReadOnlyList<FeatureGroupDefinition>> GetGroupsAsync();

    Task<FeatureDefinition> GetOrNullAsync(string name);

    Task<IReadOnlyList<FeatureDefinition>> GetFeaturesAsync();
}
