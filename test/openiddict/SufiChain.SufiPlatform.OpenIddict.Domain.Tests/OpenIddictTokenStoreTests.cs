using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.OpenIddict;

public class OpenIddictIdentifierConverterTests
{
    private readonly OpenIddictIdentifierConverter _converter = new();

    [Fact]
    public void Identifier_Should_Round_Trip()
    {
        var id = Guid.NewGuid();
        _converter.FromString(_converter.ToString(id)).ShouldBe(id);
    }

    [Fact]
    public void FromString_Should_Reject_An_Invalid_Identifier()
    {
        _converter.FromString("").ShouldBe(Guid.Empty);
        Should.Throw<FormatException>(() => _converter.FromString("not-a-guid"));
    }
}
