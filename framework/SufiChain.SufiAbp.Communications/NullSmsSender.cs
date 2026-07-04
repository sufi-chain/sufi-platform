using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Communications.Sms;

/// <summary>
/// Null implementation of SMS sender (does nothing)
/// </summary>
[Dependency(TryRegister = true)]
public class NullSmsSender : ISmsSender, ISingletonDependency
{
    public MessageType MessageType => MessageType.Sms;
    
    public ILogger<NullSmsSender> Logger { get; set; }

    public NullSmsSender()
    {
        Logger = NullLogger<NullSmsSender>.Instance;
    }

    public Task SendAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        Logger.LogWarning($"NullSmsSender: Skipping SMS send to {phoneNumber}: {message}");
        return Task.CompletedTask;
    }

    public Task QueueAsync(
        string phoneNumber,
        string message,
        string? from = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        Logger.LogWarning($"NullSmsSender: Skipping SMS queue to {phoneNumber}: {message}");
        return Task.CompletedTask;
    }
}
