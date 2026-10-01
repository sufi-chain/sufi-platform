using Shouldly;
using SufiChain.SufiPlatform.Localization.Entities;
using Xunit;

namespace SufiChain.SufiPlatform.Localization;

public class LocalizationTextTests
{
    [Fact]
    public void Constructor_Should_Store_The_Translation()
    {
        var text = new LocalizationText(Guid.NewGuid(), null, "FileManager", "en", "Menu:Files", "Files");
        text.Key.ShouldBe("Menu:Files");
        text.Value.ShouldBe("Files");
    }

    [Fact]
    public void SetKey_Should_Reject_Whitespace()
    {
        var text = new LocalizationText(Guid.NewGuid(), null, "FileManager", "en", "Menu:Files", "Files");
        Should.Throw<ArgumentException>(() => text.SetKey(" "));
    }
}
