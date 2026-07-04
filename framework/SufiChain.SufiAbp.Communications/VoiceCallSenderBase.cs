using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiAbp.Communications.BackgroundJobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Communications.VoiceCall;

/// <summary>
/// Base class for voice call sender implementations
/// </summary>
public abstract class VoiceCallSenderBase : IVoiceCallSender, ITransientDependency
{
    public MessageType MessageType => MessageType.VoiceCall;
    
    public ILogger<VoiceCallSenderBase> Logger { get; set; }
    
    protected IBackgroundJobManager BackgroundJobManager { get; }

    protected VoiceCallSenderBase(IBackgroundJobManager backgroundJobManager)
    {
        BackgroundJobManager = backgroundJobManager;
        Logger = NullLogger<VoiceCallSenderBase>.Instance;
    }

    public virtual async Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        if (additionalArgs?.QueueMessage == true)
        {
            await QueueAsync(phoneNumber, message, from, voiceOptions, additionalArgs);
            return;
        }

        await SendVoiceCallAsync(phoneNumber, message, from, voiceOptions);
    }

    public virtual async Task SendAudioAsync(
        string phoneNumber,
        string audioFileUrl,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        await SendVoiceCallWithAudioAsync(phoneNumber, audioFileUrl, from);
    }

    public virtual async Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        VoiceCallOptions? voiceOptions = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        await BackgroundJobManager.EnqueueAsync(
            new BackgroundVoiceCallSendingJobArgs
            {
                PhoneNumber = phoneNumber,
                Message = message,
                From = from,
                VoiceOptions = voiceOptions,
                Priority = additionalArgs?.Priority ?? MessagePriority.Normal
            }
        );
    }

    /// <summary>
    /// Implement this method to send voice call with text-to-speech using your provider
    /// </summary>
    protected abstract Task SendVoiceCallAsync(
        string phoneNumber, 
        string message, 
        string? from, 
        VoiceCallOptions? voiceOptions
    );

    /// <summary>
    /// Implement this method to send voice call with audio file using your provider
    /// </summary>
    protected abstract Task SendVoiceCallWithAudioAsync(
        string phoneNumber, 
        string audioFileUrl, 
        string? from
    );
}
