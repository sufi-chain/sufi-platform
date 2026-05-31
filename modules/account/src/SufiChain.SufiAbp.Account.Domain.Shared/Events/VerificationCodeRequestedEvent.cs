using System;

namespace SufiChain.SufiAbp.Account;

public class VerificationCodeRequestedEvent
{
    public Guid? UserId { get; set; }

    public string Identifier { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public VerificationPurpose Purpose { get; set; }

    public VerificationDeliveryChannel? PreferredChannel { get; set; }

    public string? AppName { get; set; }
}
