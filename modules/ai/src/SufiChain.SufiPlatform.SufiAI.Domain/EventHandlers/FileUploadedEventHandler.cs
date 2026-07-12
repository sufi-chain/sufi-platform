using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.FileManager.ETOs;
using SufiChain.SufiPlatform.FileManager.Features;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI.EventHandlers;

/// <summary>
/// Handles FileUploadedEto events from File-Manager module
/// Updates AIUsageLog with file information
/// </summary>
public class FileUploadedEventHandler : IDistributedEventHandler<FileUploadedEto>, ITransientDependency
{
    private readonly IAIUsageLogRepository _usageLogRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<FileUploadedEventHandler> _logger;

    public FileUploadedEventHandler(
        IAIUsageLogRepository usageLogRepository,
        IFeatureChecker featureChecker,
        ILogger<FileUploadedEventHandler> logger)
    {
        _usageLogRepository = usageLogRepository;
        _featureChecker = featureChecker;
        _logger = logger;
    }

    public async Task HandleEventAsync(FileUploadedEto eventData)
    {
        if (!await IsFileManagerIntegrationEnabledAsync())
        {
            return;
        }

        // Only handle AI file structures.
        if (eventData.StructureKey?.StartsWith(AIFileStructureKeys.AI, StringComparison.OrdinalIgnoreCase) != true)
        {
            return;
        }

        _logger.LogInformation(
            "Handling FileUploadedEto for AI. FileId: {FileId}, SourceEntityId: {SourceEntityId}",
            eventData.Id,
            eventData.SourceEntityId);

        // If there's a source entity ID, try to find the corresponding usage log
        if (eventData.SourceEntityId.HasValue)
        {
            try
            {
                var usageLog = await _usageLogRepository.FindAsync(eventData.SourceEntityId.Value);
                
                if (usageLog != null)
                {
                    // Update usage log with file information
                    var fileUrl = $"/api/file-manager/file-items/{eventData.Id}/download";
                    usageLog.SetFileInfo(eventData.Id, fileUrl);
                    
                    await _usageLogRepository.UpdateAsync(usageLog);

                    _logger.LogInformation(
                        "Updated AIUsageLog {UsageLogId} with FileId {FileId}",
                        usageLog.Id,
                        eventData.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "AIUsageLog not found for SourceEntityId: {SourceEntityId}",
                        eventData.SourceEntityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update AIUsageLog with file information. SourceEntityId: {SourceEntityId}, FileId: {FileId}",
                    eventData.SourceEntityId,
                    eventData.Id);
            }
        }
        else
        {
            _logger.LogDebug(
                "FileUploadedEto has no SourceEntityId. FileId: {FileId}",
                eventData.Id);
        }
    }

    private async Task<bool> IsFileManagerIntegrationEnabledAsync()
    {
        return await _featureChecker.IsEnabledAsync(SufiAIFeatures.Enable) &&
               await _featureChecker.IsEnabledAsync(SufiAIFeatures.FileManagerIntegration) &&
               await _featureChecker.IsEnabledAsync(SufiFileManagerFeatures.Enable);
    }
}