using Volo.Abp.Application.Services;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Resolves which AIManagement workspace should power an assistant conversation.
/// Host integrations may route by session metadata, links, or access mode before falling back to the tenant default.
/// </summary>
public interface IChatAssistantWorkspaceResolver : IApplicationService
{
    Task<string?> ResolveWorkspaceNameAsync(ChatAssistantWorkspaceResolveContext context);
}

/// <summary>
/// Input for workspace resolution.
/// </summary>
public class ChatAssistantWorkspaceResolveContext
{
    public Guid? SessionId { get; set; }

    public string? SessionMetadataJson { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public string? LinkedEntityType { get; set; }

    public string? LinkedEntityId { get; set; }

    /// <summary>
    /// Optional explicit workspace override (for example Chat Bot <c>AiWorkspaceName</c>).
    /// </summary>
    public string? ExplicitWorkspaceName { get; set; }

    /// <summary>
    /// Optional tenant-defined assistant key from Chat assistant mappings.
    /// </summary>
    public string? AssistantKey { get; set; }
}
