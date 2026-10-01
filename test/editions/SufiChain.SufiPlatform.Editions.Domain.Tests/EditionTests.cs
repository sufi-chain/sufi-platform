using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Editions;

public class EditionTests
{
    [Fact]
    public void SetCode_Should_Trim_And_Uppercase()
    {
        var edition = new Edition(Guid.NewGuid(), "Pro", "Pro", " pro ");
        edition.Code.ShouldBe("PRO");
        edition.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void SetName_Should_Reject_Blank_Text()
    {
        var edition = new Edition(Guid.NewGuid(), "Pro", "Pro", "PRO");
        Should.Throw<ArgumentException>(() => edition.SetName(" "));
    }
}
