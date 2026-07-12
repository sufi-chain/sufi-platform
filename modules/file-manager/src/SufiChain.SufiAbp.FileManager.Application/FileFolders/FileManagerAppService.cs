using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.FileManager.Features;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.Users;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Application service for file manager clipboard and bulk operations
/// </summary>
[RequiresFeature(SufiAbpFileManagerFeatures.Enable, SufiAbpFileManagerFeatures.FileItems)]
public class FileManagerAppService : SufiAbpApplicationService, IFileManagerAppService
{
    private readonly IFileFolderRepository _folderRepository;
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IDistributedCache<ClipboardStateDto> _clipboardCache;
    private readonly IStructureBlobContainerProvider _structureBlobContainerProvider;

    public FileManagerAppService(
        IFileFolderRepository folderRepository,
        IFileItemRepository fileItemRepository,
        IDistributedCache<ClipboardStateDto> clipboardCache,
        IStructureBlobContainerProvider structureBlobContainerProvider)
    {
        _folderRepository = folderRepository;
        _fileItemRepository = fileItemRepository;
        _clipboardCache = clipboardCache;
        _structureBlobContainerProvider = structureBlobContainerProvider;
    }

    private string GetClipboardCacheKey() => $"FileManager:Clipboard:{CurrentUser.Id}";

    #region Clipboard Operations

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<ClipboardResultDto> CutAsync(ClipboardOperationInput input)
    {
        var state = new ClipboardStateDto
        {
            HasContent = true,
            Operation = ClipboardOperation.Cut,
            FileIds = input.FileIds,
            FolderIds = input.FolderIds,
            CreatedAt = Clock.Now
        };

        await _clipboardCache.SetAsync(
            GetClipboardCacheKey(),
            state,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

        return new ClipboardResultDto
        {
            Success = true,
            FileCount = input.FileIds.Count,
            FolderCount = input.FolderIds.Count,
            Message = $"Cut {input.FileIds.Count} files and {input.FolderIds.Count} folders"
        };
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<ClipboardResultDto> CopyAsync(ClipboardOperationInput input)
    {
        var state = new ClipboardStateDto
        {
            HasContent = true,
            Operation = ClipboardOperation.Copy,
            FileIds = input.FileIds,
            FolderIds = input.FolderIds,
            CreatedAt = Clock.Now
        };

        await _clipboardCache.SetAsync(
            GetClipboardCacheKey(),
            state,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

        return new ClipboardResultDto
        {
            Success = true,
            FileCount = input.FileIds.Count,
            FolderCount = input.FolderIds.Count,
            Message = $"Copied {input.FileIds.Count} files and {input.FolderIds.Count} folders"
        };
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<PasteResultDto> PasteAsync(PasteInput input)
    {
        var clipboardState = await _clipboardCache.GetAsync(GetClipboardCacheKey());

        if (clipboardState == null || !clipboardState.HasContent)
        {
            return new PasteResultDto
            {
                Success = false,
                Errors = new List<string> { "Clipboard is empty" }
            };
        }

        var result = new PasteResultDto { Success = true };

        // Determine target folder
        Guid? targetFolderId = input.TargetFolderId;
        if (!targetFolderId.HasValue && !string.IsNullOrEmpty(input.TargetPath))
        {
            var targetFolder = await _folderRepository.FindByPathAsync(input.TargetPath, CurrentTenant.Id);
            targetFolderId = targetFolder?.Id;
        }

        // Process files
        foreach (var fileId in clipboardState.FileIds)
        {
            try
            {
                if (clipboardState.Operation == ClipboardOperation.Cut)
                {
                    await MoveFileToFolderAsync(fileId, targetFolderId);
                    result.FilesMoved++;
                }
                else
                {
                    await CopyFileToFolderAsync(fileId, targetFolderId, input.ConflictResolution);
                    result.FilesCopied++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process file {fileId}: {ex.Message}");
            }
        }

        // Process folders
        foreach (var folderId in clipboardState.FolderIds)
        {
            try
            {
                if (clipboardState.Operation == ClipboardOperation.Cut)
                {
                    await MoveFolderToParentAsync(folderId, targetFolderId);
                    result.FoldersMoved++;
                }
                else
                {
                    await CopyFolderToParentAsync(folderId, targetFolderId);
                    result.FoldersCopied++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process folder {folderId}: {ex.Message}");
            }
        }

        // Clear clipboard after cut operation
        if (clipboardState.Operation == ClipboardOperation.Cut)
        {
            await ClearClipboardAsync();
        }

        return result;
    }

    public async Task<ClipboardStateDto> GetClipboardStateAsync()
    {
        var state = await _clipboardCache.GetAsync(GetClipboardCacheKey());
        return state ?? new ClipboardStateDto { HasContent = false, Operation = ClipboardOperation.None };
    }

    public async Task ClearClipboardAsync()
    {
        await _clipboardCache.RemoveAsync(GetClipboardCacheKey());
    }

    #endregion

    #region Bulk Operations

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<BulkOperationResultDto> MoveItemsAsync(BulkMoveInput input)
    {
        var result = new BulkOperationResultDto { Success = true };

        Guid? targetFolderId = input.TargetFolderId;
        if (!targetFolderId.HasValue && !string.IsNullOrEmpty(input.TargetPath))
        {
            var targetFolder = await _folderRepository.FindByPathAsync(input.TargetPath, CurrentTenant.Id);
            targetFolderId = targetFolder?.Id;
        }

        // Move files
        foreach (var fileId in input.FileIds)
        {
            try
            {
                await MoveFileToFolderAsync(fileId, targetFolderId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = fileId,
                    ItemName = "File",
                    ErrorMessage = ex.Message
                });
            }
        }

        // Move folders
        foreach (var folderId in input.FolderIds)
        {
            try
            {
                await MoveFolderToParentAsync(folderId, targetFolderId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = folderId,
                    ItemName = "Folder",
                    ErrorMessage = ex.Message
                });
            }
        }

        result.Success = result.FailedCount == 0;
        return result;
    }

    [Authorize(FileManagerPermissions.FileItems.Create)]
    public async Task<BulkOperationResultDto> CopyItemsAsync(BulkCopyInput input)
    {
        var result = new BulkOperationResultDto { Success = true };

        Guid? targetFolderId = input.TargetFolderId;
        if (!targetFolderId.HasValue && !string.IsNullOrEmpty(input.TargetPath))
        {
            var targetFolder = await _folderRepository.FindByPathAsync(input.TargetPath, CurrentTenant.Id);
            targetFolderId = targetFolder?.Id;
        }

        // Copy files
        foreach (var fileId in input.FileIds)
        {
            try
            {
                await CopyFileToFolderAsync(fileId, targetFolderId, ConflictResolution.Rename);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = fileId,
                    ItemName = "File",
                    ErrorMessage = ex.Message
                });
            }
        }

        // Copy folders
        foreach (var folderId in input.FolderIds)
        {
            try
            {
                await CopyFolderToParentAsync(folderId, targetFolderId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = folderId,
                    ItemName = "Folder",
                    ErrorMessage = ex.Message
                });
            }
        }

        result.Success = result.FailedCount == 0;
        return result;
    }

    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task<BulkOperationResultDto> DeleteItemsAsync(BulkDeleteInput input)
    {
        var result = new BulkOperationResultDto { Success = true };

        // Delete files
        foreach (var fileId in input.FileIds)
        {
            try
            {
                var fileItem = await _fileItemRepository.GetAsync(fileId);
                var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);

                // Delete from blob storage
                await blobContainer.DeleteAsync(fileItem.BlobName);
                if (!string.IsNullOrEmpty(fileItem.ThumbnailBlobName))
                {
                    await blobContainer.DeleteAsync(fileItem.ThumbnailBlobName);
                }

                await _fileItemRepository.DeleteAsync(fileId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = fileId,
                    ItemName = "File",
                    ErrorMessage = ex.Message
                });
            }
        }

        // Delete folders
        foreach (var folderId in input.FolderIds)
        {
            try
            {
                var folder = await _folderRepository.GetAsync(folderId);

                if (folder.Type != FolderType.Custom)
                {
                    throw new UserFriendlyException("Cannot delete system folders.");
                }

                // Delete files in folder
                await DeleteFilesInFolderRecursiveAsync(folderId);

                // Delete folder and descendants
                var descendants = await _folderRepository.GetDescendantsAsync(folderId);
                foreach (var descendant in descendants.OrderByDescending(d => d.Path.Length))
                {
                    await _folderRepository.DeleteAsync(descendant.Id);
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add(new BulkOperationErrorDto
                {
                    ItemId = folderId,
                    ItemName = "Folder",
                    ErrorMessage = ex.Message
                });
            }
        }

        result.Success = result.FailedCount == 0;
        return result;
    }

    public async Task<DownloadResultDto> DownloadAsZipAsync(DownloadInput input)
    {
        // TODO: Implement ZIP download functionality
        // This would require creating a ZIP file with the selected items
        // and providing a download URL

        await Task.CompletedTask;

        return new DownloadResultDto
        {
            Success = false,
            FileCount = input.FileIds.Count + input.FolderIds.Count
        };
    }

    #endregion

    #region Search

    public async Task<SearchResultDto> SearchAsync(SearchInput input)
    {
        var result = new SearchResultDto
        {
            Query = input.Query,
            Items = new List<SearchResultItemDto>()
        };

        // Search files
        if (input.SearchFileNames)
        {
            var query = await _fileItemRepository.GetQueryableAsync();
            query = query.Where(m => m.TenantId == CurrentTenant.Id);

            if (!string.IsNullOrEmpty(input.Query))
            {
                var searchTerm = input.Query.ToLower();
                query = query.Where(m => 
                    m.OriginalName.ToLower().Contains(searchTerm) ||
                    m.Name.ToLower().Contains(searchTerm) ||
                    (m.Alt != null && m.Alt.ToLower().Contains(searchTerm)));
            }

            // Apply scope filter
            if (input.ScopeFolderId.HasValue)
            {
                if (input.IncludeSubfolders)
                {
                    var descendants = await _folderRepository.GetDescendantsAsync(input.ScopeFolderId.Value);
                    var folderIds = descendants.Select(d => d.Id).ToList();
                    query = query.Where(m => m.FolderId != null && folderIds.Contains(m.FolderId.Value));
                }
                else
                {
                    query = query.Where(m => m.FolderId == input.ScopeFolderId.Value);
                }
            }

            var files = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount));

            foreach (var file in files)
            {
                result.Items.Add(new SearchResultItemDto
                {
                    Id = file.Id,
                    Name = file.OriginalName,
                    Path = file.BlobName,
                    IsFolder = false,
                    Icon = GetFileIcon(file.MimeType),
                    MimeType = file.MimeType,
                    Size = file.Size,
                    ModifiedDate = file.LastModificationTime ?? file.CreationTime,
                    ParentFolderId = file.FolderId,
                    HighlightedName = HighlightSearchTerm(file.OriginalName, input.Query)
                });
            }
        }

        // Search folders
        if (input.SearchFolderNames)
        {
            var folderQuery = await _folderRepository.GetQueryableAsync();
            folderQuery = folderQuery.Where(f => f.TenantId == CurrentTenant.Id);

            if (!string.IsNullOrEmpty(input.Query))
            {
                var searchTerm = input.Query.ToLower();
                folderQuery = folderQuery.Where(f => f.Name.ToLower().Contains(searchTerm));
            }

            // Apply scope filter
            if (input.ScopeFolderId.HasValue && input.IncludeSubfolders)
            {
                var scopeFolder = await _folderRepository.GetAsync(input.ScopeFolderId.Value);
                folderQuery = folderQuery.Where(f => f.Path.StartsWith(scopeFolder.Path));
            }

            var folders = await AsyncExecuter.ToListAsync(
                folderQuery.Take(input.MaxResultCount - result.Items.Count));

            foreach (var folder in folders)
            {
                result.Items.Add(new SearchResultItemDto
                {
                    Id = folder.Id,
                    Name = folder.Name,
                    Path = folder.Path,
                    IsFolder = true,
                    Icon = folder.Icon ?? "folder",
                    ModifiedDate = folder.LastModificationTime ?? folder.CreationTime,
                    ParentFolderId = folder.ParentId,
                    HighlightedName = HighlightSearchTerm(folder.Name, input.Query)
                });
            }
        }

        result.TotalCount = result.Items.Count;
        return result;
    }

    #endregion

    #region Private Helpers

    private async Task MoveFileToFolderAsync(Guid fileId, Guid? targetFolderId)
    {
        var file = await _fileItemRepository.GetAsync(fileId);
        file.FolderId = targetFolderId;
        await _fileItemRepository.UpdateAsync(file, autoSave: true);
    }

    private async Task CopyFileToFolderAsync(Guid fileId, Guid? targetFolderId, ConflictResolution conflictResolution)
    {
        var sourceFile = await _fileItemRepository.GetAsync(fileId);
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(sourceFile.StructureKey);

        // Copy blob data
        byte[] blobData;
        await using (var stream = await blobContainer.GetOrNullAsync(sourceFile.BlobName))
        {
            if (stream == null)
                throw new Volo.Abp.UserFriendlyException("Source file blob not found");
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            blobData = ms.ToArray();
        }

        var newBlobName = GenerateNewBlobName(sourceFile.BlobName);
        await blobContainer.SaveAsync(newBlobName, new MemoryStream(blobData), overrideExisting: true);

        // Copy thumbnail if exists
        string? newThumbnailBlobName = null;
        if (!string.IsNullOrEmpty(sourceFile.ThumbnailBlobName))
        {
            await using (var thumbStream = await blobContainer.GetOrNullAsync(sourceFile.ThumbnailBlobName))
            {
                if (thumbStream != null)
                {
                    using var thumbMs = new MemoryStream();
                    await thumbStream.CopyToAsync(thumbMs);
                    var thumbnailData = thumbMs.ToArray();
                    newThumbnailBlobName = GenerateNewBlobName(sourceFile.ThumbnailBlobName);
                    await blobContainer.SaveAsync(newThumbnailBlobName, new MemoryStream(thumbnailData), overrideExisting: true);
                }
            }
        }

        // Create new file item
        var newFile = new FileItem(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            sourceFile.Name,
            sourceFile.OriginalName,
            newBlobName,
            sourceFile.MimeType,
            sourceFile.Size,
            sourceFile.FileType,
            sourceFile.StructureKey)
        {
            FolderId = targetFolderId,
            Width = sourceFile.Width,
            Height = sourceFile.Height,
            Duration = sourceFile.Duration,
            ThumbnailBlobName = newThumbnailBlobName,
            Alt = sourceFile.Alt,
            Tags = new List<string>(sourceFile.Tags),
            IsProcessed = sourceFile.IsProcessed,
            IsTemp = false
        };

        await _fileItemRepository.InsertAsync(newFile, autoSave: true);
    }

    private async Task MoveFolderToParentAsync(Guid folderId, Guid? targetParentId)
    {
        var folder = await _folderRepository.GetAsync(folderId);

        if (folder.Type != FolderType.Custom)
        {
            throw new UserFriendlyException("Cannot move system folders.");
        }

        // Check for circular reference
        if (targetParentId.HasValue)
        {
            if (targetParentId.Value == folderId)
            {
                throw new UserFriendlyException("Cannot move a folder into itself.");
            }

            var descendants = await _folderRepository.GetDescendantsAsync(folderId);
            if (descendants.Any(d => d.Id == targetParentId.Value))
            {
                throw new UserFriendlyException("Cannot move a folder into one of its descendants.");
            }
        }

        string newPath;
        if (targetParentId.HasValue)
        {
            var newParent = await _folderRepository.GetAsync(targetParentId.Value);
            newPath = $"{newParent.Path}/{SanitizeFolderName(folder.Name)}";
        }
        else
        {
            newPath = $"/{SanitizeFolderName(folder.Name)}";
        }

        var oldPath = folder.Path;
        folder.MoveTo(targetParentId, newPath);

        // Update paths of all descendants
        var descendants2 = await _folderRepository.GetDescendantsAsync(folderId);
        foreach (var descendant in descendants2.Where(d => d.Id != folderId))
        {
            var updatedPath = newPath + descendant.Path.Substring(oldPath.Length);
            descendant.Path = updatedPath;
            await _folderRepository.UpdateAsync(descendant);
        }

        await _folderRepository.UpdateAsync(folder, autoSave: true);
    }

    private async Task CopyFolderToParentAsync(Guid folderId, Guid? targetParentId)
    {
        var sourceFolder = await _folderRepository.GetAsync(folderId);

        string targetPath;
        if (targetParentId.HasValue)
        {
            var targetParent = await _folderRepository.GetAsync(targetParentId.Value);
            targetPath = $"{targetParent.Path}/{SanitizeFolderName(sourceFolder.Name)}";
        }
        else
        {
            targetPath = $"/{SanitizeFolderName(sourceFolder.Name)}";
        }

        // Handle naming conflict
        var copyName = sourceFolder.Name;
        var copyIndex = 1;
        while (await _folderRepository.PathExistsAsync(targetPath, CurrentTenant.Id))
        {
            copyName = $"{sourceFolder.Name} ({copyIndex++})";
            targetPath = targetParentId.HasValue
                ? $"{(await _folderRepository.GetAsync(targetParentId.Value)).Path}/{SanitizeFolderName(copyName)}"
                : $"/{SanitizeFolderName(copyName)}";
        }

        var newFolder = new FileFolder(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            copyName,
            targetPath,
            FolderType.Custom,
            targetParentId);

        newFolder.SetDisplayProperties(sourceFolder.Icon, sourceFolder.Color, sourceFolder.Description);

        await _folderRepository.InsertAsync(newFolder, autoSave: true);

        // Copy files in folder
        var query = await _fileItemRepository.GetQueryableAsync();
        var files = await AsyncExecuter.ToListAsync(query.Where(m => m.FolderId == folderId));

        foreach (var file in files)
        {
            await CopyFileToFolderAsync(file.Id, newFolder.Id, ConflictResolution.Rename);
        }

        // Recursively copy subfolders
        var subfolders = await _folderRepository.GetChildrenAsync(folderId);
        foreach (var subfolder in subfolders)
        {
            await CopyFolderToParentAsync(subfolder.Id, newFolder.Id);
        }
    }

    private async Task DeleteFilesInFolderRecursiveAsync(Guid folderId)
    {
        var descendants = await _folderRepository.GetDescendantsAsync(folderId);
        var folderIds = descendants.Select(d => d.Id).ToList();

        var query = await _fileItemRepository.GetQueryableAsync();
        var files = await AsyncExecuter.ToListAsync(
            query.Where(m => m.FolderId != null && folderIds.Contains(m.FolderId.Value)));

        foreach (var file in files)
        {
            var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(file.StructureKey);
            await blobContainer.DeleteAsync(file.BlobName);
            if (!string.IsNullOrEmpty(file.ThumbnailBlobName))
            {
                await blobContainer.DeleteAsync(file.ThumbnailBlobName);
            }
            await _fileItemRepository.DeleteAsync(file.Id);
        }
    }

    private static string GenerateNewBlobName(string originalBlobName)
    {
        var extension = System.IO.Path.GetExtension(originalBlobName);
        var directory = System.IO.Path.GetDirectoryName(originalBlobName) ?? "";
        var newFileName = $"{Guid.NewGuid()}{extension}";
        return string.IsNullOrEmpty(directory) 
            ? newFileName 
            : $"{directory}/{newFileName}";
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return sanitized.Trim().Replace(' ', '-').ToLowerInvariant();
    }

    private static string GetFileIcon(string mimeType)
    {
        if (mimeType.StartsWith("image/")) return "file-image";
        if (mimeType.StartsWith("video/")) return "file-video";
        if (mimeType.StartsWith("audio/")) return "file-audio";
        if (mimeType.Contains("pdf")) return "file-pdf";
        if (mimeType.Contains("word") || mimeType.Contains("msword") || mimeType.Contains("opendocument.text")) return "file-doc";
        if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet") || mimeType.Contains("opendocument.spreadsheet")) return "file-excel";
        if (mimeType.Contains("powerpoint") || mimeType.Contains("presentation") || mimeType.Contains("opendocument.presentation")) return "file-ppt";
        if (mimeType.Contains("csv") || mimeType.Contains("text/csv")) return "file-csv";
        if (mimeType.Contains("json")) return "file-json";
        if (mimeType.Contains("xml")) return "file-xml";
        if (mimeType.Contains("text/plain")) return "file-text";
        if (mimeType.Contains("zip") || mimeType.Contains("archive") || mimeType.Contains("x-rar") || mimeType.Contains("x-7z")) return "file-archive";
        return "file";
    }

    private static string HighlightSearchTerm(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm)) return text;
        
        var index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return text;

        var before = text.Substring(0, index);
        var match = text.Substring(index, searchTerm.Length);
        var after = text.Substring(index + searchTerm.Length);

        return $"{before}<mark>{match}</mark>{after}";
    }

    #endregion
}
