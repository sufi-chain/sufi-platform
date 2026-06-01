namespace SufiChain.Chat.Connectors.Email;

public class ChatInboundEmailMessage
{
    public string MessageId { get; set; } = string.Empty;

    public string? InReplyTo { get; set; }

    public string? References { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public string? FromName { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
}

public interface IChatInboundEmailClient
{
    Task<IReadOnlyList<ChatInboundEmailMessage>> FetchUnreadAsync(
        ChatEmailConnectorRuntimeSettings settings,
        CancellationToken cancellationToken = default);
}
