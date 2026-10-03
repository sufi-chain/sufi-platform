namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpResendCooldownResult
{
    public bool IsAllowed { get; init; }

    /// <summary>
    /// When allowed: seconds until the next request is accepted. When rejected: seconds left on the current cooldown.
    /// </summary>
    public int WaitSeconds { get; init; }

    public static OtpResendCooldownResult Allowed(int nextWaitSeconds) => new() { IsAllowed = true, WaitSeconds = nextWaitSeconds };

    public static OtpResendCooldownResult Rejected(int remainingSeconds) => new() { IsAllowed = false, WaitSeconds = remainingSeconds };
}
