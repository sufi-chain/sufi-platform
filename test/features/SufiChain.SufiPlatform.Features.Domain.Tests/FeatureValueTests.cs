using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Features;

public class FeatureValueTests
{
    [Fact]
    public void Constructor_Should_Store_A_Provider_Value()
    {
        var value = new FeatureValue(Guid.NewGuid(), "Sufi.Feature", "true", "T", "tenant");
        value.Name.ShouldBe("Sufi.Feature");
        value.Value.ShouldBe("true");
        value.ProviderKey.ShouldBe("tenant");
    }

    [Fact]
    public void Constructor_Should_Reject_A_Blank_Value()
    {
        Should.Throw<ArgumentException>(() => new FeatureValue(Guid.NewGuid(), "Sufi.Feature", " ", "T", null));
    }
}
