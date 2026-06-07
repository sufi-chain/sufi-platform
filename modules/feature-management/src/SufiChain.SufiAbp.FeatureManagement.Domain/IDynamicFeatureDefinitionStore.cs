using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.FeatureManagement;

public interface IDynamicFeatureDefinitionStore
{
    Task<FeatureGroupDefinition> GetGroupOrNullAsync(string name);

    Task<IReadOnlyList<FeatureGroupDefinition>> GetGroupsAsync();

    Task<FeatureDefinition> GetOrNullAsync(string name);

    Task<IReadOnlyList<FeatureDefinition>> GetFeaturesAsync();
}
