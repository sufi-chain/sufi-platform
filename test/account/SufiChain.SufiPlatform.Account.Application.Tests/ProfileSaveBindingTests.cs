using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class ProfileSaveBindingTests
{
    [Fact]
    public void Save_Name_InputNeverShowsPreviousValue()
    {
        var shown = ProfileSaveBinding.Shown(
            new ProfileDto { UserName = "qa", Email = "qa@example.com", Name = "QA S28", ConcurrencyStamp = "1" },
            new ProfileDto { UserName = "qa", Email = "qa@example.com", Name = "Old", ConcurrencyStamp = "0" });

        shown.Name.ShouldBe("QA S28");
    }

    [Fact]
    public void Save_Name_ShowsSuccessAndNewValue()
    {
        var shown = ProfileSaveBinding.Shown(
            new ProfileDto { UserName = "qa", Email = "qa@example.com", Name = "QA S28", ConcurrencyStamp = "1" },
            null);
        shown.Name.ShouldBe("QA S28");
    }
}
