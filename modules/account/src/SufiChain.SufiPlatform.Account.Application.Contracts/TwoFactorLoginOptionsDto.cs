using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Anonymous-safe 2FA options for the login page (settings only, no user context).
/// </summary>
public class TwoFactorLoginOptionsDto
{
    public bool AllowAuthenticatorApp { get; set; }

    public bool AllowCodeDelivery { get; set; }

    public IReadOnlyList<VerificationDeliveryChannel> AvailableCodeChannels { get; set; } =
        new List<VerificationDeliveryChannel>();
}
