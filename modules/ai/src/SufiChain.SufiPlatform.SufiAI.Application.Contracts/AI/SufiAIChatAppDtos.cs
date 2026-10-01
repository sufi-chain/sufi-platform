using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI;

public class SufiAIChatMessageDto
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

public class SufiAIChatAttachmentInput
{
    public string Type { get; set; } = string.Empty;

    public string? DataUrl { get; set; }

    public string? FileName { get; set; }

    public string? MimeType { get; set; }
}

public class SufiAISendChatMessageInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;

    public Guid? ModelConfigurationId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public List<SufiAIChatMessageDto> ConversationHistory { get; set; } = new();

    public float? Temperature { get; set; }

    public int? MaxTokens { get; set; }

    public string? ReasoningEffort { get; set; }

    /// <summary>
    /// Admin chat choice. Null keeps the product default: Responses only when the selected model supports it.
    /// </summary>
    public OpenAIApiMode? OpenAIApiMode { get; set; }

    public List<SufiAIChatAttachmentInput> Attachments { get; set; } = new();

    public List<string> AllowedMcpToolNames { get; set; } = new();
}

public class SufiAIChatResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int? TokensUsed { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }
}
