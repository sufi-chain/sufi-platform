using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Editions.Entitlements;

/// <summary>
/// Marker contract for a future On-prem license-backed entitlement source.
/// Do not register on the SaaS host.
/// </summary>
public interface ILicenseApiEntitlementSource : IEntitlementSource
{
    Task<ConfirmLicenseResponse?> ConfirmAsync(CancellationToken cancellationToken = default);
}
