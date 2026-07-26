using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI;

public class SufiAIChatMessageDto
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

public class SufiAISendChatMessageInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public List<SufiAIChatMessageDto> ConversationHistory { get; set; } = new();

    public float? Temperature { get; set; }

    public int? MaxTokens { get; set; }

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
