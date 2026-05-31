using System;

namespace SufiChain.SufiAbp.Account.Otp;

[Serializable]
public class OtpCacheItem
{
    public string CodeHash { get; set; } = string.Empty;

    public int Attempts { get; set; }

    public Guid? UserId { get; set; }
}
