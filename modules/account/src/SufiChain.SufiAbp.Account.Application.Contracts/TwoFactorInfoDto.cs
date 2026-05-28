using System.Collections.Generic;

namespace SufiChain.SufiAbp.Account;

public class TwoFactorInfoDto
{
    public bool IsEnabled { get; set; }

    public bool HasAuthenticator { get; set; }

    public bool AllowAuthenticatorApp { get; set; }

    public bool AllowCodeDelivery { get; set; }

    public IReadOnlyList<VerificationDeliveryChannel> AvailableCodeChannels { get; set; } =
        new List<VerificationDeliveryChannel>();
}
