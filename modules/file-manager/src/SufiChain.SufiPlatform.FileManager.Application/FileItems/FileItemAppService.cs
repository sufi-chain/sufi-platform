using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.FileManager.Processing;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using Volo.Abp;
using Volo.Abp.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.ObjectExtending;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

[RequiresFeature(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems)]
public partial class FileItemAppService : SufiApplicationService, IFileItemAppService
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IFileStructureRepository _fileStructureRepository;
    private readonly IStructureCache _structureCache;
    private readonly IFolderAppService _folderAppService;
    private readonly IFileFolderRepository _folderRepository;
    private readonly IFolderAccessResolver _folderAccessResolver;
    private readonly IUserFolderAccessContextProvider _folderAccessContextProvider;
    private readonly IStructureBlobContainerProvider _structureBlobContainerProvider;
    private readonly IImageProcessor _imageProcessor;
    private readonly IVideoProcessor _videoProcessor;
    private readonly IFileBlobNameCalculator _blobNameCalculator;
    private readonly FileManagerOptions _options;
    private readonly ISettingProvider _settingProvider;
    private readonly IFileManagerTenantPolicyProvider _tenantPolicyProvider;
    private readonly ILogger<FileItemAppService> _logger;
    private readonly IS3PublicBlobUrlProvider _s3PublicBlobUrlProvider;
    private readonly FileItemManager _fileItemManager;
    private readonly FileItemBlobAccessService _blobAccessService;

    public FileItemAppService(
        IFileItemRepository fileItemRepository,
        IFileStructureRepository fileStructureRepository,
        IStructureCache structureCache,
        IFolderAppService folderAppService,
        IFileFolderRepository folderRepository,
        IFolderAccessResolver folderAccessResolver,
        IUserFolderAccessContextProvider folderAccessContextProvider,
        IStructureBlobContainerProvider structureBlobContainerProvider,
        IImageProcessor imageProcessor,
        IVideoProcessor videoProcessor,
        IFileBlobNameCalculator blobNameCalculator,
        IOptions<FileManagerOptions> options,
        ISettingProvider settingProvider,
        IFileManagerTenantPolicyProvider tenantPolicyProvider,
        ILogger<FileItemAppService> logger,
        IS3PublicBlobUrlProvider s3PublicBlobUrlProvider,
        FileItemManager fileItemManager,
        FileItemBlobAccessService blobAccessService)
    {
        _fileItemRepository = fileItemRepository;
        _fileStructureRepository = fileStructureRepository;
        _structureCache = structureCache;
        _folderAppService = folderAppService;
        _folderRepository = folderRepository;
        _folderAccessResolver = folderAccessResolver;
        _folderAccessContextProvider = folderAccessContextProvider;
        _structureBlobContainerProvider = structureBlobContainerProvider;
        _imageProcessor = imageProcessor;
        _videoProcessor = videoProcessor;
        _blobNameCalculator = blobNameCalculator;
        _options = options.Value;
        _settingProvider = settingProvider;
        _tenantPolicyProvider = tenantPolicyProvider;
        _logger = logger;
        _s3PublicBlobUrlProvider = s3PublicBlobUrlProvider;
        _fileItemManager = fileItemManager;
        _blobAccessService = blobAccessService;
    }

    [RemoteService(false)]
    [Authorize]
    public async Task<UploadValidationResult> ValidateUploadAsync(string fileName, string mimeType, string? structureKey, long fileSize)
    {
        if (string.IsNullOrEmpty(structureKey))
            return new UploadValidationResult { IsValid = true };

        FileStructure structure;
        try
        {
            structure = await GetStructureByKeyAsync(structureKey);
        }
        catch (UserFriendlyException ex)
        {
            return new UploadValidationResult { IsValid = false, ErrorMessage = ex.Message };
        }

        var allowedMimeTypes = (structure.AllowedMimeTypes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
        if (allowedMimeTypes.Any() && !allowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            return new UploadValidationResult { IsValid = false, ErrorMessage = $"File type '{mimeType}' is not allowed" };

        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var allowedExtensions = (structure.AllowedExtensions ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
        if (allowedExtensions.Any() && !allowedExtensions.Contains(extension))
            return new UploadValidationResult { IsValid = false, ErrorMessage = $"File extension '.{extension}' is not allowed. Allowed types: {structure.AllowedExtensions}" };

        if (fileSize > structure.MaxFileSize)
            return new UploadValidationResult { IsValid = false, ErrorMessage = $"File size exceeds maximum allowed size of {structure.MaxFileSize / (1024 * 1024)}MB" };

        return new UploadValidationResult { IsValid = true };
    }

    private async Task<Guid?> ResolveFolderIdAsync(Guid? folderId, string? folderPath)
    {
        if (folderId.HasValue)
        {
            return folderId;
        }

        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            var folder = await _folderAppService.GetOrCreateFolderByPathAsync(folderPath.Trim());
            return folder?.Id;
        }

        return null;
    }

    /// <summary>
    /// Authorizes an upload based on its target:
    /// - Structure-scoped uploads (StructureKey set, no free folder) require only authentication;
    ///   the calling integration service (e.g. ticket/chat composer) is responsible for the
    ///   domain-level ownership check, and the structure itself enforces size/type/count limits.
    /// - Folder uploads (FolderId/FolderPath set) require an explicit folder-level Write grant
    ///   for the current user, resolved through the FolderPermission OU/Role/User model.
    /// - Admins (FileItems.Create) bypass the folder grant check.
    /// This keeps the broad file-manager permission off the end-user role while still allowing
    /// legitimate per-structure uploads (portal ticket/chat attachments).
    /// </summary>
    protected virtual async Task EnsureCanUploadAsync(Guid? folderId, string? structureKey)
    {
        if (await AuthorizationService.IsGrantedAsync(FileManagerPermissions.FileItems.Create))
        {
            return;
        }

        // Structure-scoped uploads (no free folder) are gated by the integration caller's
        // own ownership check and the structure's validation, not by a file-manager permission.
        if (!string.IsNullOrEmpty(structureKey) && !folderId.HasValue)
        {
            return;
        }

        // Folder uploads require an explicit per-folder Write grant.
        if (folderId.HasValue)
        {
            var folder = await _folderRepository.GetWithPermissionsAsync(folderId.Value);
            if (folder == null)
            {
                throw new AbpAuthorizationException("Target folder was not found.");
            }

            var context = await _folderAccessContextProvider.GetContextAsync();
            var canWrite = await _folderAccessResolver.HasPermissionAsync(
                folder, context, FolderPermissionLevel.Write);

            if (!canWrite)
            {
                throw new AbpAuthorizationException(
                    $"Given policy has not granted: {FileManagerPermissions.FileItems.Create}");
            }

            return;
        }

        // No structure and no folder: a free-form upload into the file manager root requires
        // the broad file-manager permission, which ordinary portal users do not hold.
        throw new AbpAuthorizationException(
            $"Given policy has not granted: {FileManagerPermissions.FileItems.Create}");
    }

    [RemoteService(false)]
    [Authorize]
    public async Task<FileItemDto> UploadAsync(UploadFileInput input)
    {
        var folderId = await ResolveFolderIdAsync(input.FolderId, input.FolderPath);
        await EnsureCanUploadAsync(folderId, input.StructureKey);

        // Validate against structure if provided
        FileStructure? structure = null;
        if (!string.IsNullOrEmpty(input.StructureKey))
        {
            structure = await GetStructureByKeyAsync(input.StructureKey);
            await ValidateUploadContentAsync(input.Content, input.FileName, input.MimeType, structure);
        }

        // Determine file type
        var fileType = DetermineFileType(input.MimeType);

        // Create file item entity
        var fileId = GuidGenerator.Create();
        var blobName = _blobNameCalculator.Calculate(
            fileId,
            input.FileName,
            !input.AutoConfirm,
            input.StructureKey);

        var fileItem = new FileItem(
            id: fileId,
            tenantId: CurrentTenant.Id,
            name: Path.GetFileNameWithoutExtension(input.FileName),
            originalName: input.FileName,
            blobName: blobName,
            mimeType: input.MimeType,
            size: input.Content.Length,
            fileType: fileType,
            structureKey: input.StructureKey)
        {
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            IsTemp = !input.AutoConfirm,
            Alt = input.Alt,
            FolderId = folderId
        };

        // Process based on file type
        byte[] dataToStore = input.Content;
        
        if (fileType == FileType.Image)
        {
            await ProcessImageAsync(fileItem, input.Content, structure);
            
            // Convert to WebP if enabled
            if (structure?.EnableWebPConversion == true)
            {
                _logger.LogInformation("WebP conversion enabled for structure: {StructureKey}", structure.Key);
                try
                {
                    var (webpData, webpMimeType, webpExtension) = await _imageProcessor.ConvertToWebPAsync(input.Content);
                    
                    // Validate the converted data is not empty
                    if (webpData == null || webpData.Length == 0)
                    {
                        _logger.LogWarning("WebP conversion returned empty data, keeping original format");
                    }
                    else
                    {
                        _logger.LogInformation("WebP conversion successful. Original: {OriginalSize} bytes, Converted: {ConvertedSize} bytes", 
                            input.Content.Length, webpData.Length);
                        dataToStore = webpData;
                        fileItem.MimeType = webpMimeType;
                        fileItem.BlobName = Path.ChangeExtension(fileItem.BlobName, webpExtension);
                        // Update OriginalName extension to match the converted format
                        fileItem.OriginalName = Path.ChangeExtension(fileItem.OriginalName, webpExtension);
                        // IMPORTANT: Update the size to reflect the converted file size
                        fileItem.Size = webpData.Length;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "WebP conversion failed, keeping original format");
                    // Keep original format if conversion fails
                }
            }
            else
            {
                _logger.LogDebug("WebP conversion not enabled for this upload (StructureKey: {StructureKey})", input.StructureKey);
            }
        }
        else if (fileType == FileType.Video)
        {
            await ProcessVideoAsync(fileItem, input.Content, structure);
        }

        // Validate data before saving
        if (dataToStore == null || dataToStore.Length == 0)
        {
            throw new UserFriendlyException("Failed to process file - no data to store");
        }

        // Save to blob storage first, track what we've saved for potential rollback
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(input.StructureKey);
        string? savedBlobName = null;
        try
        {
            await blobContainer.SaveAsync(fileItem.BlobName, dataToStore, overrideExisting: true);
            savedBlobName = fileItem.BlobName;

            fileItem.IsProcessed = true;

            // Insert to database
            await _fileItemRepository.InsertAsync(fileItem, autoSave: true);

            return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
        }
        catch (Exception ex)
        {
            // Rollback: Delete blob if it was saved but DB insert failed
            if (savedBlobName != null)
            {
                _logger.LogWarning(ex, "Upload failed after saving blob, rolling back: {BlobName}", savedBlobName);
                try
                {
                    await blobContainer.DeleteAsync(savedBlobName);
                    
                    // Also delete thumbnail if it exists
                    if (!string.IsNullOrEmpty(fileItem.ThumbnailBlobName))
                    {
                        await blobContainer.DeleteAsync(fileItem.ThumbnailBlobName);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback blob deletion: {BlobName}", savedBlobName);
                }
            }
            throw;
        }
    }

    [RemoteService(false)]
    [DisableValidation]
    [Authorize]
    public async Task<FileItemDto> UploadStreamAsync(UploadFileStreamInput input)
    {
        var folderId = await ResolveFolderIdAsync(input.FolderId, input.FolderPath);
        await EnsureCanUploadAsync(folderId, input.StructureKey);

        // Validate file size
        var maxFileSizeBytes = (long)_options.MaxUploadFileSizeMB * 1024 * 1024;
        if (input.ContentLength > maxFileSizeBytes)
        {
            throw new UserFriendlyException($"File size exceeds maximum allowed size of {_options.MaxUploadFileSizeMB}MB");
        }

        // Validate against structure if provided
        FileStructure? structure = null;
        if (!string.IsNullOrEmpty(input.StructureKey))
        {
            structure = await GetStructureByKeyAsync(input.StructureKey);
            
            // Validate file size against structure
            if (input.ContentLength > structure.MaxFileSize)
            {
                throw new UserFriendlyException($"File size exceeds maximum allowed size of {structure.MaxFileSize / (1024 * 1024)}MB");
            }
            
            // Validate mime type and extension (without needing content)
            ValidateMimeTypeAndExtension(input.FileName, input.MimeType, structure);
        }

        // Determine file type
        var fileType = DetermineFileType(input.MimeType);

        // Create file item entity
        var fileId = GuidGenerator.Create();
        var blobName = _blobNameCalculator.Calculate(
            fileId,
            input.FileName,
            !input.AutoConfirm,
            input.StructureKey);

        var fileItem = new FileItem(
            id: fileId,
            tenantId: CurrentTenant.Id,
            name: Path.GetFileNameWithoutExtension(input.FileName),
            originalName: input.FileName,
            blobName: blobName,
            mimeType: input.MimeType,
            size: input.ContentLength,
            fileType: fileType,
            structureKey: input.StructureKey)
        {
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            IsTemp = !input.AutoConfirm,
            Alt = input.Alt,
            FolderId = folderId
        };

        // Determine if we should process or just stream
        var maxInMemorySizeBytes = (long)_options.MaxInMemoryFileSizeMB * 1024 * 1024;
        var shouldProcess = !input.SkipProcessing && 
                           input.ContentLength <= maxInMemorySizeBytes &&
                           (fileType == FileType.Image || (fileType == FileType.Video && structure?.GenerateThumbnail == true));

        string? savedBlobName = null;
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(input.StructureKey);
        try
        {
            if (shouldProcess)
            {
                // Load into memory for processing (small files only)
                using var memoryStream = new MemoryStream();
                await input.ContentStream.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();

                // Process based on file type
                byte[] dataToStore = content;

                if (fileType == FileType.Image)
                {
                    await ProcessImageAsync(fileItem, content, structure);

                    // Convert to WebP if enabled
                    if (structure?.EnableWebPConversion == true)
                    {
                        try
                        {
                            var (webpData, webpMimeType, webpExtension) = await _imageProcessor.ConvertToWebPAsync(content);
                            if (webpData != null && webpData.Length > 0)
                            {
                                dataToStore = webpData;
                                fileItem.MimeType = webpMimeType;
                                fileItem.BlobName = Path.ChangeExtension(fileItem.BlobName, webpExtension);
                                fileItem.OriginalName = Path.ChangeExtension(fileItem.OriginalName, webpExtension);
                                fileItem.Size = webpData.Length;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "WebP conversion failed, keeping original format");
                        }
                    }
                }
                else if (fileType == FileType.Video)
                {
                    await ProcessVideoAsync(fileItem, content, structure);
                }

                // Save processed data to blob storage
                await blobContainer.SaveAsync(fileItem.BlobName, dataToStore, overrideExisting: true);
                savedBlobName = fileItem.BlobName;
            }
            else
            {
                // Stream directly to blob storage without loading into memory
                _logger.LogInformation("Streaming large file directly to blob storage: {FileName}, Size: {Size} bytes", 
                    input.FileName, input.ContentLength);

                await blobContainer.SaveAsync(fileItem.BlobName, input.ContentStream, overrideExisting: true);
                savedBlobName = fileItem.BlobName;
            }

            fileItem.IsProcessed = shouldProcess;

            // Insert to database
            await _fileItemRepository.InsertAsync(fileItem, autoSave: true);

            return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
        }
        catch (Exception ex)
        {
            // Rollback: Delete blob if it was saved
            if (savedBlobName != null)
            {
                _logger.LogWarning(ex, "Upload failed after saving blob, rolling back: {BlobName}", savedBlobName);
                try
                {
                    await blobContainer.DeleteAsync(savedBlobName);
                    
                    // Also delete thumbnail if it exists
                    if (!string.IsNullOrEmpty(fileItem.ThumbnailBlobName))
                    {
                        await blobContainer.DeleteAsync(fileItem.ThumbnailBlobName);
                    }
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback blob deletion: {BlobName}", savedBlobName);
                }
            }
            throw;
        }
    }

    [RemoteService(false)]
    [Authorize]
    public async Task<ListResultDto<FileItemDto>> UploadMultipleAsync(UploadMultipleFileInput input)
    {
        var results = new List<FileItemDto>();

        foreach (var file in input.Files)
        {
            var singleInput = new UploadFileInput
            {
                FileName = file.FileName,
                Content = file.Content,
                MimeType = file.MimeType,
                StructureKey = input.StructureKey,
                EntityType = input.EntityType,
                EntityId = input.EntityId,
                FolderId = input.FolderId,
                FolderPath = input.FolderPath,
                AutoConfirm = input.AutoConfirm,
                Alt = file.Alt
            };

            var result = await UploadAsync(singleInput);
            results.Add(result);
        }

        return new ListResultDto<FileItemDto>(results);
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<FileItemDto> GetAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        var dto = ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
        var entry = await _structureCache.GetAsync(fileItem.StructureKey);
        ApplyStructurePublicAccess(dto, entry);
        return dto;
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<PagedResultDto<FileItemDto>> GetListAsync(GetFileListInput input)
    {
        var query = await _fileItemRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Keyword))
        {
            query = query.Where(x => x.Name.Contains(input.Keyword) || x.OriginalName.Contains(input.Keyword));
        }

        if (input.FileType.HasValue)
        {
            query = query.Where(x => x.FileType == input.FileType.Value);
        }

        if (!string.IsNullOrEmpty(input.EntityType))
        {
            query = query.Where(x => x.EntityType == input.EntityType);
        }

        if (input.EntityId.HasValue)
        {
            query = query.Where(x => x.EntityId == input.EntityId);
        }

        if (!string.IsNullOrEmpty(input.StructureKey))
        {
            query = query.Where(x => x.StructureKey == input.StructureKey);
        }

        if (input.OnlyFromPublicStructures == true)
        {
            var publicKeys = await _structureCache.GetPublicStructureKeysAsync();
            query = query.Where(x => x.StructureKey != null && publicKeys.Contains(x.StructureKey));
        }

        if (input.IsTemp.HasValue)
        {
            query = query.Where(x => x.IsTemp == input.IsTemp.Value);
        }

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(query);

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            // Support format: "PropertyName DESC" or "PropertyName ASC" or just "PropertyName"
            var sortParts = input.Sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sortProperty = sortParts[0];
            var isDescending = sortParts.Length > 1 && sortParts[1].ToUpper() == "DESC";

            query = sortProperty.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "originalname" => isDescending ? query.OrderByDescending(x => x.OriginalName) : query.OrderBy(x => x.OriginalName),
                "size" => isDescending ? query.OrderByDescending(x => x.Size) : query.OrderBy(x => x.Size),
                "filetype" => isDescending ? query.OrderByDescending(x => x.FileType) : query.OrderBy(x => x.FileType),
                "creationtime" => isDescending ? query.OrderByDescending(x => x.CreationTime) : query.OrderBy(x => x.CreationTime),
                "lastmodificationtime" => isDescending ? query.OrderByDescending(x => x.LastModificationTime) : query.OrderBy(x => x.LastModificationTime),
                "width" => isDescending ? query.OrderByDescending(x => x.Width) : query.OrderBy(x => x.Width),
                "height" => isDescending ? query.OrderByDescending(x => x.Height) : query.OrderBy(x => x.Height),
                "duration" => isDescending ? query.OrderByDescending(x => x.Duration) : query.OrderBy(x => x.Duration),
                _ => query.OrderByDescending(x => x.CreationTime) // Default
            };
        }
        else
        {
            query = query.OrderByDescending(x => x.CreationTime);
        }

        // Apply paging
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        // Execute query
        var items = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<FileItem>, List<FileItemDto>>(items);
        await EnrichWithStructureIsPublicAccessAsync(dtos);
        return new PagedResultDto<FileItemDto>(totalCount, dtos);
    }

    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);

        // Delete from blob storage
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);
        await blobContainer.DeleteAsync(fileItem.BlobName);

        // Delete thumbnail if exists
        if (!string.IsNullOrEmpty(fileItem.ThumbnailBlobName))
        {
            await blobContainer.DeleteAsync(fileItem.ThumbnailBlobName);
        }

        // Delete from database
        await _fileItemRepository.DeleteAsync(id);
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<FileItemDto> UpdateMetadataAsync(Guid id, UpdateFileMetadataInput input)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);

        if (input.Name != null)
        {
            fileItem.Name = input.Name;
        }

        if (input.Alt != null)
        {
            fileItem.Alt = input.Alt;
        }

        if (input.Tags != null)
        {
            fileItem.Tags = input.Tags.ToList();
        }

        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

        return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
    }

    //[Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<StorageQuotaDto> GetStorageQuotaAsync()
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        
        // Filter by tenant
        if (CurrentTenant.Id.HasValue)
        {
            query = query.Where(x => x.TenantId == CurrentTenant.Id);
        }

        var usedBytes = await AsyncExecuter.SumAsync(query, x => (long?)x.Size) ?? 0;

        var policy = await _tenantPolicyProvider.GetGeneralPolicyAsync();
        var limitMB = policy.StorageQuotaMB;
        if (limitMB == 0)
        {
            limitMB = 1024;
        }

        return new StorageQuotaDto
        {
            UsedBytes = usedBytes,
            UsedMB = usedBytes / (1024.0 * 1024.0),
            LimitMB = limitMB,
            AvailableMB = limitMB - (usedBytes / (1024.0 * 1024.0)),
            PercentageUsed = limitMB > 0 ? (usedBytes / (1024.0 * 1024.0)) / limitMB * 100 : 0
        };
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<FileStatisticsDto> GetStatisticsAsync()
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        // Exclude temp files (file explorer uses AutoConfirm=true so uploads are confirmed)
        query = query.Where(x => !x.IsTemp);

        // Get counts and size using server-side aggregation
        var totalCount = await AsyncExecuter.CountAsync(query);
        var imageCount = await AsyncExecuter.CountAsync(query.Where(x => x.FileType == FileType.Image));
        var videoCount = await AsyncExecuter.CountAsync(query.Where(x => x.FileType == FileType.Video));
        var documentCount = await AsyncExecuter.CountAsync(query.Where(x => x.FileType == FileType.Document));
        var audioCount = await AsyncExecuter.CountAsync(query.Where(x => x.FileType == FileType.Audio));
        var totalSize = await AsyncExecuter.SumAsync(query, x => (long?)x.Size) ?? 0;

        return new FileStatisticsDto
        {
            TotalCount = totalCount,
            ImageCount = imageCount,
            VideoCount = videoCount,
            DocumentCount = documentCount,
            AudioCount = audioCount,
            OtherCount = totalCount - imageCount - videoCount - documentCount - audioCount,
            TotalSize = totalSize
        };
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<string> GetDownloadUrlAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        return await _blobAccessService.GetDownloadUrlAsync(fileItem);
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<string> GetThumbnailUrlAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        return await _blobAccessService.GetThumbnailUrlAsync(fileItem);
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<string> GetStreamUrlAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        return await _blobAccessService.GetStreamUrlAsync(fileItem);
    }

    [Authorize(FileManagerPermissions.FileItems.Default)]
    public async Task<string> GetTemporaryAccessUrlAsync(Guid id, int durationMinutes)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        return await _blobAccessService.GetTemporaryAccessUrlAsync(fileItem, durationMinutes);
    }

    private static string GetContainerName(string? structureKey) =>
        FileItemBlobAccessService.GetContainerName(structureKey);

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<FileItemDto> ConfirmAsync(Guid id)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);

        if (!fileItem.IsTemp)
        {
            return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
        }

        // Track original blob names for potential rollback
        var originalBlobName = fileItem.BlobName;
        var originalThumbnailBlobName = fileItem.ThumbnailBlobName;
        
        // Track new blobs for cleanup on failure
        string? newBlobName = null;
        string? newThumbnailBlobName = null;

        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);
        try
        {
            // Move from temp to permanent
            newBlobName = _blobNameCalculator.Calculate(
                fileItem.Id,
                fileItem.OriginalName,
                false, // Not temp
                fileItem.StructureKey);

            // Copy blob to new location
            byte[] blobData;
            await using (var stream = await blobContainer.GetOrNullAsync(originalBlobName))
            {
                if (stream == null)
                    throw new UserFriendlyException("File blob not found");
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                blobData = ms.ToArray();
            }
            await blobContainer.SaveAsync(newBlobName, new MemoryStream(blobData), overrideExisting: true);

            // Update thumbnail if exists
            if (!string.IsNullOrEmpty(originalThumbnailBlobName))
            {
                await using (var thumbStream = await blobContainer.GetOrNullAsync(originalThumbnailBlobName))
                {
                    if (thumbStream != null)
                    {
                        using var thumbMs = new MemoryStream();
                        await thumbStream.CopyToAsync(thumbMs);
                        var thumbnailData = thumbMs.ToArray();
                        newThumbnailBlobName = newBlobName.Replace(Path.GetExtension(newBlobName), "_thumb" + Path.GetExtension(newBlobName));
                        await blobContainer.SaveAsync(newThumbnailBlobName, new MemoryStream(thumbnailData), overrideExisting: true);
                    }
                    else
                    {
                        fileItem.ThumbnailBlobName = null; // Original thumbnail missing in storage
                    }
                }
            }

            // Update file item with new blob names
            fileItem.BlobName = newBlobName;
            if (newThumbnailBlobName != null)
            {
                fileItem.ThumbnailBlobName = newThumbnailBlobName;
            }
            fileItem.IsTemp = false;

            // Update database
            await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

            // Only delete old blobs after DB update succeeds
            try
            {
                await blobContainer.DeleteAsync(originalBlobName);
                if (!string.IsNullOrEmpty(originalThumbnailBlobName))
                {
                    await blobContainer.DeleteAsync(originalThumbnailBlobName);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - orphan temp blobs will be cleaned up by background job
                _logger.LogWarning(ex, "Failed to delete old temp blobs during confirm: {BlobName}", originalBlobName);
            }

            return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
        }
        catch (Exception ex)
        {
            // Rollback: Delete newly created blobs if they exist
            _logger.LogWarning(ex, "Confirm failed, rolling back blob operations for: {FileId}", id);
            
            try
            {
                if (newBlobName != null)
                {
                    await blobContainer.DeleteAsync(newBlobName);
                }
                if (newThumbnailBlobName != null)
                {
                    await blobContainer.DeleteAsync(newThumbnailBlobName);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback blob creation during confirm: {BlobName}", newBlobName);
            }
            
            throw;
        }
    }

    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task DeleteManyAsync(Guid[] ids)
    {
        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<FileItemDto> ReplaceContentAsync(Guid id, ReplaceFileContentInput input)
    {
        var fileItem = await _fileItemRepository.GetAsync(id);
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);

        // Save content to blob (same blob name = replace)
        await blobContainer.SaveAsync(fileItem.BlobName, input.Content, overrideExisting: true);

        // Regenerate thumbnail for images
        var fileType = DetermineFileType(input.MimeType ?? fileItem.MimeType);
        if (fileType == FileType.Image)
        {
            try
            {
                var thumbnailData = await _imageProcessor.GenerateThumbnailAsync(input.Content, 200, 200);
                var thumbnailBlobName = fileItem.BlobName.Replace(
                    Path.GetExtension(fileItem.BlobName),
                    "_thumb.webp");
                await blobContainer.SaveAsync(thumbnailBlobName, thumbnailData, overrideExisting: true);
                fileItem.ThumbnailBlobName = thumbnailBlobName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to regenerate thumbnail for file {FileId}", id);
                fileItem.ThumbnailBlobName = null;
            }
        }

        // Update entity
        fileItem.Size = input.Content.Length;
        if (!string.IsNullOrEmpty(input.FileName))
        {
            fileItem.OriginalName = input.FileName;
            fileItem.Name = Path.GetFileNameWithoutExtension(input.FileName);
        }
        if (!string.IsNullOrEmpty(input.MimeType))
        {
            fileItem.MimeType = input.MimeType;
        }
        if (fileType == FileType.Image)
        {
            try
            {
                var (width, height) = await _imageProcessor.GetDimensionsAsync(input.Content);
                fileItem.Width = width;
                fileItem.Height = height;
            }
            catch
            {
                // Keep existing dimensions if we can't read them
            }
        }

        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);

        return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
    }

    [Authorize(FileManagerPermissions.FileItems.Create)]
    public async Task<FileItemDto> SaveAsAsync(Guid sourceId, SaveAsFileInput input)
    {
        var sourceFile = await _fileItemRepository.GetAsync(sourceId);
        var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(sourceFile.StructureKey);
        var fileType = DetermineFileType(input.MimeType);

        var fileId = GuidGenerator.Create();
        var blobName = _blobNameCalculator.Calculate(
            fileId,
            input.FileName,
            false,
            sourceFile.StructureKey);

        var fileItem = new FileItem(
            id: fileId,
            tenantId: CurrentTenant.Id,
            name: Path.GetFileNameWithoutExtension(input.FileName),
            originalName: input.FileName,
            blobName: blobName,
            mimeType: input.MimeType,
            size: input.Content.Length,
            fileType: fileType,
            structureKey: sourceFile.StructureKey)
        {
            FolderId = input.FolderId ?? sourceFile.FolderId,
            IsTemp = false,
            IsProcessed = false
        };

        if (fileType == FileType.Image)
        {
            try
            {
                var (width, height) = await _imageProcessor.GetDimensionsAsync(input.Content);
                fileItem.Width = width;
                fileItem.Height = height;

                var thumbnailData = await _imageProcessor.GenerateThumbnailAsync(input.Content, 200, 200);
                var thumbnailBlobName = blobName.Replace(
                    Path.GetExtension(blobName),
                    "_thumb.webp");
                await blobContainer.SaveAsync(thumbnailBlobName, thumbnailData, overrideExisting: true);
                fileItem.ThumbnailBlobName = thumbnailBlobName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process image for SaveAs file {FileName}", input.FileName);
            }
            fileItem.IsProcessed = true;
        }

        await blobContainer.SaveAsync(blobName, input.Content, overrideExisting: true);
        await _fileItemRepository.InsertAsync(fileItem, autoSave: true);

        return ObjectMapper.Map<FileItem, FileItemDto>(fileItem);
    }

    [AllowAnonymous]
    public Task<FileContentResultDto> GetDownloadContentAsync(Guid id, string? token) =>
        _blobAccessService.GetDownloadContentAsync(id, token);

    [AllowAnonymous]
    public Task<StreamContentResultDto> GetStreamContentAsync(Guid id, string? token) =>
        _blobAccessService.GetStreamContentAsync(id, token);

    [AllowAnonymous]
    public Task<FileContentResultDto> GetThumbnailContentAsync(Guid id, string? token) =>
        _blobAccessService.GetThumbnailContentAsync(id, token);

    #region Private Methods

    private async Task<FileStructure> GetStructureByKeyAsync(string key)
    {
        // Try cache first (avoids DB for read paths)
        var cached = await _structureCache.GetAsync(key);
        if (cached != null)
        {
            return MapCacheEntryToStructure(cached);
        }

        // Fallback to repository (e.g. structure just created, cache invalidated)
        var structure = await _fileStructureRepository.FindByKeyAsync(key);
        if (structure == null)
        {
            throw new UserFriendlyException($"File structure '{key}' not found");
        }

        return structure;
    }

    private static FileStructure MapCacheEntryToStructure(StructureCacheEntry entry)
    {
        var structure = new FileStructure(
            Guid.Empty,
            entry.Key,
            entry.Key,
            entry.AllowedFileTypes,
            entry.AllowedExtensions,
            entry.AllowedMimeTypes,
            entry.MaxFileSize)
        {
            IsPublicAccess = entry.IsPublicAccess,
            GenerateThumbnail = entry.GenerateThumbnail,
            ThumbnailWidth = entry.ThumbnailWidth,
            ThumbnailHeight = entry.ThumbnailHeight,
            EnableWebPConversion = entry.EnableWebPConversion,
            WebPQuality = entry.WebPQuality,
            MinImageWidth = entry.MinImageWidth,
            MinImageHeight = entry.MinImageHeight,
            MaxImageWidth = entry.MaxImageWidth,
            MaxImageHeight = entry.MaxImageHeight
        };
        if (entry.ExtraProperties != null && entry.ExtraProperties.Count > 0)
        {
            foreach (var kv in entry.ExtraProperties)
            {
                structure.SetProperty(kv.Key, kv.Value);
            }
        }
        return structure;
    }

    private void ValidateMimeTypeAndExtension(string fileName, string mimeType, FileStructure structure)
    {
        // Validate mime type - normalize to lowercase
        var allowedMimeTypes = (structure.AllowedMimeTypes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (allowedMimeTypes.Any() && !allowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
        {
            throw new UserFriendlyException($"File type '{mimeType}' is not allowed");
        }

        // Validate extension - normalize both to lowercase without dots
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var allowedExtensions = (structure.AllowedExtensions ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (allowedExtensions.Any() && !allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException($"File extension '.{extension}' is not allowed. Allowed types: {structure.AllowedExtensions}");
        }
    }

    private async Task ValidateUploadContentAsync(byte[] content, string fileName, string mimeType, FileStructure structure)
    {
        // Validate file size
        if (content.Length > structure.MaxFileSize)
        {
            throw new UserFriendlyException($"File size exceeds maximum allowed size of {structure.MaxFileSize / (1024 * 1024)}MB");
        }

        // Validate mime type - normalize to lowercase
        var allowedMimeTypes = (structure.AllowedMimeTypes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (allowedMimeTypes.Any() && !allowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
        {
            throw new UserFriendlyException($"File type '{mimeType}' is not allowed");
        }

        // Validate extension - normalize both to lowercase without dots
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var allowedExtensions = (structure.AllowedExtensions ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (allowedExtensions.Any() && !allowedExtensions.Contains(extension))
        {
            throw new UserFriendlyException($"File extension '.{extension}' is not allowed. Allowed types: {structure.AllowedExtensions}");
        }

        // Validate image dimensions if applicable
        var fileType = DetermineFileType(mimeType);
        if (fileType == FileType.Image)
        {
            var (width, height) = await _imageProcessor.GetDimensionsAsync(content);

            if (structure.MinImageWidth.HasValue && width < structure.MinImageWidth.Value)
            {
                throw new UserFriendlyException($"Image width must be at least {structure.MinImageWidth}px");
            }

            if (structure.MinImageHeight.HasValue && height < structure.MinImageHeight.Value)
            {
                throw new UserFriendlyException($"Image height must be at least {structure.MinImageHeight}px");
            }

            if (structure.MaxImageWidth.HasValue && width > structure.MaxImageWidth.Value)
            {
                throw new UserFriendlyException($"Image width must not exceed {structure.MaxImageWidth}px");
            }

            if (structure.MaxImageHeight.HasValue && height > structure.MaxImageHeight.Value)
            {
                throw new UserFriendlyException($"Image height must not exceed {structure.MaxImageHeight}px");
            }
        }
    }

    private FileType DetermineFileType(string mimeType)
    {
        if (mimeType.StartsWith("image/"))
        {
            return FileType.Image;
        }
        else if (mimeType.StartsWith("video/"))
        {
            return FileType.Video;
        }
        else
        {
            return FileType.Document;
        }
    }

    private async Task ProcessImageAsync(FileItem fileItem, byte[] content, FileStructure? structure)
    {
        // Get dimensions
        var (width, height) = await _imageProcessor.GetDimensionsAsync(content);
        fileItem.Width = width;
        fileItem.Height = height;

        // Generate thumbnail if needed
        if (structure?.GenerateThumbnail == true)
        {
            var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);
            var thumbnailData = await _imageProcessor.GenerateThumbnailAsync(
                content,
                structure.ThumbnailWidth,
                structure.ThumbnailHeight);

            var thumbnailBlobName = fileItem.BlobName.Replace(
                Path.GetExtension(fileItem.BlobName),
                "_thumb.webp");

            await blobContainer.SaveAsync(thumbnailBlobName, thumbnailData, overrideExisting: true);
            fileItem.ThumbnailBlobName = thumbnailBlobName;
        }
    }

    private async Task ProcessVideoAsync(FileItem fileItem, byte[] content, FileStructure? structure)
    {
        using var stream = new MemoryStream(content);
        
        // Get metadata
        var metadata = await _videoProcessor.GetMetadataAsync(stream);
        fileItem.Width = metadata.Width;
        fileItem.Height = metadata.Height;
        fileItem.Duration = metadata.Duration;

        // Generate thumbnail if needed
        if (structure?.GenerateThumbnail == true)
        {
            var blobContainer = await _structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);
            stream.Position = 0;
            var thumbnailData = await _videoProcessor.GenerateThumbnailAsync(
                stream,
                null,
                structure.ThumbnailWidth,
                structure.ThumbnailHeight);

            var thumbnailBlobName = fileItem.BlobName.Replace(
                Path.GetExtension(fileItem.BlobName),
                "_thumb.jpg");

            await blobContainer.SaveAsync(thumbnailBlobName, thumbnailData, overrideExisting: true);
            fileItem.ThumbnailBlobName = thumbnailBlobName;
        }
    }

    private async Task<bool> GetStructureIsPublicAccessAsync(string? structureKey)
    {
        return await _structureCache.IsPublicAccessAsync(structureKey);
    }

    private async Task EnrichWithStructureIsPublicAccessAsync(List<FileItemDto> dtos)
    {
        var structureKeys = dtos
            .Where(d => !string.IsNullOrEmpty(d.StructureKey))
            .Select(d => d.StructureKey!)
            .Distinct()
            .ToList();
        if (structureKeys.Count == 0)
            return;
        var allStructures = await _structureCache.GetAllAsync();
        foreach (var dto in dtos)
        {
            if (!string.IsNullOrEmpty(dto.StructureKey) && allStructures.TryGetValue(dto.StructureKey, out var entry))
            {
                ApplyStructurePublicAccess(dto, entry);
            }
        }
    }

    /// <summary>
    /// Fills StructureIsPublicAccess / StructureBaseUrl / StructureStorageProvider so UI can build
    /// direct S3 object URLs (no API proxy) when the structure is public and storage is S3.
    /// </summary>
    private void ApplyStructurePublicAccess(FileItemDto dto, StructureCacheEntry? entry)
    {
        if (entry == null)
        {
            return;
        }

        dto.StructureIsPublicAccess = entry.IsPublicAccess;
        dto.StructureBaseUrl = entry.BaseUrl;
        dto.StructureStorageProvider = entry.ExtraProperties?.GetOrDefault(FileStructureStorageConstants.Provider) as string;

        if (!entry.IsPublicAccess)
        {
            return;
        }

        var containerName = GetContainerName(dto.StructureKey);
        if (_s3PublicBlobUrlProvider.TryGetPublicBaseUrl(containerName, out var publicBaseUrl)
            && !string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            if (string.IsNullOrWhiteSpace(dto.StructureBaseUrl))
            {
                dto.StructureBaseUrl = publicBaseUrl;
            }

            // Default storage settings may not store Provider on the structure — still mark as S3 for direct URLs.
            if (string.IsNullOrWhiteSpace(dto.StructureStorageProvider))
            {
                dto.StructureStorageProvider = nameof(FileStructureStorageProvider.S3Provider);
            }
        }
    }

    #endregion
}
