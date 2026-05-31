namespace SufiChain.SufiAbp.Account;

public class VerifyOtpInput
{
    public string Identifier { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public VerificationDeliveryChannel? Channel { get; set; }
}
