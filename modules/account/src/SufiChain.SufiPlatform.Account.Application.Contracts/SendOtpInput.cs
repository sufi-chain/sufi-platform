namespace SufiChain.SufiPlatform.Account;

public class SendOtpInput : CaptchaInputDto
{
    public string Identifier { get; set; } = string.Empty;

    public VerificationDeliveryChannel? Channel { get; set; }

    public string? AppName { get; set; }
}
