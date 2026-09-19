using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Request for a chat completion against a named AI workspace.
/// </summary>
public class SufiAIChatRequest
{
    /// <summary>
    /// Name of the AI workspace to execute against.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Optional selectable model route inside the named workspace.
    /// Null keeps the workspace default chat route.
    /// </summary>
    public Guid? ModelConfigurationId { get; set; }

    /// <summary>
    /// Conversation messages, oldest first. The last message is typically the
    /// current user message.
    /// </summary>
    public List<SufiAIChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Optional system prompt prepended to the conversation.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Optional sampling temperature.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>Optional strict output schema. Requires a provider route supporting structured outputs.</summary>
    public SufiAIJsonResponseSchema? ResponseSchema { get; set; }
}
