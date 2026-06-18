using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

[Area(AIManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai-management/ai")]
public class AIController : AIManagementController, IAIAppService
{
    private readonly IAIAppService _aiAppService;

    public AIController(IAIAppService aiAppService)
    {
        _aiAppService = aiAppService;
    }

    [HttpPost("audio/transcriptions")]
    public virtual Task<AudioTranscriptionDto> TranscribeAudioAsync(TranscribeAudioInput input)
    {
        return _aiAppService.TranscribeAudioAsync(input);
    }

    [HttpPost("audio/speech")]
    public virtual Task<TextToSpeechDto> GenerateSpeechAsync(GenerateSpeechInput input)
    {
        return _aiAppService.GenerateSpeechAsync(input);
    }

    [HttpPost("vision/analyze")]
    public virtual Task<VisionAnalysisDto> AnalyzeImageAsync(AnalyzeImageInput input)
    {
        return _aiAppService.AnalyzeImageAsync(input);
    }

    [HttpPost("embeddings")]
    public virtual Task<EmbeddingsDto> GenerateEmbeddingsAsync(GenerateEmbeddingsInput input)
    {
        return _aiAppService.GenerateEmbeddingsAsync(input);
    }

    [HttpGet("workspaces/{workspaceName}/capabilities/{capabilityType}")]
    public virtual Task<bool> HasCapabilityAsync(string workspaceName, AICapabilityType capabilityType)
    {
        return _aiAppService.HasCapabilityAsync(workspaceName, capabilityType);
    }

    [HttpGet("workspaces/{workspaceId}/model-configurations")]
    public virtual Task<List<AIModelConfigurationDto>> GetModelConfigurationsAsync(Guid workspaceId)
    {
        return _aiAppService.GetModelConfigurationsAsync(workspaceId);
    }

    [HttpPost("model-configurations")]
    public virtual Task<AIModelConfigurationDto> CreateModelConfigurationAsync(CreateAIModelConfigurationDto input)
    {
        return _aiAppService.CreateModelConfigurationAsync(input);
    }

    [HttpPut("model-configurations/{id}")]
    public virtual Task<AIModelConfigurationDto> UpdateModelConfigurationAsync(Guid id, UpdateAIModelConfigurationDto input)
    {
        return _aiAppService.UpdateModelConfigurationAsync(id, input);
    }

    [HttpDelete("model-configurations/{id}")]
    public virtual Task DeleteModelConfigurationAsync(Guid id)
    {
        return _aiAppService.DeleteModelConfigurationAsync(id);
    }

    [HttpGet("workspaces/{workspaceId}/usage-logs")]
    public virtual Task<List<AIUsageLogDto>> GetUsageLogsAsync(
        Guid workspaceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return _aiAppService.GetUsageLogsAsync(workspaceId, startDate, endDate);
    }

    [HttpGet("workspaces/{workspaceId}/usage-statistics")]
    public virtual Task<UsageStatisticsDto> GetUsageStatisticsAsync(
        Guid workspaceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return _aiAppService.GetUsageStatisticsAsync(workspaceId, startDate, endDate);
    }
}
