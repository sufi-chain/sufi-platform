using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI;

// Audio Transcription DTOs
public class TranscribeAudioInput
{
    public string WorkspaceName { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = "mp3";
    public string? Language { get; set; }
    public string? Prompt { get; set; }
}

public class AudioTranscriptionDto
{
    public string Text { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Language { get; set; }
    public Guid? FileId { get; set; }
    public string? FileUrl { get; set; }
}

// Text-to-Speech DTOs
public class GenerateSpeechInput
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Voice { get; set; }
    public string? AudioFormat { get; set; }
    public float? Speed { get; set; }
}

public class TextToSpeechDto
{
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Guid? FileId { get; set; }
    public string? FileUrl { get; set; }
}

// Vision Analysis DTOs
public class AnalyzeImageInput
{
    public string WorkspaceName { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ImageFormat { get; set; } = "png";
    public string Prompt { get; set; } = string.Empty;
    public int? MaxTokens { get; set; }
}

public class VisionAnalysisDto
{
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public Guid? FileId { get; set; }
    public string? FileUrl { get; set; }
}

// Embeddings DTOs
public class GenerateEmbeddingsInput
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class EmbeddingsDto
{
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string Model { get; set; } = string.Empty;
    public int? TotalTokens { get; set; }
}

// Model Configuration DTOs
public class AIModelConfigurationDto : Application.Dtos.EntityDto<Guid>
{
    public Guid WorkspaceId { get; set; }
    public AICapabilityType CapabilityType { get; set; }
    public OpenAIApiMode? OpenAIApiMode { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public bool HasApiKey { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public decimal? InputCostPer1MTokens { get; set; }
    public decimal? OutputCostPer1MTokens { get; set; }
}

public class CreateAIModelConfigurationDto
{
    public Guid WorkspaceId { get; set; }
    public AICapabilityType CapabilityType { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public decimal? InputCostPer1MTokens { get; set; }
    public decimal? OutputCostPer1MTokens { get; set; }
    public int Priority { get; set; }
    public OpenAIApiMode? OpenAIApiMode { get; set; }

}

public class UpdateAIModelConfigurationDto
{
    public string ModelId { get; set; } = string.Empty;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public OpenAIApiMode? OpenAIApiMode { get; set; }
    public decimal? InputCostPer1MTokens { get; set; }
    public decimal? OutputCostPer1MTokens { get; set; }
    public int Priority { get; set; }
}

// Usage Log DTOs
public class AIUsageLogDto : Application.Dtos.EntityDto<Guid>
{
    public Guid WorkspaceId { get; set; }
    public AICapabilityType CapabilityType { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public AIProviderType Provider { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public bool HasTokenUsage { get; set; }
    public string? UsageUnavailableReason { get; set; }
    public decimal EstimatedCost { get; set; }
    public bool IsCostCalculated { get; set; }
    public string? CostCalculationNote { get; set; }
    public long LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
}

public class UsageStatisticsDto
{
    public decimal TotalCost { get; set; }
    public long TotalTokens { get; set; }
    public long KnownTotalTokens { get; set; }
    public int UsageUnavailableRequests { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public Dictionary<AICapabilityType, int> RequestsByCapability { get; set; } = new();
    public Dictionary<string, decimal> CostByModel { get; set; } = new();
}
