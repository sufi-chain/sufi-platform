using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.FileManager.Integration;

/// <inheritdoc cref="IFileStorageTrustedService"/>
public class FileStorageTrustedService : IFileStorageTrustedService, ITransientDependency
{
    protected FileItemAppService FileItemAppService { get; }

    protected IFileItemRepository FileItemRepository { get; }

    protected IFileFolderRepository FolderRepository { get; }

    protected IFileStorageIntegrationService IntegrationService { get; }

    protected IFileAccessTokenService FileAccessTokenService { get; }

    protected IGuidGenerator GuidGenerator { get; }

    protected ICurrentTenant CurrentTenant { get; }

    public FileStorageTrustedService(
        FileItemAppService fileItemAppService,
        IFileItemRepository fileItemRepository,
        IFileFolderRepository folderRepository,
        IFileStorageIntegrationService integrationService,
        IFileAccessTokenService fileAccessTokenService,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        FileItemAppService = fileItemAppService;
        FileItemRepository = fileItemRepository;
        FolderRepository = folderRepository;
        IntegrationService = integrationService;
        FileAccessTokenService = fileAccessTokenService;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
    }

    [UnitOfWork]
    public virtual async Task<FileFolderReferenceDto> EnsureFolderAsync(FileFolderEnsureRequest input)
    {
        Check.NotNull(input, nameof(input));
        var structureKey = Check.NotNullOrWhiteSpace(input.StructureKey, nameof(input.StructureKey)).Trim();
        var root = "/" + structureKey.Trim('/');
        var segments = Check.NotNullOrWhiteSpace(input.Path, nameof(input.Path))
            .Trim()
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Trim())
            .Where(segment => segment.Length > 0)
            .ToArray();

        var normalizedPath = "/" + string.Join('/', segments);
        if (!string.Equals(normalizedPath, root, StringComparison.OrdinalIgnoreCase)
            && !normalizedPath.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Folder path '{input.Path}' is outside structure '{structureKey}'.", nameof(input));
        }

        FileFolder? current = null;
        var currentPath = string.Empty;
        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            var isLeaf = index == segments.Length - 1;
            currentPath += "/" + segment;

            var folder = await FolderRepository.FindByPathAsync(currentPath, CurrentTenant.Id);
            if (folder == null)
            {
                var isRoot = current == null;
                folder = new FileFolder(
                    GuidGenerator.Create(),
                    CurrentTenant.Id,
                    isLeaf && !input.Name.IsNullOrWhiteSpace() ? input.Name!.Trim() : segment,
                    currentPath,
                    isRoot ? FolderType.Structure : FolderType.Custom,
                    current?.Id,
                    structureKey);

                if (isLeaf)
                {
                    ApplyLeafDisplay(folder, input);
                }

                await FolderRepository.InsertAsync(folder, autoSave: true);
            }
            else if (isLeaf && !IsRootFolder(folder) && ApplyLeafDisplay(folder, input))
            {
                await FolderRepository.UpdateAsync(folder, autoSave: true);
            }

            current = folder;
        }

        return new FileFolderReferenceDto
        {
            Id = current!.Id,
            Path = current.Path,
            Name = current.Name,
            StructureKey = current.StructureKey
        };

        bool IsRootFolder(FileFolder folder) => string.Equals(folder.Path, root, StringComparison.OrdinalIgnoreCase);
    }

    [UnitOfWork]
    public virtual async Task<FileReferenceDto> UploadAsync(FileUploadRequest input)
    {
        Check.NotNull(input, nameof(input));
        var uploaded = await FileItemAppService.UploadForIntegrationAsync(new UploadFileInput
        {
            FileName = input.FileName,
            Content = input.Content,
            MimeType = input.MimeType,
            StructureKey = input.StructureKey,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            FolderId = input.FolderId,
            FolderPath = input.FolderPath,
            AutoConfirm = input.AutoConfirm,
            Alt = input.Alt
        });

        var entity = await FileItemRepository.GetAsync(uploaded.Id);
        if (input.Properties is { Count: > 0 })
        {
            foreach (var (key, value) in input.Properties)
            {
                entity.SetProperty(key, value);
            }

            await FileItemRepository.UpdateAsync(entity, autoSave: true);
        }

        return Map(entity);
    }

    public virtual async Task<List<FileReferenceDto>> GetListByEntityAsync(
        string entityType,
        Guid entityId,
        string? structureKey = null)
    {
        Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        var items = structureKey.IsNullOrWhiteSpace()
            ? await FileItemRepository.GetListAsync(item => item.EntityType == entityType && item.EntityId == entityId)
            : await FileItemRepository.GetListAsync(item =>
                item.EntityType == entityType && item.EntityId == entityId && item.StructureKey == structureKey);

        return items
            .OrderBy(item => item.CreationTime)
            .Select(Map)
            .ToList();
    }

    public virtual async Task<FileReferenceDto?> FindAsync(Guid id)
    {
        var entity = await FileItemRepository.FindAsync(id);
        return entity == null ? null : Map(entity);
    }

    public virtual Task<FileContentBytesDto> GetContentAsync(Guid id)
    {
        return IntegrationService.GetContentAsync(id);
    }

    [UnitOfWork]
    public virtual async Task SetPropertiesAsync(Guid id, IReadOnlyDictionary<string, string?> properties)
    {
        Check.NotNull(properties, nameof(properties));
        if (properties.Count == 0)
        {
            return;
        }

        var entity = await FileItemRepository.GetAsync(id);
        foreach (var (key, value) in properties)
        {
            if (value == null)
            {
                entity.RemoveProperty(key);
            }
            else
            {
                entity.SetProperty(key, value);
            }
        }

        await FileItemRepository.UpdateAsync(entity, autoSave: true);
    }

    [UnitOfWork]
    public virtual Task DeleteAsync(Guid id)
    {
        return FileItemAppService.DeleteForIntegrationAsync(id);
    }

    protected virtual bool ApplyLeafDisplay(FileFolder folder, FileFolderEnsureRequest input)
    {
        var changed = false;
        if (!input.Name.IsNullOrWhiteSpace() && !string.Equals(folder.Name, input.Name!.Trim(), StringComparison.Ordinal))
        {
            folder.Name = input.Name.Trim();
            changed = true;
        }

        var resourceName = input.LocalizationResourceName.IsNullOrWhiteSpace()
            ? null
            : input.LocalizationResourceName!.Trim();
        var currentResource = folder.GetProperty<string?>(FileFolderPropertyNames.LocalizationResourceName);
        if (!string.Equals(currentResource, resourceName, StringComparison.Ordinal))
        {
            if (resourceName == null)
            {
                folder.RemoveProperty(FileFolderPropertyNames.LocalizationResourceName);
            }
            else
            {
                folder.SetProperty(FileFolderPropertyNames.LocalizationResourceName, resourceName);
            }

            changed = true;
        }

        if (!input.Description.IsNullOrWhiteSpace() && !string.Equals(folder.Description, input.Description, StringComparison.Ordinal))
        {
            folder.Description = input.Description;
            changed = true;
        }

        return changed;
    }

    protected virtual FileReferenceDto Map(FileItem item)
    {
        string? accessToken = null;
        if (FileAccessTokenService.TryGenerateToken(item.Id, out var token))
        {
            accessToken = token;
        }

        return new FileReferenceDto
        {
            Id = item.Id,
            FileName = item.OriginalName,
            MimeType = item.MimeType,
            SizeInBytes = item.Size,
            AccessToken = accessToken,
            StructureKey = item.StructureKey,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            TenantId = item.TenantId,
            Width = item.Width,
            Height = item.Height,
            CreationTime = item.CreationTime,
            Properties = item.ExtraProperties
                .Where(pair => pair.Value != null)
                .ToDictionary(pair => pair.Key, pair => pair.Value!.ToString() ?? string.Empty, StringComparer.Ordinal)
        };
    }
}
