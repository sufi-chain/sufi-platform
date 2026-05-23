using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.AIManagement.AI;

/// <summary>
/// Request for chat completion
/// </summary>
public class ChatCompletionRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public string? SystemPrompt { get; set; }
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public bool Stream { get; set; }
}

/// <summary>
/// Response from chat completion
/// </summary>
public class ChatCompletionResponse
{
    public string Content { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public bool IsUsageChunk { get; set; }
    public string? UsageUnavailableReason { get; set; }
    public string? FinishReason { get; set; }
}

/// <summary>
/// Chat message
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public List<MessageContent>? MultiModalContent { get; set; }
}

/// <summary>
/// Multi-modal message content (text, image, etc.)
/// </summary>
public class MessageContent
{
    public string Type { get; set; } = string.Empty; // "text", "image_url"
    public string? Text { get; set; }
    public ImageContent? ImageUrl { get; set; }
}

public class ImageContent
{
    public string Url { get; set; } = string.Empty;
    public string? Detail { get; set; } // "auto", "low", "high"
}

/// <summary>
/// Request for audio transcription
/// </summary>
public class AudioTranscriptionRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = "mp3"; // mp3, wav, m4a, etc.
    public string? Language { get; set; }
    public string? Prompt { get; set; }
}

/// <summary>
/// Response from audio transcription
/// </summary>
public class AudioTranscriptionResponse
{
    public string Text { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string? Language { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public string? UsageUnavailableReason { get; set; }
}

/// <summary>
/// Request for text-to-speech
/// </summary>
public class TextToSpeechRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Voice { get; set; } // "alloy", "echo", "fable", etc.
    public string? AudioFormat { get; set; } // "mp3", "opus", "aac", "flac"
    public float? Speed { get; set; } // 0.25 to 4.0
}

/// <summary>
/// Response from text-to-speech
/// </summary>
public class TextToSpeechResponse
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
}

/// <summary>
/// Request for vision analysis
/// </summary>
public class VisionAnalysisRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ImageFormat { get; set; } = "png"; // png, jpg, webp, etc.
    public string Prompt { get; set; } = string.Empty;
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Response from vision analysis
/// </summary>
public class VisionAnalysisResponse
{
    public string Description { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public string? UsageUnavailableReason { get; set; }
}

/// <summary>
/// Request for embeddings generation
/// </summary>
public class EmbeddingsRequest
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? EncodingFormat { get; set; } // "float", "base64"
}

/// <summary>
/// Response from embeddings generation
/// </summary>
public class EmbeddingsResponse
{
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string ModelId { get; set; } = string.Empty;
    public int? TotalTokens { get; set; }
    public string? UsageUnavailableReason { get; set; }
}
