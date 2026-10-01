using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Permissions;

public class PermissionGrantTests
{
    [Fact]
    public void Constructor_Should_Store_The_Tenant_Grant()
    {
        var tenantId = Guid.NewGuid();
        var grant = new PermissionGrant(Guid.NewGuid(), "Sufi.Pages", "R", "admin", tenantId);
        grant.Name.ShouldBe("Sufi.Pages");
        grant.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public void Constructor_Should_Reject_A_Blank_Name()
    {
        Should.Throw<ArgumentException>(() => new PermissionGrant(Guid.NewGuid(), " ", "R", "admin"));
    }
}
