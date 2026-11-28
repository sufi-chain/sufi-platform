using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class DedicatedTenantHostWriteGuardTests
{
    [Fact]
    public void HostContextSave_RowOfDedicatedTenant_Throws()
    {
        var error = Should.Throw<Volo.Abp.BusinessException>(() =>
            DedicatedTenantHostWriteGuard.ThrowIfDedicatedRowUsesHostConnection(
                Guid.NewGuid(),
                tenantIsDedicated: true,
                contextConnection: "Host=host-db",
                hostConnection: "Host=host-db"));

        error.Code.ShouldBe(TenantsErrorCodes.DedicatedTenantHostWrite);
    }

    [Fact]
    public void HostContextSave_RowOfSharedTenant_DoesNotThrow()
    {
        DedicatedTenantHostWriteGuard.ThrowIfDedicatedRowUsesHostConnection(
            Guid.NewGuid(),
            tenantIsDedicated: false,
            contextConnection: "Host=host-db",
            hostConnection: "Host=host-db");
    }

    [Fact]
    public void HostContextSave_DedicatedTenantOnItsOwnDatabase_DoesNotThrow()
    {
        DedicatedTenantHostWriteGuard.ThrowIfDedicatedRowUsesHostConnection(
            Guid.NewGuid(),
            tenantIsDedicated: true,
            contextConnection: "Host=tenant-db",
            hostConnection: "Host=host-db");
    }
}
