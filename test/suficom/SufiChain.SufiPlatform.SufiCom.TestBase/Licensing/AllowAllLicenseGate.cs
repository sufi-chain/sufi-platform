using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Licensing;

namespace SufiChain.SufiPlatform.SufiCom.Licensing;

/// <summary>
/// Test double that grants every product so module tests exercise SufiCom rules
/// instead of the fail-closed production gate. Licensing behavior has its own suite.
/// </summary>
public sealed class AllowAllLicenseGate : ILicenseGate
{
    private static readonly LicenseEntitlement Entitlement = new()
    {
        LicenseId = "test",
        Tier = "Test",
        Products = LicenseProducts.All,
        SignatureValid = true,
        IsPresent = true
    };

    public Task<LicenseEntitlement> GetEntitlementAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Entitlement);

    public Task EnsureCanWriteAsync(string productCode, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<bool> CanWriteAsync(string productCode, CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
