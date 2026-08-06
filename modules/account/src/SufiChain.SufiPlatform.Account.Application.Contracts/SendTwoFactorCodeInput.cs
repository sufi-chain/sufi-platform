namespace SufiChain.SufiPlatform.Account;

public class SendTwoFactorCodeInput
{
    /// <summary>
    /// Pending interactive login token from the login page (optional for signed-in users).
    /// </summary>
    public string? PendingToken { get; set; }

    public VerificationDeliveryChannel? PreferredChannel { get; set; }

    public string? AppName { get; set; }
}
