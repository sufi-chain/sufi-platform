using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.AuditLogging;

public class AuditLogExcelFileTests
{
    [Fact]
    public void Constructor_Should_Store_A_File_Name()
    {
        var tenantId = Guid.NewGuid();
        var file = new AuditLogExcelFile(Guid.NewGuid(), "audit.xlsx", tenantId);
        file.FileName.ShouldBe("audit.xlsx");
        file.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public void Constructor_Should_Reject_A_Blank_File_Name()
    {
        Should.Throw<ArgumentException>(() => new AuditLogExcelFile(Guid.NewGuid(), " "));
    }
}
