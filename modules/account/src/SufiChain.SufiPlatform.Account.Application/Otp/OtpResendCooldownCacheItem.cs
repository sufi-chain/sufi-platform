using System;

namespace SufiChain.SufiPlatform.Account.Otp;

[Serializable]
public class OtpResendCooldownCacheItem
{
    public int SendCount { get; set; }

    public DateTime NextAllowedAt { get; set; }
}
