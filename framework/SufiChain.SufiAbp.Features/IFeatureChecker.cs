namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Checks runtime feature values (SufiAbp-branded surface).
/// </summary>
public interface IFeatureChecker
{
    Task<string> GetOrNullAsync(string name);

    Task<bool> IsEnabledAsync(string name);
}
