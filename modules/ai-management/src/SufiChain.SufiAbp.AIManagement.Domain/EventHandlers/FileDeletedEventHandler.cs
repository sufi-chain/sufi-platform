using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.FileManager.ETOs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiAbp.AIManagement.EventHandlers;

/// <summary>
/// Handles FileDeletedEto events from File-Manager module
/// Clears file information from AIUsageLog
/// </summary>
public class FileDeletedEventHandler : IDistributedEventHandler<FileDeletedEto>, ITransientDependency
{
    private readonly IAIUsageLogRepository _usageLogRepository;
    private readonly ILogger<FileDeletedEventHandler> _logger;

    public FileDeletedEventHandler(
        IAIUsageLogRepository usageLogRepository,
        ILogger<FileDeletedEventHandler> logger)
    {
        _usageLogRepository = usageLogRepository;
        _logger = logger;
    }

    public async Task HandleEventAsync(FileDeletedEto eventData)
    {
        // Only handle AIManagement file structures.
        if (eventData.StructureKey?.StartsWith(AIManagementFileStructureKeys.AIManagement, StringComparison.OrdinalIgnoreCase) != true)
        {
            return;
        }

        _logger.LogInformation(
            "Handling FileDeletedEto for AIManagement. FileId: {FileId}",
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
}
