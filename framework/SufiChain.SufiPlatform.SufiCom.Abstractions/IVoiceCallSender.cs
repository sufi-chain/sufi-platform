using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiCom.VoiceCall;

/// <summary>
/// Interface for sending voice call messages
/// </summary>
public interface IVoiceCallSender : IMessageSender
{
    /// <summary>
    /// Initiates a voice call with a text-to-speech message
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (E.164 format recommended)</param>
    /// <param name="message">Message to be converted to speech</param>
    /// <param name="from">Caller ID or phone number (optional, provider-specific)</param>
    /// <param name="voiceOptions">Voice and language options</param>
    /// <param name="additionalArgs">Additional sending arguments</param>
    Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );

    /// <summary>
    /// Initiates a voice call with an audio file
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="audioFileUrl">URL to the audio file to play</param>
    /// <param name="from">Caller ID or phone number</param>
    /// <param name="additionalArgs">Additional sending arguments</param>
    Task SendAudioAsync(
        string phoneNumber,
        string audioFileUrl,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );

    /// <summary>
    /// Queues a voice call for background processing
    /// </summary>
    Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );
}
