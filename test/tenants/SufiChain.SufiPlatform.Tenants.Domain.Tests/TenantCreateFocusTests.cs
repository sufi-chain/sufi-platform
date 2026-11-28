using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantCreateFocusTests
{
    [Fact]
    public void TenantCreate_EmptySubmit_FocusesName()
    {
        TenantCreateFocus.FirstInvalid(new TenantCreateDto()).ShouldBe("Name");
    }

    [Fact]
    public void TenantCreate_OnlyEmailInvalid_FocusesEmail()
    {
        TenantCreateFocus.FirstInvalid(new TenantCreateDto
        {
            Name = "Demo",
            AdminEmailAddress = "not-an-email",
            AdminPassword = "secret"
        }).ShouldBe("AdminEmailAddress");
    }
}
