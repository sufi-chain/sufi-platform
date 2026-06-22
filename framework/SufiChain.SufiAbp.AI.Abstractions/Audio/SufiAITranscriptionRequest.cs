using System;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Request to transcribe audio to text against a named AI workspace.
/// </summary>
public class SufiAITranscriptionRequest
{
    /// <summary>
    /// Name of the AI workspace to execute against.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Raw audio bytes.
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Audio container/format (e.g. <c>mp3</c>, <c>wav</c>, <c>m4a</c>).
    /// </summary>
    public string AudioFormat { get; set; } = "mp3";

    /// <summary>
    /// Optional ISO language hint.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional prompt to guide the transcription.
    /// </summary>
    public string? Prompt { get; set; }
}
