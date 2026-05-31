using System;

namespace SufiChain.SufiAbp.Account.Otp;

[Serializable]
public class OtpRateLimitCacheItem
{
    public int Count { get; set; }
}
