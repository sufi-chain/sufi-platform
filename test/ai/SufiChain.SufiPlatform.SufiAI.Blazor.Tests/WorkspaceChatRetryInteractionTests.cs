using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Components;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Services;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.PageToolbars;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

/// <summary>
/// sufi-chain#551: the retry control on the workspace page resaves the failed reply.
/// It does not send another user message or call the model again.
/// </summary>
public class WorkspaceChatRetryInteractionTests : BunitContext
{
    private readonly RecordingTranscripts _transcripts = new();
    private readonly RecordingMessages _messages = new();
    private readonly RecordingChat _chat = new();
    private readonly Guid _sessionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _otherSessionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public WorkspaceChatRetryInteractionTests()
    {
        Services.AddSingleton<IPageLayout>(new TestPageLayout());
        Services.AddSingleton<IStringLocalizerFactory>(new TestLocalizerFactory());
        Services.AddSingleton<IUserExceptionInformer>(new TestExceptionInformer());
        Services.AddSingleton<IWorkspaceAppService>(Dispatch<IWorkspaceAppService>());
        Services.AddSingleton<IAIAppService>(Dispatch<IAIAppService>());
        Services.AddSingleton<IChatComposerCapabilitiesAppService>(Dispatch<IChatComposerCapabilitiesAppService>());
        Services.AddSingleton<ISessionStorageService>(Dispatch<ISessionStorageService>());
        Services.AddSingleton<IChatSessionAppService>(Dispatch<IChatSessionAppService>((method, args) =>
        {
            if (method.Name == nameof(IChatSessionAppService.GetAsync))
            {
                return Task.FromResult(new ChatSessionDto { Id = (Guid)args![0]! });
            }

            if (method.Name == nameof(IChatSessionAppService.GetMySessionsAsync))
            {
                return Task.FromResult(new PagedResultDto<ChatSessionListDto>(0, new List<ChatSessionListDto>()));
            }

            return null;
        }));
        Services.AddSingleton<IChatMessageAppService>(_messages);
        Services.AddSingleton<ISufiAIChatAppService>(_chat);
        Services.AddSingleton<IChatAssistantTranscriptWriter>(_transcripts);
        ComponentFactories.AddStub<ChatMessengerShell>(parameters => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "send-turn");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () =>
                parameters.Get(component => component.OnSendMessage).InvokeAsync(new ChatComposerSendRequest
                {
                    Body = "user turn"
                })));
            builder.AddContent(3, "Send");
            builder.CloseElement();

            builder.OpenElement(4, "button");
            builder.AddAttribute(5, "class", "switch-session");
            builder.AddAttribute(6, "onclick", EventCallback.Factory.Create(this, () =>
                parameters.Get(component => component.OnSessionSelected).InvokeAsync(_otherSessionId)));
            builder.AddContent(7, "Switch");
            builder.CloseElement();
        });
    }

    [Fact]
    public async Task Retry_keeps_the_original_operation_and_does_not_send_or_call_the_model_again()
    {
        var cut = Render<WorkspaceChat>();
        var sessionId = _sessionId;
        Set(cut.Instance, "_workspaceId", (Guid?)Guid.NewGuid());
        Set(cut.Instance, "_workspaceName", "workspace");
        Set(cut.Instance, "_selectedRoute", new AIModelRouteDto { IsReady = true, IsUserSelectable = true });
        var state = (ChatMessengerState)Get(cut.Instance, "_messengerState")!;
        state.SelectedSessionId = sessionId;

        await cut.Find(".send-turn").ClickAsync();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".workspace-chat-page__retry")));
        Assert.Equal(1, _messages.Sends);
        Assert.Equal(1, _chat.Calls);
        Assert.Equal(1, _transcripts.Calls);
        var operationId = Assert.Single(_transcripts.OperationIds);
        var userTurnId = _messages.UserMessageId;

        await cut.Find(".workspace-chat-page__retry").ClickAsync();

        cut.WaitForAssertion(() => Assert.Throws<ElementNotFoundException>(() => cut.Find(".workspace-chat-page__retry")));
        Assert.Equal(1, _messages.Sends);
        Assert.Equal(1, _chat.Calls);
        Assert.Equal(2, _transcripts.Calls);
        Assert.Equal(new[] { operationId, operationId }, _transcripts.OperationIds);
        Assert.Equal(userTurnId, _messages.UserMessageId);
        Assert.Equal(sessionId, _transcripts.SessionIds.Distinct().Single());
        var saved = (ChatMessengerState)Get(cut.Instance, "_messengerState")!;
        Assert.Contains(saved.Messages, message => message.SenderKind == ChatMessageSenderKind.Assistant);
    }

    [Fact]
    public async Task Switching_sessions_clears_the_retry_button()
    {
        var cut = Render<WorkspaceChat>();
        Set(cut.Instance, "_workspaceId", (Guid?)Guid.NewGuid());
        Set(cut.Instance, "_workspaceName", "workspace");
        Set(cut.Instance, "_selectedRoute", new AIModelRouteDto { IsReady = true, IsUserSelectable = true });
        var state = (ChatMessengerState)Get(cut.Instance, "_messengerState")!;
        state.SelectedSessionId = _sessionId;

        await cut.Find(".send-turn").ClickAsync();
        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".workspace-chat-page__retry")));

        await cut.Find(".switch-session").ClickAsync();

        cut.WaitForAssertion(() => Assert.Throws<ElementNotFoundException>(() => cut.Find(".workspace-chat-page__retry")));
        Assert.Equal(1, _messages.Sends);
        Assert.Equal(1, _chat.Calls);
        Assert.Equal(1, _transcripts.Calls);
    }

    private static void Set(WorkspaceChat page, string fieldName, object? value)
    {
        typeof(WorkspaceChat).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, value);
    }

    private static object? Get(WorkspaceChat page, string fieldName)
    {
        return typeof(WorkspaceChat).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(page);
    }

    private static T Dispatch<T>(Func<MethodInfo, object?[], object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, ServiceProxy>();
        ((ServiceProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    private sealed class RecordingTranscripts : IChatAssistantTranscriptWriter
    {
        public int Calls { get; private set; }

        public List<Guid> OperationIds { get; } = new();

        public List<Guid> SessionIds { get; } = new();

        public Task<List<ChatMessageDto>> SaveForCurrentUserAsync(SendAssistantTranscriptInput input)
        {
            Calls++;
            OperationIds.Add(input.OperationId);
            SessionIds.Add(input.SessionId);
            if (Calls == 1)
            {
                throw new InvalidOperationException("delivery failed");
            }

            return Task.FromResult(new List<ChatMessageDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SessionId = input.SessionId,
                    Body = input.Body,
                    SenderKind = ChatMessageSenderKind.Assistant
                }
            });
        }
    }

    private sealed class RecordingMessages : IChatMessageAppService
    {
        public int Sends { get; private set; }

        public Guid UserMessageId { get; } = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        public Task<ChatMessageDto> SendAsync(SendChatMessageInput input)
        {
            Sends++;
            return Task.FromResult(new ChatMessageDto
            {
                Id = UserMessageId,
                SessionId = input.SessionId,
                Body = input.Body,
                SenderKind = ChatMessageSenderKind.Visitor
            });
        }

        public Task<PagedResultDto<ChatMessageDto>> GetListAsync(GetChatMessageListInput input)
        {
            return Task.FromResult(new PagedResultDto<ChatMessageDto>(0, new List<ChatMessageDto>()));
        }

        public Task DeleteAsync(Guid messageId) => Task.CompletedTask;

        public Task ClearSessionMessagesAsync(Guid sessionId) => Task.CompletedTask;
    }

    private sealed class RecordingChat : ISufiAIChatAppService
    {
        public int Calls { get; private set; }

        public Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new SufiAIChatResponseDto { Message = "same reply" });
        }

        public Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input)
        {
            throw new NotSupportedException();
        }

        public async IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input)
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private class ServiceProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?>? Handler { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var result = Handler?.Invoke(targetMethod!, args ?? Array.Empty<object?>());
            if (result != null || targetMethod == null)
            {
                return result;
            }

            if (targetMethod.ReturnType == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var inner = targetMethod.ReturnType.GetGenericArguments()[0];
                object? value = inner == typeof(List<WorkspaceDto>)
                    ? new List<WorkspaceDto>()
                    : inner.GetConstructor(Type.EmptyTypes) != null
                        ? Activator.CreateInstance(inner)
                        : null;
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, new[] { value });
            }

            return null;
        }
    }

    private sealed class TestPageLayout : IPageLayout
    {
        public string? Title { get; set; }

        public string? Subtitle { get; set; }

        public ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; } = new();

        public ObservableCollection<PageToolbarItem> ToolbarItems { get; } = new();

        public object? ToolbarContent { get; set; }

        public void Reset()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { }
            remove { }
        }
    }

    private sealed class TestExceptionInformer : IUserExceptionInformer
    {
        public void Inform(UserExceptionInformerContext context)
        {
        }

        public Task InformAsync(UserExceptionInformerContext context) => Task.CompletedTask;

        public string GetLocalizedMessage(Exception exception) => exception.Message;
    }

    private sealed class TestLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new TestLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new TestLocalizer();
    }

    private sealed class TestLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
    }
}
