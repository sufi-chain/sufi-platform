using System;

namespace SufiChain.SufiPlatform.Account.Otp;

[Serializable]
public class OtpRateLimitCacheItem
{
    public int Count { get; set; }
}
