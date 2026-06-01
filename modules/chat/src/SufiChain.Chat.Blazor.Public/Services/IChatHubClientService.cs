using Microsoft.AspNetCore.SignalR.Client;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Blazor.Public.Services;

/// <summary>
/// Client-side SignalR connection to the chat hub.
/// </summary>
public interface IChatHubClientService : IAsyncDisposable
{
    HubConnectionState ConnectionState { get; }

    event Func<ChatMessageDto, Task>? MessageReceived;

    event Func<ChatSessionDto, Task>? SessionUpdated;

    event Func<ChatUsageCheckResultDto, Task>? UsageLimitExceeded;

    Task EnsureConnectedAsync(CancellationToken cancellationToken = default);

    Task JoinSessionAsync(Guid sessionId, string? anonymousVisitorId = null, CancellationToken cancellationToken = default);

    Task LeaveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
