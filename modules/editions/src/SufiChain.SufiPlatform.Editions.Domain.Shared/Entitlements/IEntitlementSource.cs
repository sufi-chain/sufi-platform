using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Editions.Entitlements;

/// <summary>
/// Resolves entitlement feature values for the current runtime.
/// SaaS uses edition/tenant feature stores; On-prem later uses a license API snapshot.
/// </summary>
public interface IEntitlementSource
{
    Task<string?> GetOrNullAsync(string featureName, CancellationToken cancellationToken = default);

    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
}
