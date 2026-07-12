using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.FileManager.BackgroundJobs;

/// <summary>
/// Background job for archiving old files based on retention policy
/// </summary>
public class FileArchivingBackgroundJob : AsyncBackgroundJob<FileArchivingArgs>, ITransientDependency
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly FileItemManager _fileItemManager;
    private readonly ILogger<FileArchivingBackgroundJob> _logger;

    public FileArchivingBackgroundJob(
        IFileItemRepository fileItemRepository,
        FileItemManager fileItemManager,
        ILogger<FileArchivingBackgroundJob> logger)
    {
        _fileItemRepository = fileItemRepository;
        _fileItemManager = fileItemManager;
        _logger = logger;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(FileArchivingArgs args)
    {
        _logger.LogInformation(
            "Starting file archiving job for directory: {DirectoryPath}, older than {Days} days",
            args.DirectoryPath,
            args.OlderThanDays);

        var cutoffDate = DateTime.UtcNow.AddDays(-args.OlderThanDays);
        
        var query = await _fileItemRepository.GetQueryableAsync();
        var filesToArchive = query
            .Where(f => !f.IsArchived)
            .Where(f => f.CreationTime < cutoffDate)
            .Where(f => !f.IsTemp);

        // Filter by directory path if specified
        if (!string.IsNullOrEmpty(args.DirectoryPath))
        {
            filesToArchive = filesToArchive.Where(f => f.BlobName.StartsWith(args.DirectoryPath));
        }

        // Filter by file structure if specified
        if (!string.IsNullOrEmpty(args.StructureKey))
        {
            filesToArchive = filesToArchive.Where(f => f.StructureKey == args.StructureKey);
        }

        var files = filesToArchive.Take(args.BatchSize).ToList();

        _logger.LogInformation("Found {Count} files to archive", files.Count);

        var archivedCount = 0;
        foreach (var file in files)
        {
            try
            {
                await _fileItemManager.ArchiveAsync(file, args.ArchiveReason ?? "Automatic archiving - retention policy");
                archivedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to archive file {FileId}: {FileName}", file.Id, file.Name);
            }
        }

        _logger.LogInformation(
            "File archiving job completed. Archived {ArchivedCount} out of {TotalCount} files",
            archivedCount,
            files.Count);
    }
}

/// <summary>
/// Arguments for file archiving background job
/// </summary>
[Serializable]
public class FileArchivingArgs
{
    /// <summary>
    /// Directory path to archive files from (optional, null = all directories)
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Archive files older than this many days
    /// </summary>
    public int OlderThanDays { get; set; } = 90;

    /// <summary>
    /// File structure key filter (optional, e.g., "General")
    /// </summary>
    public string? StructureKey { get; set; }

    /// <summary>
    /// Reason for archiving
    /// </summary>
    public string? ArchiveReason { get; set; }

    /// <summary>
    /// Batch size for processing
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
