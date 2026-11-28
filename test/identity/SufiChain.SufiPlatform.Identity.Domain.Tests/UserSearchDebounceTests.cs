using SufiChain.SufiPlatform.Identity;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity.Domain.Tests;

public class UserSearchDebounceTests
{
    [Fact]
    public void Typing_Waits()
    {
        UserSearchDebounce.ApplyImmediately(null, "ali").ShouldBeFalse();
        UserSearchDebounce.DelayMs.ShouldBe(300);
    }

    [Fact]
    public void Enter_AndClear_ApplyAtOnce()
    {
        UserSearchDebounce.ApplyImmediately("Enter", "ali").ShouldBeTrue();
        UserSearchDebounce.ApplyImmediately(null, "").ShouldBeTrue();
        UserSearchDebounce.ApplyImmediately(null, null).ShouldBeTrue();
    }
}
