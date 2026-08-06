namespace SufiChain.SufiPlatform.Account;

public static class VerificationDeliveryChannelExtensions
{
    public static bool IsPhoneChannel(this VerificationDeliveryChannel channel) =>
        channel is VerificationDeliveryChannel.Sms or VerificationDeliveryChannel.Voice;
}
