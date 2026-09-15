using Shouldly;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.RAG;

public class VectorStoreTenantScopeTests
{
    [Fact]
    public void GetTenantKey_Should_Return_Host_When_Tenant_Is_Null()
    {
        VectorStoreTenantScope.GetTenantKey(null).ShouldBe("host");
    }

    [Fact]
    public void GetTenantKey_Should_Return_Lowercase_N_Format()
    {
        var tenantId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        VectorStoreTenantScope.GetTenantKey(tenantId).ShouldBe("a1b2c3d4e5f67890abcdef1234567890");
    }

    [Fact]
    public void BuildName_Should_Append_Tenant_Key_To_Base()
    {
        VectorStoreTenantScope.BuildName("sufiplatform_ai_documents", "host")
            .ShouldBe("sufiplatform_ai_documents_host");

        VectorStoreTenantScope.BuildName("ai_documents", "a1b2c3d4e5f67890abcdef1234567890")
            .ShouldBe("ai_documents_a1b2c3d4e5f67890abcdef1234567890");
    }

    [Fact]
    public void BuildName_Should_Sanitize_Unsafe_Characters()
    {
        VectorStoreTenantScope.BuildName("Sufi-Platform.AI", "Host!")
            .ShouldBe("sufi_platform_ai_host");
    }

    [Fact]
    public void BuildName_Should_Reject_Empty_Base()
    {
        Should.Throw<ArgumentException>(() => VectorStoreTenantScope.BuildName("   ", "host"));
    }
}
