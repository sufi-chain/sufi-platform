namespace SufiChain.Chat.Usage;

public class ChatUsagePolicy
{
    public AccessMode AccessMode { get; set; }

    public int MaxSessionsPerUserPerDay { get; set; }

    public int MaxMessagesPerSession { get; set; }

    public int MaxAttachmentsPerSession { get; set; }

    public long MaxAttachmentBytesPerSession { get; set; }

    public bool EnableAnonymousIpGuard { get; set; }

    public int MaxSessionsPerIpPerDay { get; set; }

    public int MaxMessagesPerIpPerDay { get; set; }

    public int MaxAiSessionsPerIpPerHour { get; set; }

    public int MaxMessagesBeforeSignupRequired { get; set; }

    public int MaxAiQuestionsBeforeSignupRequired { get; set; }

    public LimitExceededAction LimitExceededAction { get; set; } = LimitExceededAction.BlockSend;
}
