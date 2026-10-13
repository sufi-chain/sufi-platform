namespace SufiChain.SufiPlatform.Account.Otp;

public static class OtpMessageKeys
{
    public const string CodeSentEmail = "OtpCodeSentEmail";
    public const string CodeSentPhone = "OtpCodeSentPhone";

    public static string CodeSent(VerificationDeliveryChannel channel) =>
        channel.IsPhoneChannel() ? CodeSentPhone : CodeSentEmail;
}
