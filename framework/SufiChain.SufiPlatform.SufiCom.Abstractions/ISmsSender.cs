using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiCom.Sms;

/// <summary>
/// Interface for sending SMS messages
/// </summary>
public interface ISmsSender : IMessageSender
{
    /// <summary>
    /// Sends an SMS message
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (E.164 format recommended)</param>
    /// <param name="message">Message text content</param>
    /// <param name="from">Sender ID or phone number (optional, provider-specific)</param>
    /// <param name="additionalArgs">Additional sending arguments</param>
    Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );

    /// <summary>
    /// Queues an SMS message for background sending
    /// </summary>
    Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );
}
