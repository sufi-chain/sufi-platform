using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Archiving methods for FileItemAppService
/// </summary>
public partial class FileItemAppService
{
    /// <summary>
    /// Archive a file item
    /// </summary>
    [RequiresFeature(SufiFileManagerFeatures.Archiving)]
    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task ArchiveAsync(Guid id, string? reason = null)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        
        if (fileItem.IsArchived)
        {
            throw new Volo.Abp.UserFriendlyException("File is already archived");
        }

        await _fileItemManager.ArchiveAsync(fileItem, reason);
    }

    /// <summary>
    /// Archive multiple files based on criteria (returns count of archived files)
    /// </summary>
    [RequiresFeature(SufiFileManagerFeatures.Archiving)]
    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task<int> ArchiveOldFilesAsync(ArchiveOldFilesInput input)
    {
        var cutoffDate = Clock.Now.AddDays(-input.OlderThanDays);
        
        var query = await _fileItemRepository.GetQueryableAsync();
        var filesToArchive = query
            .Where(f => !f.IsArchived)
            .Where(f => f.CreationTime < cutoffDate)
            .Where(f => !f.IsTemp);

        // Filter by directory path if specified
        if (!string.IsNullOrEmpty(input.DirectoryPath))
        {
            filesToArchive = filesToArchive.Where(f => f.BlobName.StartsWith(input.DirectoryPath));
        }

        // Filter by file structure if specified
        if (!string.IsNullOrEmpty(input.StructureKey))
        {
            filesToArchive = filesToArchive.Where(f => f.StructureKey == input.StructureKey);
        }

        var files = filesToArchive.Take(input.MaxFiles).ToList();

        var archivedCount = 0;
        foreach (var file in files)
        {
            try
            {
                await _fileItemManager.ArchiveAsync(file, input.ArchiveReason ?? "Manual bulk archiving");
                archivedCount++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to archive file {FileId}: {FileName}", file.Id, file.Name);
            }
        }

        return archivedCount;
    }

    /// <summary>
    /// Restore a file from archive
    /// </summary>
    [RequiresFeature(SufiFileManagerFeatures.Archiving)]
    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task RestoreFromArchiveAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        
        if (!fileItem.IsArchived)
        {
            throw new Volo.Abp.UserFriendlyException("File is not archived");
        }

        await _fileItemManager.RestoreFromArchiveAsync(fileItem);
    }

    /// <summary>
    /// Get list of archived files
    /// </summary>
    [RequiresFeature(SufiFileManagerFeatures.Archiving)]
    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<PagedResultDto<FileItemDto>> GetArchivedFilesAsync(GetArchivedFilesInput input)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        
        // Filter archived files only
        query = query.Where(f => f.IsArchived);

        // Filter by file structure if specified
        if (!string.IsNullOrEmpty(input.StructureKey))
        {
            query = query.Where(f => f.StructureKey == input.StructureKey);
        }

        // Filter by directory path if specified
        if (!string.IsNullOrEmpty(input.DirectoryPath))
        {
            query = query.Where(f => f.BlobName.StartsWith(input.DirectoryPath));
        }

        // Filter by file name if specified
        if (!string.IsNullOrEmpty(input.FileName))
        {
            query = query.Where(f => f.Name.Contains(input.FileName) || f.OriginalName.Contains(input.FileName));
        }

        // Filter by archived date range
        if (input.ArchivedAfter.HasValue)
        {
            query = query.Where(f => f.ArchivedAt >= input.ArchivedAfter.Value);
        }

        if (input.ArchivedBefore.HasValue)
        {
            query = query.Where(f => f.ArchivedAt <= input.ArchivedBefore.Value);
        }

        // Get total count
        var totalCount = query.Count();

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            // Simple sorting implementation - can be enhanced
            query = input.Sorting.ToLower() switch
            {
                "name" => query.OrderBy(f => f.Name),
                "name desc" => query.OrderByDescending(f => f.Name),
                "archivedat" => query.OrderBy(f => f.ArchivedAt),
                "archivedat desc" => query.OrderByDescending(f => f.ArchivedAt),
                "size" => query.OrderBy(f => f.Size),
                "size desc" => query.OrderByDescending(f => f.Size),
                _ => query.OrderByDescending(f => f.ArchivedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(f => f.ArchivedAt);
        }

        // Apply paging
        var files = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        // Map to DTOs
        var dtos = files.Select(f => ObjectMapper.Map<FileItem, FileItemDto>(f)).ToList();

        return new PagedResultDto<FileItemDto>(totalCount, dtos);
    }
}