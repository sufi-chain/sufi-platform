namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Manages SufiAbp feature definitions.
/// </summary>
public interface IFeatureDefinitionManager
{
    /// <summary>
    /// Gets all feature groups.
    /// </summary>
    Task<IReadOnlyList<FeatureGroupDefinition>> GetGroupsAsync();

    /// <summary>
    /// Gets all features.
    /// </summary>
    Task<IReadOnlyList<FeatureDefinition>> GetAllAsync();

    /// <summary>
    /// Gets a feature by name.
    /// </summary>
    Task<FeatureDefinition> GetAsync(string name);
}
