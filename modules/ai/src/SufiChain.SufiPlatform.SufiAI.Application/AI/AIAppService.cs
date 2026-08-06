using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Security.Encryption;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.SufiAI.Storage;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.AI.Default)]
public class AIAppService : SufiApplicationService, IAIAppService
{
    private readonly IAIService _aiService;
    private readonly ISufiAIAudioService _aiAudioService;
    private readonly IAIModelConfigurationRepository _configurationRepository;
    private readonly IAIUsageLogRepository _usageLogRepository;
    private readonly IAIFileStorageService _fileStorageService;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceSyncService _workspaceSyncService;

    public AIAppService(
        IAIService aiService,
        ISufiAIAudioService aiAudioService,
        IAIModelConfigurationRepository configurationRepository,
        IAIUsageLogRepository usageLogRepository,
        IStringEncryptionService stringEncryptor,
        IAIFileStorageService fileStorageService,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService workspaceSyncService)
    {
        _aiService = aiService;
        _aiAudioService = aiAudioService;
        _configurationRepository = configurationRepository;
        _usageLogRepository = usageLogRepository;
        _stringEncryptor = stringEncryptor;
        _fileStorageService = fileStorageService;
        _workspaceRepository = workspaceRepository;
        _workspaceSyncService = workspaceSyncService;
    }

    [Authorize(AIPermissions.AI.Audio)]
    [RequiresFeature(SufiAIFeatures.Audio)]
    public async Task<AudioTranscriptionDto> TranscribeAudioAsync(TranscribeAudioInput input)
    {
        // Upload audio file to storage
        var storageResult = await _fileStorageService.UploadFileAsync(
            content: input.AudioData,
            fileName: $"audio-{DateTime.UtcNow:yyyyMMddHHmmss}.{input.AudioFormat}",
            mimeType: $"audio/{input.AudioFormat}",
            workspaceName: input.WorkspaceName,
            capability: "audio-transcription",
            sourceEntityId: null,
            metadata: new { Language = input.Language, Prompt = input.Prompt }
        );

        var request = new SufiAITranscriptionRequest
        {
            WorkspaceName = input.WorkspaceName,
            AudioData = input.AudioData,
            AudioFormat = input.AudioFormat,
            Language = input.Language,
            Prompt = input.Prompt
        };

        var response = await _aiAudioService.TranscribeAsync(request);

        return new AudioTranscriptionDto
        {
            Text = response.Text,
            Model = response.ModelId,
            Language = response.Language,
            FileId = storageResult.FileId,
            FileUrl = storageResult.FileUrl
        };
    }

    [Authorize(AIPermissions.AI.Audio)]
    [RequiresFeature(SufiAIFeatures.Audio)]
    public async Task<TextToSpeechDto> GenerateSpeechAsync(GenerateSpeechInput input)
    {
        var request = new TextToSpeechRequest
        {
            WorkspaceName = input.WorkspaceName,
            Text = input.Text,
            Voice = input.Voice,
            AudioFormat = input.AudioFormat,
            Speed = input.Speed
        };

        var response = await _aiService.GenerateSpeechAsync(request);

        // Upload generated audio to storage
        var storageResult = await _fileStorageService.UploadFileAsync(
            content: response.AudioData,
            fileName: $"tts-{DateTime.UtcNow:yyyyMMddHHmmss}.{response.AudioFormat}",
            mimeType: $"audio/{response.AudioFormat}",
            workspaceName: input.WorkspaceName,
            capability: "text-to-speech",
            sourceEntityId: null,
            metadata: new { Voice = input.Voice, Speed = input.Speed, TextLength = input.Text.Length }
        );

        return new TextToSpeechDto
        {
            AudioData = response.AudioData,
            AudioFormat = response.AudioFormat,
            Model = response.ModelId,
            FileId = storageResult.FileId,
            FileUrl = storageResult.FileUrl
        };
    }

    [Authorize(AIPermissions.AI.Vision)]
    [RequiresFeature(SufiAIFeatures.Vision)]
    public async Task<VisionAnalysisDto> AnalyzeImageAsync(AnalyzeImageInput input)
    {
        // Upload image to storage
        var storageResult = await _fileStorageService.UploadFileAsync(
            content: input.ImageData,
            fileName: $"image-{DateTime.UtcNow:yyyyMMddHHmmss}.{input.ImageFormat}",
            mimeType: $"image/{input.ImageFormat}",
            workspaceName: input.WorkspaceName,
            capability: "vision-analysis",
            sourceEntityId: null,
            metadata: new { Prompt = input.Prompt, MaxTokens = input.MaxTokens }
        );

        var request = new VisionAnalysisRequest
        {
            WorkspaceName = input.WorkspaceName,
            ImageData = input.ImageData,
            ImageFormat = input.ImageFormat,
            Prompt = input.Prompt,
            MaxTokens = input.MaxTokens
        };

        var response = await _aiService.AnalyzeImageAsync(request);

        return new VisionAnalysisDto
        {
            Description = response.Description,
            Model = response.ModelId,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
            FileId = storageResult.FileId,
            FileUrl = storageResult.FileUrl
        };
    }

    [Authorize(AIPermissions.AI.Embeddings)]
    [RequiresFeature(SufiAIFeatures.Embeddings)]
    public async Task<EmbeddingsDto> GenerateEmbeddingsAsync(GenerateEmbeddingsInput input)
    {
        var request = new EmbeddingsRequest
        {
            WorkspaceName = input.WorkspaceName,
            Text = input.Text
        };

        var response = await _aiService.GenerateEmbeddingsAsync(request);

        return new EmbeddingsDto
        {
            Embedding = response.Embedding,
            Model = response.ModelId,
            TotalTokens = response.TotalTokens
        };
    }

    public async Task<bool> HasCapabilityAsync(string workspaceName, AICapabilityType capabilityType)
    {
        return await _aiService.HasCapabilityAsync(workspaceName, capabilityType);
    }

    [Authorize(AIPermissions.AI.ManageConfigurations)]
    [RequiresFeature(SufiAIFeatures.Workspaces)]
    public async Task<List<AIModelConfigurationDto>> GetModelConfigurationsAsync(Guid workspaceId)
    {
        var configurations = await _configurationRepository.GetByWorkspaceIdAsync(workspaceId);
        return configurations.Select(c => AIModelConfigurationMapper.ToDto(c)).ToList();
    }

    [Authorize(AIPermissions.AI.ManageConfigurations)]
    [RequiresFeature(SufiAIFeatures.Workspaces)]
    public async Task<AIModelConfigurationDto> CreateModelConfigurationAsync(CreateAIModelConfigurationDto input)
    {
        var configuration = new AIModelConfiguration(
            GuidGenerator.Create(),
            input.WorkspaceId,
            input.CapabilityType,
            input.ModelId,
            input.Priority
        );

        configuration.UpdateConfiguration(
            input.ModelId,
            input.ApiEndpoint,
            EncryptApiKey(input.ApiKey),
            input.Priority,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens,
            input.Dimensions
        );

        await _configurationRepository.InsertAsync(configuration);
        await ClearWorkspaceRuntimeCacheAsync(input.WorkspaceId);

        return AIModelConfigurationMapper.ToDto(configuration);
    }

    [Authorize(AIPermissions.AI.ManageConfigurations)]
    [RequiresFeature(SufiAIFeatures.Workspaces)]
    public async Task<AIModelConfigurationDto> UpdateModelConfigurationAsync(Guid id, UpdateAIModelConfigurationDto input)
    {
        var configuration = await _configurationRepository.GetAsync(id);

        var apiKeyToUpdate = string.IsNullOrWhiteSpace(input.ApiKey)
            ? configuration.ApiKey
            : EncryptApiKey(input.ApiKey);

        configuration.UpdateConfiguration(
            input.ModelId,
            input.ApiEndpoint,
            apiKeyToUpdate,
            input.Priority,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens,
            input.Dimensions
        );

        await _configurationRepository.UpdateAsync(configuration);
        await ClearWorkspaceRuntimeCacheAsync(configuration.WorkspaceId);

        return AIModelConfigurationMapper.ToDto(configuration);
    }

    [Authorize(AIPermissions.AI.ManageConfigurations)]
    [RequiresFeature(SufiAIFeatures.Workspaces)]
    public async Task DeleteModelConfigurationAsync(Guid id)
    {
        var configuration = await _configurationRepository.GetAsync(id);
        var workspaceId = configuration.WorkspaceId;
        await _configurationRepository.DeleteAsync(id);
        await ClearWorkspaceRuntimeCacheAsync(workspaceId);
    }

    [Authorize(AIPermissions.AI.ViewUsage)]
    [RequiresFeature(SufiAIFeatures.UsageAnalytics)]
    public async Task<List<AIUsageLogDto>> GetUsageLogsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var logs = await _usageLogRepository.GetByWorkspaceAsync(workspaceId, startDate, endDate);
        return logs.Select(l => AIUsageLogMapper.ToDto(l)).ToList();
    }

    [Authorize(AIPermissions.AI.ViewUsage)]
    [RequiresFeature(SufiAIFeatures.UsageAnalytics)]
    public async Task<UsageStatisticsDto> GetUsageStatisticsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var logs = await _usageLogRepository.GetByWorkspaceAsync(workspaceId, startDate, endDate);
        var totalCost = await _usageLogRepository.GetTotalCostAsync(workspaceId, startDate, endDate);
        var totalTokens = await _usageLogRepository.GetTotalTokensAsync(workspaceId, startDate, endDate);

        return new UsageStatisticsDto
        {
            TotalCost = totalCost,
            TotalTokens = totalTokens,
            KnownTotalTokens = totalTokens,
            UsageUnavailableRequests = logs.Count(l => l.IsSuccess && !l.HasTokenUsage),
            TotalRequests = logs.Count,
            SuccessfulRequests = logs.Count(l => l.IsSuccess),
            FailedRequests = logs.Count(l => !l.IsSuccess),
            RequestsByCapability = logs
                .GroupBy(l => l.CapabilityType)
                .ToDictionary(g => g.Key, g => g.Count()),
            CostByModel = logs
                .Where(l => l.IsSuccess)
                .GroupBy(l => l.ModelId)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.EstimatedCost))
        };
    }
    
    private string? EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }
        
        return _stringEncryptor.Encrypt(apiKey);
    }

    private async Task ClearWorkspaceRuntimeCacheAsync(Guid workspaceId)
    {
        var workspace = await _workspaceRepository.FindAsync(workspaceId);
        if (workspace == null)
        {
            return;
        }

        _workspaceSyncService.ClearWorkspaceCache(workspace.Name);
    }
}

// Mapperly mappers
[Mapper]
public static partial class AIModelConfigurationMapper
{
    public static AIModelConfigurationDto ToDto(AIModelConfiguration entity)
    {
        return new AIModelConfigurationDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            CapabilityType = entity.CapabilityType,
            ModelId = entity.ModelId,
            ApiEndpoint = entity.ApiEndpoint,
            HasApiKey = !string.IsNullOrWhiteSpace(entity.ApiKey),
            IsEnabled = entity.IsEnabled,
            Priority = entity.Priority,
            OpenAIApiMode = entity.OpenAIApiMode,
            InputCostPer1MTokens = entity.InputCostPer1MTokens,
            OutputCostPer1MTokens = entity.OutputCostPer1MTokens,
            Dimensions = entity.Dimensions
        };
    }
}

[Mapper]
public static partial class AIUsageLogMapper
{
    public static AIUsageLogDto ToDto(AIUsageLog entity)
    {
        return new AIUsageLogDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            CapabilityType = entity.CapabilityType,
            ModelId = entity.ModelId,
            Provider = entity.Provider,
            InputTokens = entity.InputTokens,
            OutputTokens = entity.OutputTokens,
            TotalTokens = entity.TotalTokens,
            HasTokenUsage = entity.HasTokenUsage,
            UsageUnavailableReason = entity.UsageUnavailableReason,
            EstimatedCost = entity.EstimatedCost,
            IsCostCalculated = entity.IsCostCalculated,
            CostCalculationNote = entity.CostCalculationNote,
            LatencyMs = entity.LatencyMs,
            IsSuccess = entity.IsSuccess,
            ErrorMessage = entity.ErrorMessage,
            CreationTime = entity.CreationTime
        };
    }
}
