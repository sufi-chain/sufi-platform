namespace SufiChain.SufiPlatform.Account;

public class OtpSendResultDto
{
    /// <summary>
    /// Seconds before another code can be requested for the same phone number; 0 when no cooldown applies.
    /// The value is returned whether or not a code was actually sent, so it reveals nothing about the account.
    /// </summary>
    public int ResendAfterSeconds { get; set; }
}
