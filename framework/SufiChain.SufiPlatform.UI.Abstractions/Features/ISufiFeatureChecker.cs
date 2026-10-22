namespace SufiChain.SufiPlatform.UI.Features;

/// <summary>
/// Abstraction for checking whether module features are enabled in the current
/// provider scope (host edition, tenant override, ...).
/// Implementations bridge to the product feature system (ABP features, editions, ...).
/// </summary>
public interface ISufiFeatureChecker
{
    /// <summary>
    /// Checks if the given feature is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <returns>True if the feature is enabled, false otherwise.</returns>
    Task<bool> IsEnabledAsync(string featureName);

    /// <summary>
    /// Checks multiple features at once for better performance.
    /// </summary>
    /// <param name="featureNames">The feature names to check.</param>
    /// <returns>Dictionary of feature name to enabled status.</returns>
    Task<Dictionary<string, bool>> IsEnabledAsync(IEnumerable<string> featureNames);
}
