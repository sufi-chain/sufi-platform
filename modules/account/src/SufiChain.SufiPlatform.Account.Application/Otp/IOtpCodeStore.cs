using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account.Otp;

public interface IOtpCodeStore
{
    Task StoreAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        OtpCacheItem item,
        int expirationMinutes,
        CancellationToken cancellationToken = default);

    Task<OtpCacheItem?> GetAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        CancellationToken cancellationToken = default);

    Task<bool> TryIncrementRateLimitAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel channel,
        string identifier,
        int maxPerHour,
        CancellationToken cancellationToken = default);

    Task<string> CreateRegistrationTokenAsync(
        string email,
        int expirationMinutes,
        CancellationToken cancellationToken = default);

    Task<string?> ConsumeRegistrationTokenAsync(
        string registrationToken,
        CancellationToken cancellationToken = default);
}
