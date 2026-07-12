using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Account;

public class VerificationMessage
{
    public VerificationPurpose Purpose { get; set; }

    public VerificationDeliveryChannel? Channel { get; set; }

    public VerificationDeliveryChannel? PreferredChannel { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? Link { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public string? AppName { get; set; }

    public Dictionary<string, object>? ExtraProperties { get; set; }
}
