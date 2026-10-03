using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpResendCooldownStore : IOtpResendCooldownStore, ITransientDependency
{
    private const string CooldownKeyPrefix = "Account:OtpCooldown:";
    private const string LockKeyPrefix = "Account:OtpCooldownLock:";

    protected IDistributedCache<OtpResendCooldownCacheItem> CooldownCache { get; }

    protected IAbpDistributedLock DistributedLock { get; }

    protected IClock Clock { get; }

    public OtpResendCooldownStore(
        IDistributedCache<OtpResendCooldownCacheItem> cooldownCache,
        IAbpDistributedLock distributedLock,
        IClock clock)
    {
        CooldownCache = cooldownCache;
        DistributedLock = distributedLock;
        Clock = clock;
    }

    public virtual async Task<OtpResendCooldownResult> TryAcquireAsync(
        string phone,
        OtpResendCooldownOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!options.IsEnabled)
        {
            return OtpResendCooldownResult.Allowed(0);
        }

        var normalizedPhone = NormalizePhone(phone);

        await using var handle = await DistributedLock.TryAcquireAsync(
            LockKeyPrefix + normalizedPhone,
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (handle == null)
        {
            // Another request for this number is being recorded right now; treat it as the one that wins.
            return OtpResendCooldownResult.Rejected(options.GetCooldownSeconds(1));
        }

        var key = CooldownKeyPrefix + normalizedPhone;
        var now = Clock.Now;
        var item = await CooldownCache.GetAsync(key, token: cancellationToken);

        if (item != null && item.NextAllowedAt > now)
        {
            return OtpResendCooldownResult.Rejected(ToWholeSeconds(item.NextAllowedAt - now));
        }

        var sendCount = (item?.SendCount ?? 0) + 1;
        var waitSeconds = options.GetCooldownSeconds(sendCount);
        var waitWindow = TimeSpan.FromSeconds(waitSeconds);
        var resetWindow = options.GetCounterResetWindow();

        await CooldownCache.SetAsync(
            key,
            new OtpResendCooldownCacheItem
            {
                SendCount = sendCount,
                NextAllowedAt = now.Add(waitWindow)
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = waitWindow > resetWindow ? waitWindow : resetWindow
            },
            token: cancellationToken);

        return OtpResendCooldownResult.Allowed(waitSeconds);
    }

    public static string NormalizePhone(string phone)
    {
        var national = IranianMobileNumber.ToNationalNumber(phone);
        if (national != null)
        {
            return "98" + national;
        }

        var digits = new string(phone.Where(char.IsDigit).Select(c => (char)('0' + (int)char.GetNumericValue(c))).ToArray());
        return digits.Length > 0 ? digits.TrimStart('0') : phone.Trim().ToLowerInvariant();
    }

    private static int ToWholeSeconds(TimeSpan remaining)
    {
        return Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
    }
}
