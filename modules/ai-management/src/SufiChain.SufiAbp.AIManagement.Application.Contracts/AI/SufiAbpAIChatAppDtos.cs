using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AIManagement.AI;

public class SufiAbpAIChatMessageDto
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

public class SufiAbpAISendChatMessageInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public List<SufiAbpAIChatMessageDto> ConversationHistory { get; set; } = new();

    public float? Temperature { get; set; }

    public int? MaxTokens { get; set; }
}

public class SufiAbpAIChatResponseDto
{
    public string Message { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int? TokensUsed { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }
}
