using Volo.Abp.DependencyInjection;
using AbpFeatureDefinitionManager = Volo.Abp.Features.IFeatureDefinitionManager;

namespace SufiChain.SufiAbp.Features;

/// <summary>
/// SufiAbp feature definition manager wrapper.
/// </summary>
[ExposeServices(typeof(IFeatureDefinitionManager))]
public class SufiAbpFeatureDefinitionManager : IFeatureDefinitionManager, ITransientDependency
{
    private readonly AbpFeatureDefinitionManager _featureDefinitionManager;

    /// <summary>
    /// Creates a new SufiAbp feature definition manager.
    /// </summary>
    public SufiAbpFeatureDefinitionManager(AbpFeatureDefinitionManager featureDefinitionManager)
    {
        _featureDefinitionManager = featureDefinitionManager;
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<FeatureGroupDefinition>> GetGroupsAsync()
    {
        return (await _featureDefinitionManager.GetGroupsAsync())
            .Select(group => new FeatureGroupDefinition(group))
            .ToList();
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<FeatureDefinition>> GetAllAsync()
    {
        return (await _featureDefinitionManager.GetAllAsync())
            .Select(feature => new FeatureDefinition(feature))
            .ToList();
    }

    /// <inheritdoc />
    public virtual async Task<FeatureDefinition> GetAsync(string name)
    {
        return new FeatureDefinition(await _featureDefinitionManager.GetAsync(name));
    }
}
