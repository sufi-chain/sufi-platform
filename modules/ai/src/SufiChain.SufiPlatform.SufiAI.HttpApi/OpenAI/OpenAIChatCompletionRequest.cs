using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.OpenAI;

public class OpenAIChatCompletionRequest
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Required]
    public List<OpenAIChatMessage> Messages { get; set; } = new();

    public double? Temperature { get; set; }

    public int? MaxTokens { get; set; }

    public double? TopP { get; set; }

    public bool Stream { get; set; }
}

public class OpenAIChatMessage
{
    [Required]
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
