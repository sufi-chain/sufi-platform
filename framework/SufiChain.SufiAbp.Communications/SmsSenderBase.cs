using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiAbp.Communications.BackgroundJobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Communications.Sms;

/// <summary>
/// Base class for SMS sender implementations
/// </summary>
public abstract class SmsSenderBase : ISmsSender, ITransientDependency
{
    public MessageType MessageType => MessageType.Sms;
    
    public ILogger<SmsSenderBase> Logger { get; set; }
    
    protected IBackgroundJobManager BackgroundJobManager { get; }

    protected SmsSenderBase(IBackgroundJobManager backgroundJobManager)
    {
        BackgroundJobManager = backgroundJobManager;
        Logger = NullLogger<SmsSenderBase>.Instance;
    }

    public virtual async Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        if (additionalArgs?.QueueMessage == true)
        {
            await QueueAsync(phoneNumber, message, from, additionalArgs);
            return;
        }

        await SendSmsAsync(phoneNumber, message, from);
    }

    public virtual async Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        await BackgroundJobManager.EnqueueAsync(
            new BackgroundSmsSendingJobArgs
            {
                PhoneNumber = phoneNumber,
                Message = message,
                From = from,
                Priority = additionalArgs?.Priority ?? MessagePriority.Normal
            }
        );
    }

    /// <summary>
    /// Implement this method to send SMS using your provider
    /// </summary>
    protected abstract Task SendSmsAsync(string phoneNumber, string message, string? from);
}
