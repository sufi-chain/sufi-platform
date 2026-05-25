using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Messaging.VoiceCall;

/// <summary>
/// Null implementation of voice call sender (does nothing)
/// </summary>
[Dependency(TryRegister = true)]
public class NullVoiceCallSender : IVoiceCallSender, ISingletonDependency
{
    public MessageType MessageType => MessageType.VoiceCall;
    
    public ILogger<NullVoiceCallSender> Logger { get; set; }

    public NullVoiceCallSender()
    {
        Logger = NullLogger<NullVoiceCallSender>.Instance;
    }

    public Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        Logger.LogWarning($"NullVoiceCallSender: Skipping voice call to {phoneNumber}: {message}");
        return Task.CompletedTask;
    }

    public Task SendAudioAsync(
        string phoneNumber,
        string audioFileUrl,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        Logger.LogWarning($"NullVoiceCallSender: Skipping audio call to {phoneNumber} with audio: {audioFileUrl}");
        return Task.CompletedTask;
    }

    public Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        Logger.LogWarning($"NullVoiceCallSender: Skipping voice call queue to {phoneNumber}: {message}");
        return Task.CompletedTask;
    }
}
