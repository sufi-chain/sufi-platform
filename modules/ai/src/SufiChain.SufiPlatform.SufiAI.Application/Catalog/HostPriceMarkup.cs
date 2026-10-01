using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Features;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public class HostPriceMarkup : ITransientDependency
{
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentTenant _currentTenant;

    public HostPriceMarkup(IFeatureChecker featureChecker, ICurrentTenant currentTenant)
    {
        _featureChecker = featureChecker;
        _currentTenant = currentTenant;
    }

    public virtual async Task<decimal> GetPercentAsync()
    {
        try
        {
            using (_currentTenant.Change(null))
            {
                var raw = await _featureChecker.GetOrNullAsync(SufiAIFeatures.PriceMarkupPercent);
                return decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var percent) &&
                       percent > 0
                    ? percent
                    : 0m;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return 0m;
        }
    }
}
