using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Application service for multi-modal AI operations
/// </summary>
public interface IAIAppService : IApplicationService
{
    // Audio operations
    Task<AudioTranscriptionDto> TranscribeAudioAsync(TranscribeAudioInput input);
    Task<TextToSpeechDto> GenerateSpeechAsync(GenerateSpeechInput input);
    
    // Vision operations
    Task<VisionAnalysisDto> AnalyzeImageAsync(AnalyzeImageInput input);
    
    // Embeddings operations
    Task<EmbeddingsDto> GenerateEmbeddingsAsync(GenerateEmbeddingsInput input);
    
    // Capability check
    Task<bool> HasCapabilityAsync(string workspaceName, AICapabilityType capabilityType);
    
    // Model configuration management
    Task<List<AIModelConfigurationDto>> GetModelConfigurationsAsync(Guid workspaceId);
    Task<AIModelConfigurationDto> CreateModelConfigurationAsync(CreateAIModelConfigurationDto input);
    Task<AIModelConfigurationDto> UpdateModelConfigurationAsync(Guid id, UpdateAIModelConfigurationDto input);
    Task DeleteModelConfigurationAsync(Guid id);
    
    // Usage statistics
    Task<List<AIUsageLogDto>> GetUsageLogsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null);
    Task<UsageStatisticsDto> GetUsageStatisticsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null);
}
