using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class SettingTests
{
    [Fact]
    public void Constructor_Should_Store_Name_And_Value()
    {
        var setting = new Setting(Guid.NewGuid(), "Sufi.Theme", "dark", "T", null);
        setting.Name.ShouldBe("Sufi.Theme");
        setting.Value.ShouldBe("dark");
    }

    [Fact]
    public void Constructor_Should_Reject_A_Null_Value()
    {
        Should.Throw<ArgumentNullException>(() => new Setting(Guid.NewGuid(), "Sufi.Theme", null!));
    }
}
