using Microsoft.AspNetCore.SignalR.Client;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Blazor.Public.Services;

public class ChatHubClientService : IChatHubClientService, IScopedDependency
{
    protected IChatHubUrlResolver HubUrlResolver { get; }

    protected IChatHubConnectionAccessTokenProvider AccessTokenProvider { get; }

    private HubConnection? _connection;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public HubConnectionState ConnectionState =>
        _connection?.State ?? HubConnectionState.Disconnected;

    public event Func<ChatMessageDto, Task>? MessageReceived;

    public event Func<ChatSessionDto, Task>? SessionUpdated;

    public event Func<ChatUsageCheckResultDto, Task>? UsageLimitExceeded;

    public ChatHubClientService(
        IChatHubUrlResolver hubUrlResolver,
        IChatHubConnectionAccessTokenProvider accessTokenProvider)
    {
        HubUrlResolver = hubUrlResolver;
        AccessTokenProvider = accessTokenProvider;
    }

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[CHAT DEBUG HUB] EnsureConnectedAsync called");
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting or HubConnectionState.Reconnecting })
            {
                Console.WriteLine($"[CHAT DEBUG HUB] Already connected or connecting. State={_connection.State}");
                return;
            }

            Console.WriteLine("[CHAT DEBUG HUB] Creating new hub connection...");
            _connection?.DisposeAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl(HubUrlResolver.GetHubUrl(), options =>
                {
                    options.AccessTokenProvider = async () =>
                        await AccessTokenProvider.GetAccessTokenAsync() ?? string.Empty;
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<ChatMessageDto>(ChatRealtimeClientEvents.MessageReceived, async message =>
            {
                Console.WriteLine($"[CHAT DEBUG HUB] MessageReceived event fired. MessageId={message.Id}, SessionId={message.SessionId}, SenderKind={message.SenderKind}");
                if (MessageReceived != null)
                {
                    await MessageReceived.Invoke(message);
                }
                else
                {
                    Console.WriteLine("[CHAT DEBUG HUB] WARNING: MessageReceived event has no subscribers!");
                }
            });

            _connection.On<ChatSessionDto>(ChatRealtimeClientEvents.SessionUpdated, async session =>
            {
                if (SessionUpdated != null)
                {
                    await SessionUpdated.Invoke(session);
                }
            });

            _connection.On<ChatUsageCheckResultDto>(ChatRealtimeClientEvents.UsageLimitExceeded, async result =>
            {
                if (UsageLimitExceeded != null)
                {
                    await UsageLimitExceeded.Invoke(result);
                }
            });

            await _connection.StartAsync(cancellationToken);
            Console.WriteLine($"[CHAT DEBUG HUB] Hub connection started. State={_connection.State}");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task JoinSessionAsync(
        Guid sessionId,
        string? anonymousVisitorId = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[CHAT DEBUG HUB] JoinSessionAsync called. SessionId={sessionId}");
        await EnsureConnectedAsync(cancellationToken);

        if (_connection == null)
        {
            Console.WriteLine("[CHAT DEBUG HUB] WARNING: Connection is null, cannot join session");
            return;
        }

        await _connection.InvokeAsync(
            "JoinSessionGroupAsync",
            sessionId,
            anonymousVisitorId,
            cancellationToken);
    }

    public async Task LeaveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            return;
        }

        await _connection.InvokeAsync(
            "LeaveSessionGroupAsync",
            sessionId,
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
            _connectionLock.Dispose();
        }
    }
}
