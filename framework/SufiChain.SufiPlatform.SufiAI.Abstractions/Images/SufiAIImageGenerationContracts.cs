using System;

namespace SufiChain.SufiPlatform.SufiAI;

[Serializable]
public class SufiAIImageGenerationRequest
{
    public string WorkspaceName { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    /// <summary>Provider size token such as <c>1024x1024</c>, <c>1536x1024</c> or <c>1024x1536</c>.</summary>
    public string Size { get; set; } = "1024x1024";

    public string? Quality { get; set; }

    /// <summary><c>png</c>, <c>jpeg</c> or <c>webp</c>; providers may ignore unsupported formats.</summary>
    public string OutputFormat { get; set; } = "png";
}

[Serializable]
public class SufiAIImageGenerationResponse
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    public string MimeType { get; set; } = "image/png";

    public string? RevisedPrompt { get; set; }

    public string ModelId { get; set; } = string.Empty;
}
