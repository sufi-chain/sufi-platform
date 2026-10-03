using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account.Otp;

/// <summary>
/// Per-phone-number resend cooldown shared by every OTP purpose and phone channel (SMS and voice).
/// </summary>
public interface IOtpResendCooldownStore
{
    /// <summary>
    /// Records a send request for <paramref name="phone"/> when its cooldown has elapsed.
    /// Rejected requests do not extend the cooldown.
    /// </summary>
    Task<OtpResendCooldownResult> TryAcquireAsync(
        string phone,
        OtpResendCooldownOptions options,
        CancellationToken cancellationToken = default);
}
