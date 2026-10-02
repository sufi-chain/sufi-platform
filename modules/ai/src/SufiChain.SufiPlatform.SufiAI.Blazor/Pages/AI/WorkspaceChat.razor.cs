using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.SufiAI.Blazor.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Services;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.UI.Layout;
using Volo.Abp.Authorization;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WorkspaceChat : AIComponentBase
{
    private readonly ChatMessengerState _messengerState = new();
    private List<WorkspaceDto> _workspaces = new();
    private Guid? _workspaceId;
    private string _workspaceName = string.Empty;
    private bool _loadingWorkspaces;
    private bool _workspacesLoadStarted;
    private bool _savedMode = true;
    private bool _sending;
    private bool _hasTranscriptionRoute;
    private Guid? _modelConfigurationId;
    private string? _reasoningEffort;
    private OpenAIApiMode _apiMode = OpenAIApiMode.ChatCompletions;
    private AIModelRouteDto? _selectedRoute;
    private ChatComposerCapabilitiesDto? _capabilities;
    private string _anonymousDraft = string.Empty;
    private List<ChatMessageDto> _anonymousMessages = new();

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    [Inject] protected ISufiAIChatAppService ChatAppService { get; set; } = default!;

    [Inject] protected IAIAppService ModelAppService { get; set; } = default!;

    [Inject] protected IChatSessionAppService SessionAppService { get; set; } = default!;

    [Inject] protected IChatMessageAppService MessageAppService { get; set; } = default!;

    [Inject] protected IChatComposerCapabilitiesAppService ComposerCapabilitiesAppService { get; set; } = default!;

    [Inject] protected ISessionStorageService SessionStorage { get; set; } = default!;

    private IFileStorageIntegrationService? _fileStorage;
    private bool _fileStorageResolved;

    [Inject] protected IWorkspaceAppService Workspaces { get; set; } = default!;

    protected ChatMessengerState MessengerState => _messengerState;

    protected bool HasSelectedSession => _savedMode && MessengerState.SelectedSessionId.HasValue;

    protected bool CanStartSavedChat => _savedMode && HasSelectedSession;

    protected bool AcceptsImage => _selectedRoute?.AcceptsImageInput == true;

    protected bool AcceptsFile => _selectedRoute?.AcceptsFileInput == true;

    protected bool HideVoice => !_savedMode || !_hasTranscriptionRoute || !HasSelectedSession;

    protected bool HideAttachments => !_savedMode || (!AcceptsImage && !AcceptsFile) || !HasSelectedSession;

    protected RenderFragment ModelToolbar => builder =>
    {
        if (_workspaceId is not Guid workspaceId)
        {
            return;
        }

        builder.OpenComponent<WorkspaceModelSelector>(0);
        builder.AddAttribute(1, "WorkspaceId", workspaceId);
        builder.AddAttribute(2, "Compact", true);
        builder.AddAttribute(3, "Disabled", _sending);
        builder.AddAttribute(4, "ModelConfigurationId", _modelConfigurationId);
        builder.AddAttribute(5, "ModelConfigurationIdChanged", EventCallback.Factory.Create<Guid?>(this, OnModelChangedAsync));
        builder.AddAttribute(6, "ReasoningEffort", _reasoningEffort);
        builder.AddAttribute(7, "ReasoningEffortChanged", EventCallback.Factory.Create<string?>(this, OnEffortChangedAsync));
        builder.AddAttribute(8, "ApiMode", _apiMode);
        builder.AddAttribute(9, "ApiModeChanged", EventCallback.Factory.Create<OpenAIApiMode>(this, OnApiModeChangedAsync));
        builder.AddAttribute(10, "RouteChanged", EventCallback.Factory.Create<AIModelRouteDto?>(this, OnRouteChangedAsync));
        builder.CloseComponent();
    };

    protected override void OnInitialized()
    {
        PageLayout.Title = L["WorkspaceChat"];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender || _workspacesLoadStarted || !IsInteractive)
        {
            return;
        }

        _workspacesLoadStarted = true;
        await LoadWorkspacesAsync();
    }

    private async Task LoadWorkspacesAsync()
    {
        _loadingWorkspaces = true;
        try
        {
            var result = await Workspaces.GetLookupAsync();
            _workspaces = result.Where(workspace => workspace.IsActive).ToList();
        }
        finally
        {
            _loadingWorkspaces = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _workspaceId = workspaceId;
        var workspace = _workspaces.FirstOrDefault(item => item.Id == workspaceId);
        _workspaceName = workspace?.Name ?? string.Empty;
        _selectedRoute = null;
        _modelConfigurationId = null;
        _reasoningEffort = null;
        _apiMode = OpenAIApiMode.ChatCompletions;
        _hasTranscriptionRoute = false;
        MessengerState.Sessions = new List<ChatSessionListDto>();
        MessengerState.ClearMessages();
        MessengerState.SelectedSessionId = null;
        MessengerState.SelectedSession = null;
        _anonymousMessages = new List<ChatMessageDto>();
        _anonymousDraft = string.Empty;
        if (workspaceId is not Guid id)
        {
            return;
        }

        _savedMode = await ReadSavedModeAsync(id);
        await LoadTranscriptionAvailabilityAsync(id);
        await LoadSavedSessionsAsync();
        if (!_savedMode)
        {
            await LoadAnonymousTranscriptAsync(id);
        }

        await RefreshCapabilitiesAsync();
    }

    private async Task SetModeAsync(bool saved)
    {
        if (_savedMode == saved || _workspaceId is not Guid workspaceId)
        {
            return;
        }

        _savedMode = saved;
        await SessionStorage.SetItemAsync(ModeKey(workspaceId), saved ? "saved" : "anonymous");
        MessengerState.ClearMessages();
        MessengerState.SelectedSessionId = null;
        MessengerState.SelectedSession = null;
        if (saved)
        {
            await LoadSavedSessionsAsync();
        }
        else
        {
            await LoadAnonymousTranscriptAsync(workspaceId);
        }

        await RefreshCapabilitiesAsync();
    }

    private Task StartNewSavedChatAsync()
    {
        MessengerState.ClearMessages();
        MessengerState.SelectedSessionId = null;
        MessengerState.SelectedSession = null;
        MessengerState.DraftMessage = string.Empty;
        return Task.CompletedTask;
    }

    private async Task OnSessionSelectedAsync(Guid sessionId)
    {
        if (!_savedMode)
        {
            await SetModeAsync(true);
        }

        var session = await SessionAppService.GetAsync(sessionId);
        MessengerState.SelectedSession = session;
        MessengerState.SelectedSessionId = session.Id;
        var messages = await MessageAppService.GetListAsync(new GetChatMessageListInput
        {
            SessionId = sessionId,
            MaxResultCount = 100
        });
        MessengerState.ReplaceMessages(messages.Items);
        await RefreshCapabilitiesAsync();
    }

    private Task OnModelChangedAsync(Guid? modelConfigurationId)
    {
        _modelConfigurationId = modelConfigurationId;
        return Task.CompletedTask;
    }

    private Task OnEffortChangedAsync(string? effort)
    {
        _reasoningEffort = effort;
        return Task.CompletedTask;
    }

    private Task OnApiModeChangedAsync(OpenAIApiMode mode)
    {
        _apiMode = mode;
        return Task.CompletedTask;
    }

    private async Task OnRouteChangedAsync(AIModelRouteDto? route)
    {
        _selectedRoute = route;
        _modelConfigurationId = route is { IsUserSelectable: true } ? route.Id : null;

        await RefreshCapabilitiesAsync();
    }

    private Task OnAnonymousDraftChangedAsync(string draft)
    {
        _anonymousDraft = draft;
        return Task.CompletedTask;
    }

    private Task OnSavedDraftChangedAsync(string draft)
    {
        MessengerState.DraftMessage = draft;
        return Task.CompletedTask;
    }

    private async Task OnSendAsync(ChatComposerSendRequest request)
    {
        if (_sending || _workspaceId == null || string.IsNullOrWhiteSpace(_workspaceName) || string.IsNullOrWhiteSpace(request.Body))
        {
            return;
        }

        if (_selectedRoute is not { IsReady: true })
        {
            await Message.ErrorAsync(L["WorkspaceChat:NoSelectableChatRoutes"]);
            return;
        }

        _sending = true;
        MessengerState.IsSendingMessage = true;
        try
        {
            if (_savedMode)
            {
                await SendSavedAsync(request);
            }
            else
            {
                await SendAnonymousAsync(request.Body.Trim());
            }
        }
        catch (Exception exception)
        {
            if (!IsDisposed)
            {
                await HandleErrorAsync(exception);
            }
        }
        finally
        {
            _sending = false;
            MessengerState.IsSendingMessage = false;
            MessengerState.IsWaitingForAiResponse = false;
            MessengerState.NotifyStateChanged();
        }
    }

    private async Task SendSavedAsync(ChatComposerSendRequest request)
    {
        var body = request.Body.Trim();
        var created = false;
        if (!MessengerState.SelectedSessionId.HasValue)
        {
            var session = await SessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = BuildTitle(body),
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Web,
                AssistantWorkspaceId = _workspaceId,
                AssistantWorkspaceName = _workspaceName,
                ExternalOrchestration = true,
                Origin = ChatSessionOrigin.WorkspaceChat,
                Participants = CurrentUser.Id is Guid userId
                    ? new List<AddChatParticipantInput>
                    {
                        new()
                        {
                            UserId = userId,
                            ParticipantKind = ChatMessageSenderKind.Visitor
                        }
                    }
                    : new List<AddChatParticipantInput>()
            });
            MessengerState.SelectedSession = session;
            MessengerState.SelectedSessionId = session.Id;
            MessengerState.Sessions.Insert(0, ToListItem(session));
            created = true;
        }

        var sessionId = MessengerState.SelectedSessionId!.Value;
        var userMessage = await MessageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = sessionId,
            Body = body,
            SenderKind = ChatMessageSenderKind.Visitor,
            AttachmentFileIds = request.AttachmentFileIds,
            ModelConfigurationId = _modelConfigurationId,
            ReasoningEffort = _reasoningEffort
        });
        MessengerState.AddMessageIfMissing(userMessage);
        MessengerState.DraftMessage = string.Empty;
        MessengerState.DraftAttachmentFileIds = new List<Guid>();
        if (created)
        {
            await SessionAppService.UpdateMySessionTitleAsync(sessionId, new UpdateChatSessionTitleInput
            {
                Title = BuildTitle(body)
            });
        }

        var history = MessengerState.Messages
            .Where(message => message.Id != userMessage.Id)
            .Select(message => new SufiAIChatMessageDto
            {
                Role = message.SenderKind == ChatMessageSenderKind.Assistant ? "assistant" : "user",
                Content = message.Body
            })
            .ToList();
        var attachments = await BuildAttachmentsAsync(request.AttachmentFileIds);
        var reply = new StringBuilder();
        var preview = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            SenderKind = ChatMessageSenderKind.Assistant,
            CreationTime = DateTime.UtcNow
        };
        MessengerState.IsWaitingForAiResponse = true;
        MessengerState.NotifyStateChanged();
        await foreach (var chunk in ChatAppService.StreamMessageAsync(new SufiAISendChatMessageInput
        {
            WorkspaceName = _workspaceName,
            ModelConfigurationId = _modelConfigurationId,
            Message = body,
            ReasoningEffort = _reasoningEffort,
            OpenAIApiMode = _apiMode,
            Attachments = attachments,
            ConversationHistory = history
        }))
        {
            if (string.IsNullOrEmpty(chunk.Message))
            {
                continue;
            }

            reply.Append(chunk.Message);
            preview.Body = reply.ToString();
            MessengerState.IsWaitingForAiResponse = false;
            MessengerState.AddMessageIfMissing(preview);
            MessengerState.NotifyStateChanged();
        }

        MessengerState.Messages.RemoveAll(message => message.Id == preview.Id);
        MessengerState.IsWaitingForAiResponse = false;
        if (reply.Length == 0)
        {
            MessengerState.NotifyStateChanged();
            await Message.ErrorAsync(L["WorkspaceChat:ReplyFailed"]);
            return;
        }

        var assistantMessage = await MessageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = sessionId,
            Body = reply.ToString(),
            SenderKind = ChatMessageSenderKind.Assistant,
            ModelConfigurationId = _modelConfigurationId,
            ReasoningEffort = _reasoningEffort
        });
        MessengerState.AddMessageIfMissing(assistantMessage);
        await LoadSavedSessionsAsync();
    }

    private async Task SendAnonymousAsync(string body)
    {
        var history = _anonymousMessages
            .Select(message => new SufiAIChatMessageDto
            {
                Role = message.SenderKind == ChatMessageSenderKind.Assistant ? "assistant" : "user",
                Content = message.Body
            })
            .ToList();
        _anonymousMessages.Add(new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            Body = body,
            SenderKind = ChatMessageSenderKind.Visitor,
            CreationTime = DateTime.UtcNow
        });
        _anonymousDraft = string.Empty;
        var reply = new StringBuilder();
        var preview = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SenderKind = ChatMessageSenderKind.Assistant,
            CreationTime = DateTime.UtcNow
        };
        await foreach (var chunk in ChatAppService.StreamMessageAsync(new SufiAISendChatMessageInput
        {
            WorkspaceName = _workspaceName,
            ModelConfigurationId = _modelConfigurationId,
            Message = body,
            ReasoningEffort = _reasoningEffort,
            OpenAIApiMode = _apiMode,
            ConversationHistory = history
        }))
        {
            if (string.IsNullOrEmpty(chunk.Message))
            {
                continue;
            }

            reply.Append(chunk.Message);
            preview.Body = reply.ToString();
            if (_anonymousMessages.All(message => message.Id != preview.Id))
            {
                _anonymousMessages.Add(preview);
            }

            await InvokeAsync(StateHasChanged);
        }

        _anonymousMessages.RemoveAll(message => message.Id == preview.Id);

        if (reply.Length == 0)
        {
            await Message.ErrorAsync(L["WorkspaceChat:ReplyFailed"]);
        }
        else
        {
            _anonymousMessages.Add(new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                Body = reply.ToString(),
                SenderKind = ChatMessageSenderKind.Assistant,
                CreationTime = DateTime.UtcNow
            });
        }

        await SaveAnonymousTranscriptAsync();
    }

    private async Task<List<SufiAIChatAttachmentInput>> BuildAttachmentsAsync(IReadOnlyList<Guid> fileIds)
    {
        var attachments = new List<SufiAIChatAttachmentInput>();
        var fileStorage = TryGetFileStorage();
        if (fileStorage == null)
        {
            return attachments;
        }

        foreach (var fileId in fileIds)
        {
            FileContentBytesDto content;
            try
            {
                content = await fileStorage.GetContentAsync(fileId);
            }
            catch (AbpAuthorizationException)
            {
                continue;
            }

            if (content.Content == null || content.Content.Length == 0)
            {
                continue;
            }

            var mime = string.IsNullOrWhiteSpace(content.MimeType) ? "application/octet-stream" : content.MimeType;
            var isImage = mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            var isPdf = string.Equals(mime, "application/pdf", StringComparison.OrdinalIgnoreCase)
                        || content.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            if (isImage && _selectedRoute?.AcceptsImageInput != true)
            {
                continue;
            }

            if (!isImage && (!isPdf || _selectedRoute?.AcceptsFileInput != true))
            {
                continue;
            }

            attachments.Add(new SufiAIChatAttachmentInput
            {
                Type = isImage ? "image" : "file",
                DataUrl = $"data:{mime};base64,{Convert.ToBase64String(content.Content)}",
                FileName = string.IsNullOrWhiteSpace(content.FileName) ? null : content.FileName,
                MimeType = mime
            });
        }

        return attachments;
    }

    private async Task LoadSavedSessionsAsync()
    {
        if (_workspaceId is not Guid workspaceId)
        {
            return;
        }

        try
        {
            var result = await SessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
            {
                Origin = ChatSessionOrigin.WorkspaceChat,
                ConversationKind = ConversationKind.Assistant,
                ExternalOrchestration = true,
                MaxResultCount = 50
            });
            MessengerState.Sessions = result.Items
                .Where(session => session.AssistantWorkspaceId == workspaceId)
                .ToList();
        }
        catch (AbpAuthorizationException)
        {
            MessengerState.Sessions = new List<ChatSessionListDto>();
        }

        MessengerState.NotifyStateChanged();
    }

    private async Task LoadTranscriptionAvailabilityAsync(Guid workspaceId)
    {
        try
        {
            var configurations = await ModelAppService.GetModelConfigurationsAsync(workspaceId);
            _hasTranscriptionRoute = configurations.Any(configuration =>
                configuration.IsEnabled &&
                configuration.CapabilityType == AICapabilityType.AudioTranscription);
        }
        catch (AbpAuthorizationException)
        {
            _hasTranscriptionRoute = false;
        }
    }

    private async Task RefreshCapabilitiesAsync()
    {
        ChatComposerCapabilitiesDto capabilities;
        try
        {
            capabilities = await ComposerCapabilitiesAppService.GetAsync(MessengerState.SelectedSessionId);
        }
        catch (AbpAuthorizationException)
        {
            capabilities = new ChatComposerCapabilitiesDto();
        }

        capabilities.CanShareLocation = false;
        var allowFiles = _savedMode && HasSelectedSession && (AcceptsImage || AcceptsFile);
        capabilities.CanAttachFiles = capabilities.CanAttachFiles && allowFiles;
        capabilities.CanRecordVoice = capabilities.CanRecordVoice && _savedMode && HasSelectedSession && _hasTranscriptionRoute;
        var fileTypes = ChatAttachmentAllowedFileTypes.None;
        if (AcceptsImage)
        {
            fileTypes |= ChatAttachmentAllowedFileTypes.Image;
        }

        if (AcceptsFile)
        {
            fileTypes |= ChatAttachmentAllowedFileTypes.Document;
        }

        capabilities.AllowedFileTypes = fileTypes;
        _capabilities = capabilities;
        MessengerState.ComposerCapabilities = capabilities;
        MessengerState.NotifyStateChanged();
    }

    private async Task<bool> ReadSavedModeAsync(Guid workspaceId)
    {
        var stored = await SessionStorage.GetItemAsync(ModeKey(workspaceId));
        return !string.Equals(stored, "anonymous", StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadAnonymousTranscriptAsync(Guid workspaceId)
    {
        var json = await SessionStorage.GetItemAsync(TranscriptKey(workspaceId));
        if (string.IsNullOrWhiteSpace(json))
        {
            _anonymousMessages = new List<ChatMessageDto>();
            return;
        }

        try
        {
            var stored = JsonSerializer.Deserialize<List<StoredTurn>>(json) ?? new List<StoredTurn>();
            _anonymousMessages = stored.Select(turn => new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                Body = turn.Body,
                SenderKind = turn.IsAssistant ? ChatMessageSenderKind.Assistant : ChatMessageSenderKind.Visitor,
                CreationTime = DateTime.UtcNow
            }).ToList();
        }
        catch (JsonException)
        {
            _anonymousMessages = new List<ChatMessageDto>();
        }
    }

    private Task SaveAnonymousTranscriptAsync()
    {
        if (_workspaceId is not Guid workspaceId)
        {
            return Task.CompletedTask;
        }

        var json = JsonSerializer.Serialize(_anonymousMessages.Select(message => new StoredTurn
        {
            IsAssistant = message.SenderKind == ChatMessageSenderKind.Assistant,
            Body = message.Body
        }));
        return SessionStorage.SetItemAsync(TranscriptKey(workspaceId), json).AsTask();
    }

    private string ModeKey(Guid workspaceId)
    {
        var tenantKey = CurrentTenant.Id?.ToString("N") ?? "host";
        return $"sufiai.workspace-chat.mode:{tenantKey}:{workspaceId:N}";
    }

    private string TranscriptKey(Guid workspaceId)
    {
        var tenantKey = CurrentTenant.Id?.ToString("N") ?? "host";
        return $"sufiai.workspace-chat.transcript:{tenantKey}:{workspaceId:N}";
    }

    private IFileStorageIntegrationService? TryGetFileStorage()
    {
        if (_fileStorageResolved)
        {
            return _fileStorage;
        }

        _fileStorageResolved = true;
        _fileStorage = ScopedServices.GetService<IFileStorageIntegrationService>();
        return _fileStorage;
    }

    private static string BuildTitle(string body)
    {
        var trimmed = body.Trim();
        return trimmed.Length <= 80 ? trimmed : trimmed[..80];
    }

    private static ChatSessionListDto ToListItem(ChatSessionDto session)
    {
        return new ChatSessionListDto
        {
            Id = session.Id,
            Title = session.Title,
            Status = session.Status,
            ConversationKind = session.ConversationKind,
            AccessMode = session.AccessMode,
            ChannelOrigin = session.ChannelOrigin,
            AssistantWorkspaceId = session.AssistantWorkspaceId,
            AssistantWorkspaceName = session.AssistantWorkspaceName,
            Origin = session.Origin,
            CreationTime = session.CreationTime,
            LastMessageTime = session.LastMessageTime
        };
    }

    private sealed class StoredTurn
    {
        public bool IsAssistant { get; set; }

        public string Body { get; set; } = string.Empty;
    }
}
