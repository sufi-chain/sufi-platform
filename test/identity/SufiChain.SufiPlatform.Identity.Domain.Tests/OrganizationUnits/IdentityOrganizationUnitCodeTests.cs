using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityOrganizationUnitCodeTests
{
    [Fact]
    public void GetRelativeCode_Should_Return_The_Suffix_Under_A_Parent()
    {
        OrganizationUnit.GetRelativeCode("00019.00055.00001", "00019").ShouldBe("00055.00001");
    }

    [Fact]
    public void GetRelativeCode_Should_Reject_An_Empty_Code()
    {
        Should.Throw<ArgumentNullException>(() => OrganizationUnit.GetRelativeCode("", "00019"));
    }
}
