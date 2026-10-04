using System;

namespace SufiChain.SufiPlatform.Account;

[Serializable]
public class PhoneConfirmationSessionCacheItem
{
    public Guid UserId { get; set; }
}
