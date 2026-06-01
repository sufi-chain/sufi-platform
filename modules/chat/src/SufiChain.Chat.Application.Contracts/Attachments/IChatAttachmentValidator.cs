namespace SufiChain.Chat.Attachments;

/// <summary>
/// Validates chat message attachments against FileManager entity scope.
/// </summary>
public interface IChatAttachmentValidator
{
    /// <summary>
    /// Validates attachment ids belong to the session and returns aggregate size/count.
    /// </summary>
    Task<ChatAttachmentValidationResult> ValidateAsync(
        Guid sessionId,
        IReadOnlyList<Guid> attachmentFileIds,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of chat attachment validation.
/// </summary>
public class ChatAttachmentValidationResult
{
    public int Count { get; set; }

    public long TotalBytes { get; set; }
}
