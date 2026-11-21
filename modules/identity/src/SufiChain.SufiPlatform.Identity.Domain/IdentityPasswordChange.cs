using Volo.Abp;

namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Shared side effects after a successful password set/change/reset (HST-05).
/// </summary>
public static class IdentityPasswordChange
{
    public static void MarkChanged(IdentityUser user)
    {
        Check.NotNull(user, nameof(user));
        user.SetLastPasswordChangeTime(DateTimeOffset.UtcNow);
        user.SetShouldChangePasswordOnNextLogin(false);
    }
}
