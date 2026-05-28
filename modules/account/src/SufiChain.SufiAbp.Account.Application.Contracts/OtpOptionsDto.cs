using System.Collections.Generic;

namespace SufiChain.SufiAbp.Account;

public class OtpOptionsDto
{
    public bool IsEnabled { get; set; }

    public bool AllowLogin { get; set; }

    public bool AllowRegistration { get; set; }

    public VerificationDeliveryChannel DefaultChannel { get; set; }

    public IReadOnlyList<VerificationDeliveryChannel> AvailableChannels { get; set; } =
        new List<VerificationDeliveryChannel>();
}
