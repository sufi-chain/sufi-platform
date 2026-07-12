namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Checks runtime feature values (Sufi-branded surface).
/// </summary>
public interface IFeatureChecker
{
    Task<string> GetOrNullAsync(string name);

    Task<bool> IsEnabledAsync(string name);
}
