using Volo.Abp.DependencyInjection;
using AbpFeatureChecker = Volo.Abp.Features.IFeatureChecker;

namespace SufiChain.SufiAbp.Features;

/// <summary>
/// SufiAbp feature checker wrapper over the ABP feature checker.
/// </summary>
[ExposeServices(typeof(IFeatureChecker))]
public class SufiAbpFeatureChecker : IFeatureChecker, ITransientDependency
{
    private readonly AbpFeatureChecker _featureChecker;

    public SufiAbpFeatureChecker(AbpFeatureChecker featureChecker)
    {
        _featureChecker = featureChecker;
    }

    public Task<string> GetOrNullAsync(string name)
    {
        return _featureChecker.GetOrNullAsync(name);
    }

    public Task<bool> IsEnabledAsync(string name)
    {
        return _featureChecker.IsEnabledAsync(name);
    }
}
