using System.Globalization;

using Microsoft.AspNetCore.Components;

using Microsoft.Extensions.Localization;

using Microsoft.Extensions.Logging;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Permissions;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor.Public.Components;

using SufiChain.SufiPlatform.Calendar.Availability;

using SufiChain.SufiPlatform.Calendar.Calendars;

using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

using SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Components;

using SufiChain.SufiPlatform.Calendar.Events;

using SufiChain.SufiPlatform.Calendar.Permissions;

using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Services;

using SufiChain.SufiPlatform.SufiCom.Chat.Composer;

using SufiChain.SufiPlatform.SufiCom.Chat.Messages;

using SufiChain.SufiPlatform.SufiCom.Chat.Permissions;

using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;

using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;

using SufiChain.SufiPlatform.SufiCom.Chat.Usage;

using SufiChain.SufiPlatform.SufiCom.Channels;

using SufiChain.SufiPlatform.SufiCom;

using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar.Features;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Licensing;
using Volo.Abp.Features;

using SufiChain.SufiPlatform.SufiCom.Chat;



namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Pages;



/// <summary>

/// Calendar page with an overlay hooshvare widget. Composes

/// <see cref="SufiCalendarScheduler"/> with <see cref="HooshvareChatWidget"/>.

/// The assistant is calendar-aware through the Calendar MCP tools, and the chat

/// composer supports press-and-hold voice/audio messages.

/// Uses only the public Chat surface (<c>Communications.Chat.Blazor.Public</c>).

/// </summary>

public partial class CalendarHooshvarePageBase : CalendarHooshvareComponentBase, IAsyncDisposable

{

    [Inject]

    protected IAvailabilityCalendarAppService CalendarAvailabilityAppService { get; set; } = default!;



    [Inject]

    protected ICalendarCatalogIntegrationService CalendarCatalog { get; set; } = default!;



    [Inject]

    protected IChatSessionAppService ChatSessionAppService { get; set; } = default!;



    [Inject]

    protected IChatMessageAppService ChatMessageAppService { get; set; } = default!;



    [Inject]

    protected IHooshvareCatalogAppService HooshvareCatalogAppService { get; set; } = default!;



    [Inject]

    new IStringLocalizerFactory StringLocalizerFactory { get; set; } = default!;



    [Inject]

    protected IChatAssistantWorkspaceResolver AssistantWorkspaceResolver { get; set; } = default!;



    [Inject]

    protected ChatMessengerState MessengerState { get; set; } = default!;



    [Inject]

    protected IChatHubClientService HubClientService { get; set; } = default!;



    [Inject]

    protected NavigationManager NavigationManager { get; set; } = default!;



    protected Guid? PersonalCalendarId { get; set; }



    protected Guid SelectedCalendarId { get; set; }



    protected ProCalendarSelector? CalendarSelector { get; set; }



    protected SufiCalendarScheduler? CalendarScheduler { get; set; }



    protected bool IsHooshvareOpen { get; set; }



    protected string HooshvareDisplayName { get; set; } = string.Empty;



    protected bool IsMobileWorkspacePage =>

        new Uri(NavigationManager.Uri).AbsolutePath.EndsWith("/mobile", StringComparison.OrdinalIgnoreCase);



    protected ChatAssistantAvailabilityDto? AssistantAvailability { get; set; }



    protected bool IsAssistantAvailable => AssistantAvailability?.IsAvailable == true;



    protected bool CanUseHooshvare { get; set; }



    protected string AssistantHooshvareKey => CalendarAssistantHooshvareKeys.Key;



    protected IReadOnlyList<string> CalendarShortcutPrompts { get; private set; } = Array.Empty<string>();



    protected IReadOnlyList<string> EffectiveShortcutPrompts =>

        CalendarShortcutPrompts.Count > 0 ? CalendarShortcutPrompts : DefaultShortcutPrompts;



    private static readonly IReadOnlyList<string> DefaultShortcutPrompts = new[]

    {

        "What does my day look like?",

        "Find a free 30 minute slot this week",

        "Schedule a meeting tomorrow at 10am",

        "Am I free on Friday afternoon?"

    };



    protected bool HasActiveAssistantSession =>

        MessengerState.SelectedSession?.ConversationKind == ConversationKind.Assistant;



    protected string? WorkspaceName => AssistantAvailability?.DefaultWorkspaceName

                                       ?? AssistantAvailability?.Assistants.FirstOrDefault()?.WorkspaceName;



    protected Guid? _calendarWorkspaceId;



    protected Guid? _calendarAssistantRuntimeId;



    protected bool _allowUserModelSelection;

    protected bool _allowComposerEmoji = true;

    protected bool _allowComposerAudio;

    protected bool _allowComposerAttachment;

    protected bool _allowComposerContext;



    protected Guid? _modelConfigurationId;



    private Guid? _activeHubSessionId;



    protected override async Task OnInitializedAsync()

    {

        HubClientService.MessageReceived += OnMessageReceivedAsync;

        HubClientService.UsageLimitExceeded += OnUsageLimitExceededAsync;

        HubClientService.AssistantProgress += OnAssistantProgressAsync;



        try

        {

        if (await IsGrantedAsync(CalendarPermissions.Events.Default))

        {

            var personalCalendar = await CalendarAvailabilityAppService.GetOrCreateMyPersonalCalendarAsync();

            PersonalCalendarId = personalCalendar.Id;

            SelectedCalendarId = personalCalendar.Id;

        }



        CanUseHooshvare = await CanUseHooshvareAsync();

        if (!CanUseHooshvare)

        {

            DisableHooshvare("CalendarHooshvare:Unauthorized");

            return;

        }



        try

        {

            AssistantAvailability = await LoadCalendarAssistantAvailabilityAsync();

        }

        catch (Exception ex)

        {

            Logger.LogWarning(ex, "Could not resolve Calendar assistant availability.");

        }



        if (string.IsNullOrWhiteSpace(HooshvareDisplayName))

        {

            HooshvareDisplayName = L["CalendarHooshvare:OpenAssistant"];

        }



        if (IsAssistantAvailable)

        {

            try

            {

                await EnsureAssistantSessionAsync();

            }

            catch (Volo.Abp.Authorization.AbpAuthorizationException ex)

            {

                Logger.LogWarning(ex, "Calendar hooshvare is hidden because the current user is not authorized for chat or AI assistant capabilities.");

                DisableHooshvare("CalendarHooshvare:Unauthorized");

            }

        }

        }

        catch (Exception exception)

        {

            Logger.LogWarning(exception, "Calendar page could not be initialized.");

            DisableHooshvare("CalendarHooshvare:LoadFailed");

        }

    }



    protected virtual async Task<ChatAssistantAvailabilityDto> LoadCalendarAssistantAvailabilityAsync()

    {

        var hooshvare = await HooshvareCatalogAppService.GetByKeyAsync(CalendarAssistantHooshvareKeys.Key);

        HooshvareDisplayName = hooshvare.DisplayName;

        _calendarAssistantRuntimeId = hooshvare.Id;

        _calendarWorkspaceId = hooshvare.WorkspaceId;

        _allowUserModelSelection = hooshvare.AllowUserModelSelection;

        _allowComposerEmoji = hooshvare.AllowComposerEmoji;

        _allowComposerAudio = hooshvare.AllowComposerAudio;

        _allowComposerAttachment = hooshvare.AllowComposerAttachment;

        _allowComposerContext = hooshvare.AllowComposerContext;



        var workspaceName = await AssistantWorkspaceResolver.ResolveWorkspaceNameAsync(

            new ChatAssistantWorkspaceResolveContext

            {

                AssistantId = hooshvare.Id

            });



        if (string.IsNullOrWhiteSpace(workspaceName))

        {

            return new ChatAssistantAvailabilityDto

            {

                IsAvailable = false,

                ReasonCode = "CalendarAssistantWorkspaceUnavailable",

                MessageKey = "CalendarHooshvare:Unavailable"

            };

        }



        CalendarShortcutPrompts = HooshvareShortcutPromptsDocument

            .Resolve(hooshvare.ShortcutPromptsJson, CultureInfo.CurrentUICulture.Name, StringLocalizerFactory)

            .ToList();



        var assistant = new ChatAssistantPickerOptionDto

        {

            Id = hooshvare.Id,

            DisplayName = hooshvare.DisplayName,

            WorkspaceId = hooshvare.WorkspaceId,

            WorkspaceName = workspaceName,

            IsDefault = true

        };



        return new ChatAssistantAvailabilityDto

        {

            IsAvailable = true,

            DefaultWorkspaceName = workspaceName,

            Assistants = new List<ChatAssistantPickerOptionDto> { assistant }

        };

    }



    protected virtual async Task<bool> CanUseHooshvareAsync()

    {

        // Events.Default ensures SelectedCalendarId is set before send; Chat/AI gates hide the widget.

        var features = ScopedServices.GetRequiredService<IFeatureChecker>();
        if (!CalendarAssistantChrome.ShowsChrome(await features.IsEnabledAsync(CalendarFeatures.Assistant)))
        {
            return false;
        }

        var license = ScopedServices.GetService<ILicenseGate>();
        if (license != null && !await license.CanWriteAsync(LicenseProducts.Calendar))
        {
            return false;
        }

        return await IsGrantedAsync(CalendarPermissions.Events.Default) &&

               await IsGrantedAsync(ChatPermissions.Sessions.Default) &&

               await IsGrantedAsync(ChatPermissions.Sessions.Create) &&

               await IsGrantedAsync(AIHooshvarePermissions.Use) &&

               await IsGrantedAnyAsync(ChatPermissions.Inbox.User, ChatPermissions.Inbox.Reply, ChatPermissions.Inbox.Operator, ChatPermissions.Messages.Default) &&

               await IsGrantedAnyAsync(ChatPermissions.Inbox.User, ChatPermissions.Inbox.Reply, ChatPermissions.Messages.Send);

    }



    protected virtual async Task<bool> IsGrantedAnyAsync(params string[] permissionNames)

    {

        foreach (var permissionName in permissionNames)

        {

            if (await IsGrantedAsync(permissionName))

            {

                return true;

            }

        }



        return false;

    }



    protected virtual void DisableHooshvare(string reasonCode)

    {

        CanUseHooshvare = false;

        IsHooshvareOpen = false;

        AssistantAvailability = new ChatAssistantAvailabilityDto

        {

            IsAvailable = false,

            ReasonCode = reasonCode,

            MessageKey = "CalendarHooshvare:Unavailable"

        };

        CalendarShortcutPrompts = Array.Empty<string>();

        MessengerState.SelectedSessionId = null;

        MessengerState.SelectedSession = null;

        MessengerState.IsLoadingMessages = false;

        MessengerState.IsSendingMessage = false;

        MessengerState.IsWaitingForAiResponse = false;

        MessengerState.NotifyStateChanged();

    }



    protected virtual async Task EnsureAssistantSessionAsync()

    {

        MessengerState.IsLoadingMessages = true;

        MessengerState.NotifyStateChanged();



        try

        {

            var session = await FindOrCreateAssistantSessionAsync();

            MessengerState.SelectedSessionId = session.Id;

            MessengerState.SelectedSession = session;

            MessengerState.SelectSession(session.Id);



            await LoadMessagesAsync(session.Id);

            await HubClientService.EnsureConnectedAsync();

            if (await HubClientService.JoinSessionAsync(session.Id))

            {

                _activeHubSessionId = session.Id;

            }

        }

        finally

        {

            MessengerState.IsLoadingMessages = false;

            MessengerState.NotifyStateChanged();

        }

    }



    protected virtual async Task<ChatSessionDto> FindOrCreateAssistantSessionAsync()

    {

        var existing = await FindExistingAssistantSessionAsync();

        if (existing != null)

        {

            return await ChatSessionAppService.GetAsync(existing.Id);

        }



        var assistantId = _calendarAssistantRuntimeId

                          ?? throw new InvalidOperationException("Calendar assistant runtime id is not resolved.");

        var workspaceId = _calendarWorkspaceId ?? Guid.Empty;

        var participants = CurrentUser.Id.HasValue

            ? new List<AddChatParticipantInput>

            {

                new()

                {

                    UserId = CurrentUser.Id,

                    ParticipantKind = ChatMessageSenderKind.Visitor,

                    DisplayName = CurrentUser.UserName

                }

            }

            : new List<AddChatParticipantInput>();



        var session = await ChatSessionAppService.CreateAsync(new CreateChatSessionInput

        {

            Title = L["CalendarHooshvare:Title"],

            AccessMode = AccessMode.PublicAuthenticated,

            ConversationKind = ConversationKind.Assistant,

            ChannelOrigin = ChannelOrigin.Web,

            AssistantWorkspaceId = workspaceId,

            AssistantId = assistantId,

            ExternalOrchestration = false,

            Origin = ChatSessionOrigin.Contextual,

            Participants = participants

        });



        MessengerState.Sessions.Insert(0, new ChatSessionListDto

        {

            Id = session.Id,

            TenantId = session.TenantId,

            Title = session.Title,

            AccessMode = session.AccessMode,

            ConversationKind = session.ConversationKind,

            ChannelOrigin = session.ChannelOrigin,

            Status = session.Status,

            AssistantWorkspaceId = session.AssistantWorkspaceId,

            AssistantWorkspaceName = session.AssistantWorkspaceName,

            AssistantId = session.AssistantId,

            IsExternallyOrchestrated = session.IsExternallyOrchestrated,

            Origin = session.Origin,

            LastMessageTime = session.LastMessageTime,

            ParticipantCount = session.Participants.Count,

            CreationTime = session.CreationTime

        });



        return session;

    }



    protected virtual async Task<ChatSessionListDto?> FindExistingAssistantSessionAsync()

    {

        var assistantId = _calendarAssistantRuntimeId;

        if (!assistantId.HasValue)

        {

            return null;

        }



        // Only reuse chat-handler sessions. Playground / HooshvareRuntime sessions set

        // ExternalOrchestration=true; ChatAssistantMessageHandler skips those and the UI

        // would hang forever on "Thinking...".

        var result = await ChatSessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput

        {

            MaxResultCount = 50,

            ConversationKind = ConversationKind.Assistant,

            AssistantId = assistantId,

            Origin = ChatSessionOrigin.Contextual,

            ExternalOrchestration = false

        });



        return result.Items.FirstOrDefault(session =>

            session.Status != ChatSessionStatus.Closed &&

            session.AssistantId == assistantId.Value &&

            !session.IsExternallyOrchestrated);

    }



    protected virtual async Task LoadMessagesAsync(Guid sessionId)

    {

        var result = await ChatMessageAppService.GetListAsync(new GetChatMessageListInput

        {

            SessionId = sessionId,

            MaxResultCount = 200,

            Sorting = "CreationTime"

        });



        if (MessengerState.SelectedSessionId != sessionId)

        {

            Logger.LogInformation(

                "Skipped stale calendar transcript load. RequestedSessionId={RequestedSessionId} SelectedSessionId={SelectedSessionId} IncomingCount={IncomingCount}",

                sessionId,

                MessengerState.SelectedSessionId,

                result.Items?.Count ?? 0);

            return;

        }



        var incoming = result.Items ?? Array.Empty<ChatMessageDto>();

        if (incoming.Count == 0 &&

            MessengerState.Messages.Count > 0 &&

            MessengerState.IsWaitingForAiResponse)

        {

            Logger.LogWarning(

                "Skipped empty calendar transcript replace while waiting for AI. SessionId={SessionId} LocalCount={LocalCount}",

                sessionId,

                MessengerState.Messages.Count);

            return;

        }



        MessengerState.ReplaceMessages(incoming);

    }



    protected virtual async Task OnSendMessageAsync(ChatComposerSendRequest request)

    {

        if (!CanUseHooshvare)

        {

            return;

        }



        if (!MessengerState.SelectedSessionId.HasValue)

        {

            return;

        }



        if (SelectedCalendarId == Guid.Empty)

        {

            Logger.LogWarning(

                "Calendar hooshvare send blocked because SelectedCalendarId is empty. SessionId={SessionId}",

                MessengerState.SelectedSessionId);

            return;

        }



        var hasContent = !string.IsNullOrWhiteSpace(request.Body) ||

                         request.AttachmentFileIds.Count > 0 ||

                         request.Metadata != null;



        if (!hasContent)

        {

            return;

        }



        try

        {

            MessengerState.IsSendingMessage = true;

            MessengerState.ClearAiProgress();

            MessengerState.IsWaitingForAiResponse = true;

            var localVisitorTurn = MessengerState.BeginLocalVisitorTurn(

                request.Body.Trim(),

                MessengerState.SelectedSessionId.Value,

                CurrentUser.Id);

            MessengerState.NotifyStateChanged();

            await InvokeAsync(StateHasChanged);



            var sent = await ChatMessageAppService.SendAsync(new SendChatMessageInput

            {

                SessionId = MessengerState.SelectedSessionId.Value,

                Body = request.Body.Trim(),

                SenderKind = ChatMessageSenderKind.Visitor,

                SenderUserId = CurrentUser.Id,

                AccessMode = AccessMode.PublicAuthenticated,

                Metadata = request.Metadata,

                ExtraProperties =

                {

                    [HooshvareRequestContextMetadata.HooshvareContextKey] =

                        await BuildHooshvareContextAsync()

                },

                AttachmentFileIds = request.AttachmentFileIds,

                ModelConfigurationId = request.ModelConfigurationId ?? _modelConfigurationId,
                ReasoningEffort = request.ReasoningEffort

            });



            MessengerState.ConfirmLocalVisitorTurn(localVisitorTurn, sent);

            MessengerState.ClearDraft();

            MessengerState.ClearSignupRequired();

        }

        catch (Exception ex)

        {

            Logger.LogException(ex);

            MessengerState.CompleteAiResponseWait();

        }

        finally

        {

            MessengerState.IsSendingMessage = false;

            MessengerState.NotifyStateChanged();

            await InvokeAsync(StateHasChanged);

        }

    }



    protected virtual async Task<Dictionary<string, string>> BuildHooshvareContextAsync()

    {

        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)

        {

            [CalendarHooshvareContextKeys.CalendarId] = SelectedCalendarId.ToString("D")

        };



        try

        {

            var calendars = await CalendarCatalog.GetVisibleCalendarsAsync();

            if (calendars.Count > 0)

            {

                context[CalendarHooshvareContextKeys.Calendars] = CalendarVisibleCatalogJson.Serialize(calendars);

            }

        }

        catch (Exception ex)

        {

            Logger.LogWarning(ex, "Calendar hooshvare could not attach the visible calendar catalog to request context.");

        }



        return context;

    }



    protected virtual Task OnMessageReceivedAsync(ChatMessageDto message)

    {

        if (MessengerState.SelectedSessionId != message.SessionId)

        {

            return Task.CompletedTask;

        }



        if (!MessengerState.ApplyIncomingMessage(message, MessengerState.SelectedSessionId))

        {

            return Task.CompletedTask;

        }



        return InvokeAsync(StateHasChanged);

    }



    protected virtual Task OnAssistantProgressAsync(ChatAssistantProgressDto progress)

    {

        if (MessengerState.SelectedSessionId != progress.SessionId)

        {

            return Task.CompletedTask;

        }



        MessengerState.ApplyAssistantProgress(progress);

        return InvokeAsync(StateHasChanged);

    }



    protected virtual Task OnUsageLimitExceededAsync(ChatUsageCheckResultDto result)

    {

        MessengerState.ApplyUsageLimit(result);

        return InvokeAsync(StateHasChanged);

    }



    protected virtual async Task RefreshAsync()

    {

        if (CalendarSelector != null)

        {

            await CalendarSelector.RefreshAsync();

        }



        if (CalendarScheduler != null)

        {

            await CalendarScheduler.RefreshAsync();

        }



        StateHasChanged();

    }



    protected virtual Task OnSelectedCalendarIdChangedAsync(Guid calendarId)

    {

        SelectedCalendarId = calendarId;

        return Task.CompletedTask;

    }



    protected virtual Task OnEventChangedAsync(CalendarEventDto calendarEvent)

    {

        StateHasChanged();

        return Task.CompletedTask;

    }



    protected virtual Task OnHooshvareOpenChangedAsync(bool value)

    {

        IsHooshvareOpen = CanUseHooshvare && IsAssistantAvailable && value;

        return Task.CompletedTask;

    }



    protected virtual Task OnShortcutSelectedAsync(string shortcut)

    {

        MessengerState.DraftMessage = $"{shortcut}: ";

        MessengerState.NotifyStateChanged();

        return Task.CompletedTask;

    }



    protected string? GetSignInUrl()

    {

        return NavigationManager.ToAbsoluteUri("/account/login").ToString();

    }



    protected Task OnModelConfigurationIdChangedAsync(Guid? modelConfigurationId)

    {

        _modelConfigurationId = modelConfigurationId;

        return Task.CompletedTask;

    }



    public async ValueTask DisposeAsync()

    {

        HubClientService.MessageReceived -= OnMessageReceivedAsync;

        HubClientService.UsageLimitExceeded -= OnUsageLimitExceededAsync;

        HubClientService.AssistantProgress -= OnAssistantProgressAsync;



        if (_activeHubSessionId.HasValue)

        {

            await HubClientService.LeaveSessionAsync(_activeHubSessionId.Value);

        }

    }

}
