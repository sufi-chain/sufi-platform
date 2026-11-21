using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityPasswordChangeTests
{
    [Fact]
    public void MarkChanged_clears_forced_password_change_and_sets_last_change_time()
    {
        var user = new IdentityUser(Guid.NewGuid(), "demo-admin", "demo@example.com");
        user.SetShouldChangePasswordOnNextLogin(true);

        IdentityPasswordChange.MarkChanged(user);

        user.ShouldChangePasswordOnNextLogin.ShouldBeFalse();
        user.LastPasswordChangeTime.ShouldNotBeNull();
        user.LastPasswordChangeTime!.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(-1));
    }
}
