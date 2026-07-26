using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;

namespace SufiChain.SufiPlatform.Editions.Entitlements;

/// <summary>
/// SaaS entitlement source: delegates to the ABP/Sufi feature checker (edition + tenant values).
/// </summary>
public class FeatureCheckerEntitlementSource : IEntitlementSource, ITransientDependency
{
    protected IFeatureChecker FeatureChecker { get; }

    public FeatureCheckerEntitlementSource(IFeatureChecker featureChecker)
    {
        FeatureChecker = featureChecker;
    }

    public virtual Task<string?> GetOrNullAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return FeatureChecker.GetOrNullAsync(featureName);
    }

    public virtual Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        return FeatureChecker.IsEnabledAsync(featureName);
    }
}
