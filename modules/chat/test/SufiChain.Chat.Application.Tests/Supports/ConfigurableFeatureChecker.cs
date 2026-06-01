using SufiChain.SufiAbp.Features;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Supports;

public class ConfigurableFeatureChecker : IFeatureChecker, ISingletonDependency
{
    private readonly HashSet<string> _disabledFeatures = new(StringComparer.OrdinalIgnoreCase);

    public void Disable(params string[] featureNames)
    {
        foreach (var featureName in featureNames)
        {
            _disabledFeatures.Add(featureName);
        }
    }

    public void Enable(params string[] featureNames)
    {
        foreach (var featureName in featureNames)
        {
            _disabledFeatures.Remove(featureName);
        }
    }

    public Task<string?> GetOrNullAsync(string name)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<bool> IsEnabledAsync(string name)
    {
        return Task.FromResult(!_disabledFeatures.Contains(name));
    }
}
