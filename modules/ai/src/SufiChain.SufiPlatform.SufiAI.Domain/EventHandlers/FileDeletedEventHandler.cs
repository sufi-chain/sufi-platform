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
/// Handles FileDeletedEto events from File-Manager module
/// Clears file information from AIUsageLog
/// </summary>
public class FileDeletedEventHandler : IDistributedEventHandler<FileDeletedEto>, ITransientDependency
{
    private readonly IAIUsageLogRepository _usageLogRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<FileDeletedEventHandler> _logger;

    public FileDeletedEventHandler(
        IAIUsageLogRepository usageLogRepository,
        IFeatureChecker featureChecker,
        ILogger<FileDeletedEventHandler> logger)
    {
        _usageLogRepository = usageLogRepository;
        _featureChecker = featureChecker;
        _logger = logger;
    }

    public async Task HandleEventAsync(FileDeletedEto eventData)
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
            "Handling FileDeletedEto for AI. FileId: {FileId}",
            eventData.Id);

        try
        {
            // Find usage logs that reference this file
            var usageLogs = await _usageLogRepository.GetListAsync(
                ul => ul.FileId == eventData.Id);

            foreach (var usageLog in usageLogs)
            {
                // Clear file information (file no longer exists)
                usageLog.SetFileInfo(Guid.Empty, null);
                await _usageLogRepository.UpdateAsync(usageLog);

                _logger.LogInformation(
                    "Cleared file information from AIUsageLog {UsageLogId} for deleted FileId {FileId}",
                    usageLog.Id,
                    eventData.Id);
            }

            if (usageLogs.Count == 0)
            {
                _logger.LogDebug(
                    "No AIUsageLogs found referencing deleted FileId: {FileId}",
                    eventData.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to handle file deletion. FileId: {FileId}",
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