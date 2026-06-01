using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Transcripts;

public class ChatTranscriptDto
{
    public ChatSessionDto Session { get; set; } = default!;

    public List<ChatMessageDto> Messages { get; set; } = new();

    public List<ConversationLinkDto> Links { get; set; } = new();

    public DateTime ExportedAt { get; set; }
}

public class ChatTranscriptExportOptions
{
    public bool IncludeInternalMessages { get; set; }

    public bool IncludeMetadata { get; set; }

    public bool IncludeLinks { get; set; } = true;

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}
