using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.ETOs;
using SufiChain.SufiAbp.FileManager.Features;
using SufiChain.SufiAbp.FileManager.FileTypes;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace SufiChain.SufiAbp.FileManager.FileItems;

/// <summary>
/// Domain service for managing file items and publishing distributed events
/// </summary>
public class FileItemManager : DomainService
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IFeatureChecker _featureChecker;

    public FileItemManager(
        IFileItemRepository fileItemRepository,
        IDistributedEventBus distributedEventBus,
        IClock clock,
        ICurrentUser currentUser,
        IFeatureChecker featureChecker)
    {
        _fileItemRepository = fileItemRepository;
        _distributedEventBus = distributedEventBus;
        _clock = clock;
        _currentUser = currentUser;
        _featureChecker = featureChecker;
    }

    /// <summary>
    /// Create a new file item and publish FileUploadedEto
    /// </summary>
    public async Task<FileItem> CreateAsync(
        string name,
        string originalName,
        string blobName,
        string mimeType,
        long size,
        FileType fileType,
        string? structureKey = null,
        Guid? sourceEntityId = null,
        string? customMetadata = null)
    {
        await CheckFileItemsFeatureAsync();

        var fileItem = new FileItem(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            name,
            originalName,
            blobName,
            mimeType,
            size,
            fileType,
            structureKey);

        if (sourceEntityId.HasValue)
        {
            fileItem.SetSourceEntity(sourceEntityId);
        }

        if (!string.IsNullOrEmpty(customMetadata))
        {
            fileItem.SetCustomMetadata(customMetadata);
        }

        await _fileItemRepository.InsertAsync(fileItem, autoSave: true);

        // Publish FileUploadedEto
        await PublishFileUploadedEventAsync(fileItem);

        return fileItem;
    }

    /// <summary>
    /// Delete a file item and publish FileDeletedEto
    /// </summary>
    public async Task DeleteAsync(FileItem fileItem)
    {
        await CheckFileItemsFeatureAsync();

        // Publish event before deletion
        await PublishFileDeletedEventAsync(fileItem);

        await _fileItemRepository.DeleteAsync(fileItem, autoSave: true);
    }

    /// <summary>
    /// Move/rename a file item and publish FileMovedEto
    /// </summary>
    public async Task MoveAsync(
        FileItem fileItem,
        string newName,
        string newBlobName,
        string newDirectoryPath)
    {
        await CheckFileItemsFeatureAsync();

        var oldName = fileItem.Name;
        var oldBlobName = fileItem.BlobName;
        var oldDirectoryPath = ExtractDirectoryPath(oldBlobName);

        // Update file item
        fileItem.Name = newName;
        fileItem.BlobName = newBlobName;

        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

        // Publish FileMovedEto
        await _distributedEventBus.PublishAsync(new FileMovedEto
        {
            Id = fileItem.Id,
            TenantId = fileItem.TenantId,
            OldFileName = oldName,
            NewFileName = newName,
            OldDirectoryPath = oldDirectoryPath,
            NewDirectoryPath = newDirectoryPath,
            OldBlobName = oldBlobName,
            NewBlobName = newBlobName,
            MovedBy = _currentUser.Id,
            MovedAt = _clock.Now
        });
    }

    /// <summary>
    /// Archive a file item and publish FileArchivedEto
    /// </summary>
    public async Task ArchiveAsync(FileItem fileItem, string? reason = null)
    {
        await CheckArchivingFeatureAsync();

        var originalDirectoryPath = ExtractDirectoryPath(fileItem.BlobName);
        
        fileItem.Archive(reason);
        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

        // Publish FileArchivedEto
        await _distributedEventBus.PublishAsync(new FileArchivedEto
        {
            Id = fileItem.Id,
            TenantId = fileItem.TenantId,
            FileName = fileItem.Name,
            OriginalDirectoryPath = originalDirectoryPath,
            ArchiveDirectoryPath = $"/archive{originalDirectoryPath}",
            BlobName = fileItem.BlobName,
            ArchivedBy = _currentUser.Id,
            ArchivedAt = _clock.Now,
            ArchiveReason = reason,
            StructureKey = fileItem.StructureKey
        });
    }

    /// <summary>
    /// Restore a file from archive
    /// </summary>
    public async Task RestoreFromArchiveAsync(FileItem fileItem)
    {
        await CheckArchivingFeatureAsync();

        fileItem.RestoreFromArchive();
        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);
    }

    /// <summary>
    /// Update file metadata and publish FileMetadataUpdatedEto
    /// </summary>
    public async Task UpdateMetadataAsync(FileItem fileItem, string? customMetadata)
    {
        await CheckFileItemsFeatureAsync();

        fileItem.SetCustomMetadata(customMetadata);
        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

        // Publish FileMetadataUpdatedEto
        await _distributedEventBus.PublishAsync(new FileMetadataUpdatedEto
        {
            Id = fileItem.Id,
            TenantId = fileItem.TenantId,
            FileName = fileItem.Name,
            UpdatedMetadata = ParseMetadata(customMetadata),
            UpdatedBy = _currentUser.Id,
            UpdatedAt = _clock.Now,
            StructureKey = fileItem.StructureKey,
            SourceEntityId = fileItem.SourceEntityId
        });
    }

    private async Task PublishFileUploadedEventAsync(FileItem fileItem)
    {
        await _distributedEventBus.PublishAsync(new FileUploadedEto
        {
            Id = fileItem.Id,
            TenantId = fileItem.TenantId,
            DirectoryPath = ExtractDirectoryPath(fileItem.BlobName),
            FileName = fileItem.Name,
            OriginalFileName = fileItem.OriginalName,
            MimeType = fileItem.MimeType,
            SizeInBytes = fileItem.Size,
            UploadedBy = _currentUser.Id,
            UploadedAt = _clock.Now,
            StructureKey = fileItem.StructureKey,
            SourceEntityId = fileItem.SourceEntityId,
            SourceEntityType = fileItem.EntityType,
            Metadata = ParseMetadata(fileItem.CustomMetadata),
            BlobName = fileItem.BlobName,
            FileType = fileItem.FileType.ToString()
        });
    }

    private async Task PublishFileDeletedEventAsync(FileItem fileItem)
    {
        await _distributedEventBus.PublishAsync(new FileDeletedEto
        {
            Id = fileItem.Id,
            TenantId = fileItem.TenantId,
            FileName = fileItem.Name,
            DirectoryPath = ExtractDirectoryPath(fileItem.BlobName),
            BlobName = fileItem.BlobName,
            DeletedBy = _currentUser.Id,
            DeletedAt = _clock.Now,
            StructureKey = fileItem.StructureKey,
            SourceEntityId = fileItem.SourceEntityId
        });
    }

    private async Task CheckEnableFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpFileManagerFeatures.Enable}");
        }
    }

    private async Task CheckFileItemsFeatureAsync()
    {
        await CheckEnableFeatureAsync();

        if (!await _featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.FileItems))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpFileManagerFeatures.FileItems}");
        }
    }

    private async Task CheckArchivingFeatureAsync()
    {
        await CheckEnableFeatureAsync();

        if (!await _featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.Archiving))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpFileManagerFeatures.Archiving}");
        }
    }

    private string ExtractDirectoryPath(string blobName)
    {
        var lastSlashIndex = blobName.LastIndexOf('/');
        return lastSlashIndex > 0 ? blobName.Substring(0, lastSlashIndex) : "/";
    }

    private System.Collections.Generic.Dictionary<string, string> ParseMetadata(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
        {
            return new System.Collections.Generic.Dictionary<string, string>();
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(metadata)
                ?? new System.Collections.Generic.Dictionary<string, string>();
        }
        catch
        {
            return new System.Collections.Generic.Dictionary<string, string>();
        }
    }
}
