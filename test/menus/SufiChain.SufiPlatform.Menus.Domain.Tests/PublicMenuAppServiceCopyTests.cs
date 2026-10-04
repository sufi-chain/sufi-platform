using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class PublicMenuAppServiceCopyTests
{
    [Fact]
    public void CopyItem_Should_Keep_CultureUrls()
    {
        var source = new MenuItemDto
        {
            Id = Guid.NewGuid(),
            Name = "solve",
            DisplayName = "Solve",
            Url = "/#solve",
            CultureUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = "/#opensource",
                ["ar"] = "/#top"
            }
        };

        var copy = new MenuItemDto();
        PublicMenuAppService.CopyItem(source, copy);

        copy.Url.ShouldBe("/#solve");
        copy.CultureUrls.ShouldNotBeNull();
        copy.CultureUrls.ShouldNotBeSameAs(source.CultureUrls);
        copy.CultureUrls["en"].ShouldBe("/#opensource");
        copy.CultureUrls["ar"].ShouldBe("/#top");

        var empty = new MenuItemDto { Name = "home", DisplayName = "Home", Url = "/" };
        var emptyCopy = new MenuItemDto();
        PublicMenuAppService.CopyItem(empty, emptyCopy);
        emptyCopy.CultureUrls.ShouldBeNull();
    }
}
