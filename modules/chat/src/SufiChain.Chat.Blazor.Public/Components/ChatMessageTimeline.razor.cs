using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.FileManager.Blazor.Public.Services;
using SufiChain.SufiBlazor.Components;
using Volo.Abp;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatMessageTimeline : ChatPublicComponentBase
{
    protected IFileItemUrlProvider FileItemUrlProvider => LazyGetRequiredService(ref _fileItemUrlProvider);
    private IFileItemUrlProvider? _fileItemUrlProvider;

    [Parameter]
    public IEnumerable<ChatMessageDto> Messages { get; set; } = Enumerable.Empty<ChatMessageDto>();

    [Parameter]
    public ChatSessionDto? Session { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }
    [Parameter]
    public bool IsWaitingForAiResponse { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment<ChatMessageDto>? MessageActions { get; set; }

    /// <summary>
    /// Text direction from the active UI culture for message content and support layouts.
    /// </summary>
    protected string DocumentDirection =>
        CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";

    /// <summary>
    /// Peer chats use LTR on the messages column so physical left/right alignment stays stable in RTL locales.
    /// </summary>
    protected string GetMessagesDirection() =>
        UsePeerParticipantLayout ? "ltr" : DocumentDirection;

    /// <summary>
    /// DM and group chats use peer alignment (own right / others left). Support and operator inboxes keep role-based layout.
    /// </summary>
    protected bool UsePeerParticipantLayout =>
        Session?.ConversationKind is ConversationKind.Direct or ConversationKind.Group;

    protected bool IsGroupChat => Session?.ConversationKind == ConversationKind.Group;

    protected virtual string GetTimelineClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        if (UsePeerParticipantLayout)
        {
            classes.Add("chat-message-timeline--peer");
        }

        return string.Join(' ', classes);
    }

    protected virtual string GetSenderLabel(ChatMessageDto message)
    {
        if (UsePeerParticipantLayout)
        {
            var displayName = ResolveSenderDisplayName(message);
            return !displayName.IsNullOrWhiteSpace()
                ? displayName
                : L["Messenger:Sender:Participant"];
        }

        return L[$"Messenger:Sender:{message.SenderKind}"];
    }

    protected virtual string GetSenderClass(ChatMessageDto message)
    {
        if (UsePeerParticipantLayout)
        {
            if (message.SenderKind == ChatMessageSenderKind.System)
            {
                return "system";
            }

            return IsOwnMessage(message) ? "mine" : "theirs";
        }

        return message.SenderKind switch
        {
            ChatMessageSenderKind.Visitor => "visitor",
            ChatMessageSenderKind.Operator => "operator",
            ChatMessageSenderKind.Assistant => "ai",
            ChatMessageSenderKind.System => "system",
            _ => "default"
        };
    }

    protected virtual bool ShouldShowSenderHeader(ChatMessageDto message)
    {
        if (UsePeerParticipantLayout)
        {
            return IsGroupChat
                   && !IsOwnMessage(message)
                   && message.SenderKind != ChatMessageSenderKind.System;
        }

        return message.SenderKind != ChatMessageSenderKind.System;
    }

    protected virtual string GetBubbleClass(ChatMessageDto message) => "chat-message-timeline__bubble";

    protected virtual string? GetBubbleBackground(ChatMessageDto message)
    {
        if (!UsePeerParticipantLayout || message.SenderKind == ChatMessageSenderKind.System)
        {
            return null;
        }

        return IsOwnMessage(message)
            ? "var(--sb-color-primary, #3b82f6)"
            : "var(--sb-color-surface, #ffffff)";
    }

    protected virtual bool IsSupportBubbleOutlined(ChatMessageDto message) =>
        !UsePeerParticipantLayout || (!IsOwnMessage(message) && message.SenderKind != ChatMessageSenderKind.System);

    protected static SbColor GetSenderChipColor(ChatMessageDto message) => message.SenderKind switch
    {
        ChatMessageSenderKind.Visitor => SbColor.Warning,
        ChatMessageSenderKind.Operator => SbColor.Info,
        ChatMessageSenderKind.Assistant => SbColor.Primary,
        ChatMessageSenderKind.System => SbColor.Muted,
        _ => SbColor.Default
    };

    protected virtual bool IsOwnMessage(ChatMessageDto message)
    {
        var currentUserId = AuthenticatedUserId ?? CurrentUser.Id;
        if (!currentUserId.HasValue)
        {
            return false;
        }

        if (message.SenderUserId.HasValue)
        {
            return message.SenderUserId.Value == currentUserId.Value;
        }

        return message.CreatorId.HasValue && message.CreatorId.Value == currentUserId.Value;
    }

    protected virtual string? ResolveSenderDisplayName(ChatMessageDto message)
    {
        if (!message.SenderUserId.HasValue || Session == null)
        {
            return null;
        }

        var participant = Session.Participants.FirstOrDefault(item =>
            item.UserId == message.SenderUserId && item.LeftAt == null);

        return participant?.DisplayName;
    }

    protected virtual ChatMessageMetadataModel? GetMessageMetadata(ChatMessageDto message)
    {
        return ChatMessageMetadata.TryParse(message.MetadataJson);
    }

    protected virtual string? GetLocationUrl(ChatMessageLocationMetadata location)
    {
        return ChatMessageMetadata.GetOpenStreetMapUrl(location);
    }

    protected virtual string GetAttachmentStreamUrl(Guid fileId)
    {
        return FileItemUrlProvider.GetStreamUrl(fileId);
    }
}
