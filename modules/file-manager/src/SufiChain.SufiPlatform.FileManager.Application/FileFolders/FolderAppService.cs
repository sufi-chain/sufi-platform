using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using Volo.Abp.Authorization.Permissions;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Application service for folder management
/// </summary>
[Authorize(FileManagerPermissions.FileItems.Default)]
[RequiresFeature(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems)]
public class FolderAppService : SufiApplicationService, IFolderAppService
{
   private readonly IFileFolderRepository _folderRepository;
   private readonly IFileItemRepository _fileItemRepository;
    private readonly IFolderAccessResolver _accessResolver;
    private readonly IUserFolderAccessContextProvider _accessContextProvider;
    private readonly StructureRootFolderCleanup _structureRootCleanup;
    private readonly IFolderGrantPrincipalStore _principalStore;
    private readonly IPermissionChecker _permissionChecker;

   public FolderAppService(
       IFileFolderRepository folderRepository,
        IFileItemRepository fileItemRepository,
        IFolderAccessResolver accessResolver,
        IUserFolderAccessContextProvider accessContextProvider,
        StructureRootFolderCleanup structureRootCleanup,
        IFolderGrantPrincipalStore principalStore,
        IPermissionChecker permissionChecker)
   {
       _folderRepository = folderRepository;
       _fileItemRepository = fileItemRepository;
        _accessResolver = accessResolver;
        _accessContextProvider = accessContextProvider;
        _structureRootCleanup = structureRootCleanup;
        _principalStore = principalStore;
        _permissionChecker = permissionChecker;
        LocalizationResource = typeof(SufiFileManagerResource);
   }

    #region Tree Operations

    public async Task<List<FolderTreeNodeDto>> GetTreeAsync(Guid? tenantId = null)
    {
        await EnsureFileItemsDefaultAsync();

        // Only host can request another tenant's tree; tenant users must use their own tree.
        Guid? effectiveTenantId;
        if (tenantId.HasValue && CurrentTenant.Id != tenantId)
        {
            if (CurrentTenant.Id.HasValue)
            {
                throw new AbpAuthorizationException(
                    "Not allowed to view another tenant's folder tree.");
            }
            effectiveTenantId = tenantId;
        }
        else
        {
            effectiveTenantId = tenantId ?? CurrentTenant.Id;
        }

        var folders = await _folderRepository.GetFolderTreeAsync(
            effectiveTenantId,
            includeShared: true);

        var accessContext = await _accessContextProvider.GetContextAsync();
        var plan = FolderVisibility.Build(folders, accessContext, _accessResolver);
        var rootFolders = folders.Where(f => f.ParentId == null && plan.Shows(f.Id)).ToList();
        var existingKeys = await _structureRootCleanup.GetExistingKeysAsync();
        return BuildTreeNodes(rootFolders, folders, accessContext, existingKeys, plan);
   }

    public async Task<List<FolderTreeNodeDto>> GetChildrenAsync(Guid? parentId, string? parentPath = null)
    {
        await EnsureFileItemsDefaultAsync();
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var plan = FolderVisibility.Build(folders, accessContext, _accessResolver);
        var byId = folders.ToDictionary(folder => folder.Id);

        Guid? parent = parentId;
        if (!parent.HasValue && !string.IsNullOrEmpty(parentPath))
        {
            var match = folders.FirstOrDefault(folder =>
                string.Equals(folder.Path, parentPath, StringComparison.OrdinalIgnoreCase));
            if (match == null || !plan.Shows(match.Id))
            {
                return new List<FolderTreeNodeDto>();
            }

            parent = match.Id;
        }
        else if (parent.HasValue && (!byId.ContainsKey(parent.Value) || !plan.Shows(parent.Value)))
        {
            throw new AbpAuthorizationException();
        }

        var children = folders.Where(folder =>
            folder.ParentId == parent && plan.Shows(folder.Id)).ToList();
        var result = new List<FolderTreeNodeDto>();
        foreach (var child in children)
        {
            result.Add(await ToListedNodeAsync(child, plan, accessContext, byId));
        }

        return result;
    }

    public async Task<FolderContentsDto> GetContentsAsync(GetFolderContentsInput input)
    {
        await EnsureFileItemsDefaultAsync();
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var plan = FolderVisibility.Build(folders, accessContext, _accessResolver);
        var byId = folders.ToDictionary(folder => folder.Id);

        if (input.SourceMode == FileExplorerSourceMode.BlobPath)
        {
            EnsureBlobPathAllowed(input, folders, accessContext, plan, byId);
            return await GetBlobPathContentsAsync(input);
        }

        FileFolder? folder = null;

        if (input.FolderId.HasValue)
        {
            byId.TryGetValue(input.FolderId.Value, out folder);
            if (folder != null &&
                folder.TenantId != CurrentTenant.Id &&
                !(folder.IsShared && CurrentTenant.Id.HasValue &&
                  folder.GetSharedTenantIds().Contains(CurrentTenant.Id.Value)))
            {
                folder = null;
            }
        }
        else if (!string.IsNullOrEmpty(input.VirtualPath))
        {
            folder = folders.FirstOrDefault(candidate =>
                string.Equals(candidate.Path, input.VirtualPath, StringComparison.OrdinalIgnoreCase));
        }

        var result = new FolderContentsDto();

        if (input.FolderId.HasValue && folder == null)
        {
            return result;
        }

        if (folder != null && !plan.CanRead(folder.Id))
        {
            throw new AbpAuthorizationException();
        }

        if (folder != null)
        {
            result.CurrentFolder = await ToListedNodeAsync(folder, plan, accessContext, byId);

            if (folder.ParentId.HasValue && byId.TryGetValue(folder.ParentId.Value, out var parent))
            {
                result.ParentFolder = await ToListedNodeAsync(parent, plan, accessContext, byId);
            }
            else
            {
                result.ParentFolder = VirtualRootNode();
            }

            result.Breadcrumbs = await BuildBreadcrumbsAsync(folder);

            foreach (var subfolder in folders.Where(candidate => candidate.ParentId == folder.Id && plan.Shows(candidate.Id)))
            {
                result.Folders.Add(await ToListedNodeAsync(subfolder, plan, accessContext, byId));
            }

            var structureKey = ResolveRequestedStructureKey(input.StructureKey, folder);
            if (!string.IsNullOrWhiteSpace(input.StructureKey) &&
                string.IsNullOrWhiteSpace(structureKey))
            {
                return result;
            }
            var files = await GetFilesForFolderContentsAsync(
                folder, structureKey, input.SkipCount, input.MaxResultCount, input.Sorting, input.Filter);
            result.Files = ObjectMapper.Map<List<FileItem>, List<FileItemDto>>(files);
            result.TotalFileCount = await GetFileCountForFolderContentsAsync(folder, structureKey);
        }
        else
        {
            result.CurrentFolder = VirtualRootNode();
            result.Breadcrumbs = new List<BreadcrumbItemDto>
            {
                new()
                {
                    Name = result.CurrentFolder.Name,
                    Path = "/",
                    Icon = "building",
                    IsCurrent = true
                }
            };
            foreach (var rootFolder in folders.Where(candidate => candidate.ParentId == null && plan.Shows(candidate.Id)))
            {
                result.Folders.Add(await ToListedNodeAsync(rootFolder, plan, accessContext, byId));
            }

            result.Files = new List<FileItemDto>();
            result.TotalFileCount = 0;
        }

        FolderNameFilter.Apply(result, input.Filter);
        result.TotalFolderCount = result.Folders.Count;
        result.TotalSize = result.Files.Sum(f => f.Size);

        return result;
    }

    protected virtual async Task<FolderContentsDto> GetBlobPathContentsAsync(GetFolderContentsInput input)
    {
        var currentPath = NormalizeVirtualPath(input.VirtualPath);
        var structureKey = input.StructureKey;
        var allFiles = await GetBlobPathFilesAsync(input.Filter, structureKey);
        var childPrefix = currentPath == "/" ? "" : currentPath.Trim('/') + "/";
        var childFolders = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var directFiles = new List<FileItem>();

        foreach (var file in allFiles)
        {
            var blobName = NormalizeBlobName(file.BlobName);
            if (!blobName.StartsWith(childPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = blobName[childPrefix.Length..];
            if (string.IsNullOrWhiteSpace(remainder))
            {
                continue;
            }

            var slashIndex = remainder.IndexOf('/');
            if (slashIndex >= 0)
            {
                childFolders.Add(remainder[..slashIndex]);
            }
            else
            {
                directFiles.Add(file);
            }
        }

        var result = new FolderContentsDto
        {
            CurrentFolder = new FolderTreeNodeDto
            {
                Id = null,
                Name = currentPath == "/" ? "Blob Storage" : currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).Last(),
                Path = currentPath,
                Type = FolderTypeDto.YearMonth,
                Icon = currentPath == "/" ? "database" : "folder",
                IsVirtual = true,
                CanWrite = false,
                CanDelete = false,
                StructureKey = structureKey
            },
            Breadcrumbs = BuildBlobPathBreadcrumbs(currentPath)
        };

        result.ParentFolder = currentPath == "/"
            ? null
            : new FolderTreeNodeDto
            {
                Id = null,
                Name = "Blob Storage",
                Path = GetParentPath(currentPath) is { Length: > 0 } parentPath ? parentPath : "/",
                Type = FolderTypeDto.YearMonth,
                Icon = "folder",
                IsVirtual = true,
                CanWrite = false,
                CanDelete = false,
                StructureKey = structureKey
            };

        foreach (var childFolder in childFolders)
        {
            var path = currentPath == "/" ? $"/{childFolder}" : $"{currentPath}/{childFolder}";
            result.Folders.Add(new FolderTreeNodeDto
            {
                Id = null,
                Name = childFolder,
                Path = path,
                Type = FolderTypeDto.YearMonth,
                Icon = "folder",
                HasChildren = true,
                IsVirtual = true,
                CanWrite = false,
                CanDelete = false,
                StructureKey = structureKey
            });
        }

        result.Files = ObjectMapper.Map<List<FileItem>, List<FileItemDto>>(
            ApplyFileSorting(directFiles.AsQueryable(), input.Sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList());
        result.TotalFileCount = directFiles.Count;
        FolderNameFilter.Apply(result, input.Filter);
        result.TotalFolderCount = result.Folders.Count;
        result.TotalSize = result.Files.Sum(f => f.Size);
        return result;
    }

    protected virtual async Task<List<FileItem>> GetBlobPathFilesAsync(string? filter, string? structureKey)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(f => !f.IsArchived && f.TenantId == CurrentTenant.Id);

        if (!string.IsNullOrWhiteSpace(structureKey))
        {
            query = query.Where(f => f.StructureKey == structureKey);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(f =>
                f.OriginalName.Contains(filter) ||
                f.Name.Contains(filter) ||
                f.BlobName.Contains(filter));
        }

        return await AsyncExecuter.ToListAsync(query);
    }

    private string? ResolveRequestedStructureKey(string? requestedStructureKey, FileFolder folder)
    {
        var folderStructureKey = folder.StructureKey ?? ResolveStructureKey(folder);
        if (string.IsNullOrWhiteSpace(requestedStructureKey))
        {
            return folderStructureKey;
        }

        return string.Equals(requestedStructureKey, folderStructureKey, StringComparison.OrdinalIgnoreCase)
            ? requestedStructureKey
            : null;
    }

    protected virtual List<BreadcrumbItemDto> BuildBlobPathBreadcrumbs(string currentPath)
    {
        var breadcrumbs = new List<BreadcrumbItemDto>
        {
            new()
            {
                Id = null,
                Name = "Blob Storage",
                Path = "/",
                Icon = "database",
                IsCurrent = currentPath == "/"
            }
        };

        var path = "";
        foreach (var segment in currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            path += "/" + segment;
            breadcrumbs.Add(new BreadcrumbItemDto
            {
                Id = null,
                Name = segment,
                Path = path,
                Icon = "folder",
                IsCurrent = path == currentPath
            });
        }

        return breadcrumbs;
    }

    public async Task<FileFolderDto> GetAsync(Guid id)
    {
        await EnsureFileItemsDefaultAsync();
        var plan = await LoadPlanAsync();
        if (!plan.Shows(id))
        {
            throw new AbpAuthorizationException();
        }

        var folder = await _folderRepository.GetAsync(id);
        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    public async Task<FileFolderDto?> GetByPathAsync(string path)
    {
        await EnsureFileItemsDefaultAsync();
        var folder = await _folderRepository.FindByPathAsync(path, CurrentTenant.Id);
        if (folder == null)
        {
            return null;
        }

        var plan = await LoadPlanAsync();
        if (!plan.Shows(folder.Id))
        {
            throw new AbpAuthorizationException();
        }

        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    public async Task<FileFolderDto?> GetOrCreateFolderByPathAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(normalized) || normalized == "/")
            return null;

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
            return null;

        await EnsureGrantedAsync(FileManagerPermissions.FileItems.Create);
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var byId = folders.ToDictionary(folder => folder.Id);

        string currentPath = "";
        FileFolder? currentFolder = null;

        foreach (var segment in segments)
        {
            var segmentName = SanitizeFolderName(segment);
            if (string.IsNullOrEmpty(segmentName))
                continue;

            var fullPath = string.IsNullOrEmpty(currentPath) ? $"/{segmentName}" : $"{currentPath}/{segmentName}";

            var existing = await _folderRepository.FindByPathAsync(fullPath, CurrentTenant.Id);
            if (existing != null)
            {
                currentFolder = existing;
                currentPath = fullPath;
                byId[existing.Id] = existing;
                continue;
            }

            if (currentFolder != null &&
                !FolderVisibility.CanWrite(FolderVisibility.Chain(currentFolder, byId), accessContext, _accessResolver))
            {
                throw new AbpAuthorizationException();
            }

            var createInput = new CreateFolderInput
            {
                Name = segmentName,
                ParentPath = string.IsNullOrEmpty(currentPath) ? null : currentPath
            };

            var created = await CreateAsync(createInput);
            currentFolder = await _folderRepository.GetAsync(created.Id);
            currentPath = currentFolder.Path;
        }

        return currentFolder != null ? ObjectMapper.Map<FileFolder, FileFolderDto>(currentFolder) : null;
    }

    #endregion

    #region CRUD Operations

    [Authorize(FileManagerPermissions.FileItems.Create)]
    public async Task<FileFolderDto> CreateAsync(CreateFolderInput input)
    {
        // Determine parent and path
        Guid? parentId = input.ParentId;
        string path;

        if (parentId.HasValue)
        {
            var parent = await _folderRepository.GetAsync(parentId.Value);
            path = $"{parent.Path}/{SanitizeFolderName(input.Name)}";
        }
        else if (!string.IsNullOrEmpty(input.ParentPath))
        {
            var parent = await _folderRepository.FindByPathAsync(input.ParentPath, CurrentTenant.Id);
            if (parent != null)
            {
                parentId = parent.Id;
                path = $"{parent.Path}/{SanitizeFolderName(input.Name)}";
            }
            else
            {
                path = $"/{SanitizeFolderName(input.Name)}";
            }
        }
        else
        {
            path = $"/{SanitizeFolderName(input.Name)}";
        }

        // Check if path already exists
        if (await _folderRepository.PathExistsAsync(path, CurrentTenant.Id))
        {
            throw new UserFriendlyException($"A folder with the name '{input.Name}' already exists in this location.");
        }

        var folder = new FileFolder(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.Name,
            path,
            FolderType.Custom,
            parentId,
            parentId.HasValue ? ResolveStructureKey(await _folderRepository.GetAsync(parentId.Value)) : null);

        folder.SetDisplayProperties(input.Icon, input.Color, input.Description);

        await _folderRepository.InsertAsync(folder, autoSave: true);

        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<FileFolderDto> RenameAsync(Guid id, RenameFolderInput input)
    {
        var folder = await _folderRepository.GetAsync(id);

        if (folder.Type != FolderType.Custom)
        {
            throw new UserFriendlyException("Cannot rename system folders.");
        }

        var oldPath = folder.Path;
        var newName = input.NewName;
        var parentPath = GetParentPath(oldPath);
        var newPath = string.IsNullOrEmpty(parentPath) 
            ? $"/{SanitizeFolderName(newName)}" 
            : $"{parentPath}/{SanitizeFolderName(newName)}";

        // Check if new path already exists
        if (oldPath != newPath && await _folderRepository.PathExistsAsync(newPath, CurrentTenant.Id))
        {
            throw new UserFriendlyException($"A folder with the name '{newName}' already exists in this location.");
        }

        folder.Rename(newName, newPath);

        // Update paths of all descendants
        await UpdateDescendantPathsAsync(folder.Id, oldPath, newPath);

        await _folderRepository.UpdateAsync(folder, autoSave: true);

        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    [Authorize(FileManagerPermissions.FileItems.Delete)]
    public async Task DeleteAsync(Guid id, bool recursive = false)
    {
        var folder = await _folderRepository.GetAsync(id);

        if (folder.Type != FolderType.Custom)
        {
            if (!await CanDeleteOrphanStructureRootAsync(folder))
            {
                throw new BusinessException(FileManagerErrorCodes.CannotDeleteSystemFolders);
            }

            await _structureRootCleanup.SoftDeleteAsync(folder, folder.StructureKey, folder.TenantId);
            return;
        }

        var hasChildren = await _folderRepository.HasChildrenAsync(id);
        var hasFiles = await GetFileCountInFolderAsync(id) > 0;

        if ((hasChildren || hasFiles) && !recursive)
        {
            throw new UserFriendlyException("Folder is not empty. Use recursive delete to remove folder and all contents.");
        }

        if (recursive)
        {
            // Delete all descendants
            var descendants = await _folderRepository.GetDescendantsAsync(id);
            foreach (var descendant in descendants.OrderByDescending(d => d.Path.Length))
            {
                // Delete files in each folder
                await DeleteFilesInFolderAsync(descendant.Id);
                await _folderRepository.DeleteAsync(descendant.Id);
            }
        }

        // Delete files in current folder
        await DeleteFilesInFolderAsync(id);
        await _folderRepository.DeleteAsync(id);
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task<FileFolderDto> MoveAsync(Guid id, MoveFolderInput input)
    {
        var folder = await _folderRepository.GetAsync(id);

        if (folder.Type != FolderType.Custom)
        {
            throw new UserFriendlyException("Cannot move system folders.");
        }

        Guid? newParentId = input.NewParentId;
        string newPath;

        if (newParentId.HasValue)
        {
            // Check for circular reference
            if (newParentId.Value == id)
            {
                throw new UserFriendlyException("Cannot move a folder into itself.");
            }

            var descendants = await _folderRepository.GetDescendantsAsync(id);
            if (descendants.Any(d => d.Id == newParentId.Value))
            {
                throw new UserFriendlyException("Cannot move a folder into one of its descendants.");
            }

            var newParent = await _folderRepository.GetAsync(newParentId.Value);
            newPath = $"{newParent.Path}/{SanitizeFolderName(folder.Name)}";
        }
        else if (!string.IsNullOrEmpty(input.NewParentPath))
        {
            var newParent = await _folderRepository.FindByPathAsync(input.NewParentPath, CurrentTenant.Id);
            if (newParent != null)
            {
                newParentId = newParent.Id;
                newPath = $"{newParent.Path}/{SanitizeFolderName(folder.Name)}";
            }
            else
            {
                newPath = $"/{SanitizeFolderName(folder.Name)}";
            }
        }
        else
        {
            newPath = $"/{SanitizeFolderName(folder.Name)}";
        }

        // Check if target path already exists
        if (folder.Path != newPath && await _folderRepository.PathExistsAsync(newPath, CurrentTenant.Id))
        {
            throw new UserFriendlyException($"A folder with this name already exists in the target location.");
        }

        var oldPath = folder.Path;
        folder.MoveTo(newParentId, newPath);

        // Update paths of all descendants
        await UpdateDescendantPathsAsync(folder.Id, oldPath, newPath);

        await _folderRepository.UpdateAsync(folder, autoSave: true);

        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    [Authorize(FileManagerPermissions.FileItems.Create)]
    public async Task<FileFolderDto> CopyAsync(Guid id, Guid? targetParentId)
    {
        var sourceFolder = await _folderRepository.GetAsync(id);

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

        // Copy files (this is a simple copy - files still reference same blobs)
        var sourceFiles = await GetFilesInFolderAsync(id);
        foreach (var file in sourceFiles)
        {
            file.FolderId = newFolder.Id;
            // Note: For a deep copy, you'd need to copy the actual blob data as well
        }

        return ObjectMapper.Map<FileFolder, FileFolderDto>(newFolder);
    }

    #endregion

    #region Permissions

   [Authorize(FileManagerPermissions.FileItems.Update)]
   public async Task SetPermissionsAsync(Guid folderId, SetFolderPermissionsInput input)
   {
       var folder = await _folderRepository.GetWithPermissionsAsync(folderId)
           ?? throw new UserFriendlyException("Folder not found.");

        var requested = input.Permissions ?? new List<FolderPermissionDto>();
        await EnsureGrantPrincipalsExistAsync(requested);

        var permissions = requested.Select(permDto => new FolderPermission(
            GuidGenerator.Create(),
            folderId,
            MapPermissionLevel(permDto.Level),
            permDto.UserId,
            permDto.RoleId,
            permDto.OrganizationUnitId)
        {
            InheritToChildren = permDto.InheritToChildren
        });

        folder.SetPermissions(permissions);

       await _folderRepository.UpdateAsync(folder, autoSave: true);
   }

    public async Task<List<FolderPermissionDto>> GetPermissionsAsync(Guid folderId)
    {
        await EnsureGrantedAsync(FileManagerPermissions.FileItems.Update);
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var folder = folders.FirstOrDefault(candidate => candidate.Id == folderId)
            ?? await _folderRepository.GetWithPermissionsAsync(folderId)
            ?? throw new UserFriendlyException("Folder not found.");
        if (folder.Permissions == null || folder.Permissions.Count == 0)
        {
            var withPermissions = await _folderRepository.GetWithPermissionsAsync(folderId);
            if (withPermissions != null)
            {
                folder = withPermissions;
            }
        }

        var accessContext = await _accessContextProvider.GetContextAsync();
        var byId = folders.ToDictionary(candidate => candidate.Id);
        byId[folder.Id] = folder;
        if (!FolderVisibility.CanShare(FolderVisibility.Chain(folder, byId), accessContext, _accessResolver))
        {
            throw new AbpAuthorizationException();
        }

       var permissions = folder.Permissions.Select(p => new FolderPermissionDto
       {
           Id = p.Id,
           UserId = p.UserId,
           RoleId = p.RoleId,
            OrganizationUnitId = p.OrganizationUnitId,
           Level = MapPermissionLevelDto(p.Level),
           InheritToChildren = p.InheritToChildren
       }).ToList();

        await _principalStore.PopulateAsync(permissions);
        return permissions;
   }

    private async Task EnsureGrantPrincipalsExistAsync(IReadOnlyList<FolderPermissionDto> permissions)
    {
        foreach (var permission in permissions)
        {
            var principalCount = 0;

            if (permission.UserId.HasValue)
            {
                principalCount++;
                if (!await _principalStore.UserExistsAsync(permission.UserId.Value))
                {
                    throw new BusinessException(FileManagerErrorCodes.FolderGrantPrincipalNotFound);
                }
            }

            if (permission.RoleId.HasValue)
            {
                principalCount++;
                if (!await _principalStore.RoleExistsAsync(permission.RoleId.Value))
                {
                    throw new BusinessException(FileManagerErrorCodes.FolderGrantPrincipalNotFound);
                }
            }

            if (permission.OrganizationUnitId.HasValue)
            {
                principalCount++;
                if (!await _principalStore.OrganizationUnitExistsAsync(permission.OrganizationUnitId.Value))
                {
                    throw new BusinessException(FileManagerErrorCodes.FolderGrantPrincipalNotFound);
                }
            }

            if (principalCount != 1)
            {
                throw new BusinessException(FileManagerErrorCodes.FolderGrantPrincipalNotFound);
            }
        }
    }

   public async Task<bool> HasPermissionAsync(Guid folderId, FolderPermissionLevelDto level)
   {
        await EnsureFileItemsDefaultAsync();
        var folder = await _folderRepository.GetWithPermissionsAsync(folderId);
        if (folder == null)
        {
            return false;
        }

        var context = await _accessContextProvider.GetContextAsync();
        return await _accessResolver.HasPermissionAsync(folder, context, MapPermissionLevel(level));
   }

    #endregion

    #region Sharing

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task ShareAsync(Guid folderId, ShareFolderInput input)
    {
        var folder = await _folderRepository.GetAsync(folderId);

        if (folder.Type != FolderType.Custom)
        {
            throw new UserFriendlyException("Cannot share system folders.");
        }

        folder.ShareWith(input.TenantIds);

        await _folderRepository.UpdateAsync(folder, autoSave: true);
    }

    [Authorize(FileManagerPermissions.FileItems.Update)]
    public async Task UnshareAsync(Guid folderId, Guid tenantId)
    {
        var folder = await _folderRepository.GetAsync(folderId);
        var sharedTenants = folder.GetSharedTenantIds();
        sharedTenants.Remove(tenantId);
        folder.ShareWith(sharedTenants);

        await _folderRepository.UpdateAsync(folder, autoSave: true);
    }

    public async Task<List<FolderTreeNodeDto>> GetSharedFoldersAsync()
    {
        await EnsureFileItemsDefaultAsync();
        if (!CurrentTenant.Id.HasValue)
        {
            return new List<FolderTreeNodeDto>();
        }

        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var plan = FolderVisibility.Build(folders, accessContext, _accessResolver);
        var byId = folders.ToDictionary(folder => folder.Id);
        var result = new List<FolderTreeNodeDto>();

        foreach (var folder in folders.Where(candidate => candidate.IsShared && plan.Shows(candidate.Id)))
        {
            result.Add(await ToListedNodeAsync(folder, plan, accessContext, byId));
        }

        return result;
    }

    #endregion

    #region Statistics

    public async Task<FolderStatisticsDto> GetStatisticsAsync(Guid? folderId, string? path = null)
    {
        await EnsureFileItemsDefaultAsync();
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var plan = FolderVisibility.Build(folders, accessContext, _accessResolver);

        FileFolder? folder = null;

        if (folderId.HasValue)
        {
            folder = folders.FirstOrDefault(candidate => candidate.Id == folderId.Value);
        }
        else if (!string.IsNullOrEmpty(path))
        {
            folder = folders.FirstOrDefault(candidate =>
                string.Equals(candidate.Path, path, StringComparison.OrdinalIgnoreCase));
        }

        if (folder != null && !plan.CanRead(folder.Id))
        {
            throw new AbpAuthorizationException();
        }

        var stats = new FolderStatisticsDto
        {
            FolderId = folder?.Id,
            Path = folder?.Path ?? "/"
        };

        if (folder != null)
        {
            // Get all descendants
            var descendants = await _folderRepository.GetDescendantsAsync(folder.Id);
            stats.TotalFolders = descendants.Count - 1; // Exclude the folder itself

            // Get all files in folder and descendants
            var files = await GetFilesInFolderAndDescendantsAsync(folder.Id);
            stats.TotalFiles = files.Count;
            stats.TotalSize = files.Sum(f => f.Size);

            if (files.Any())
            {
                stats.OldestFile = files.Min(f => f.CreationTime);
                stats.NewestFile = files.Max(f => f.CreationTime);
            }

            // Group by file type
            stats.FileTypeStats = files
                .GroupBy(f => f.FileType.ToString())
                .Select(g => new FileTypeStatDto
                {
                    FileType = g.Key,
                    Count = g.Count(),
                    Size = g.Sum(f => f.Size),
                    FormattedSize = FormatFileSize(g.Sum(f => f.Size))
                })
                .ToList();
        }

        stats.FormattedSize = FormatFileSize(stats.TotalSize);

        return stats;
    }

    #endregion

    #region Private Helpers

    private async Task EnsureFileItemsDefaultAsync()
    {
        await EnsureGrantedAsync(FileManagerPermissions.FileItems.Default);
    }

    private async Task EnsureGrantedAsync(string permission)
    {
        if (!await _permissionChecker.IsGrantedAsync(permission))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task<FolderVisibility.Plan> LoadPlanAsync()
    {
        var folders = await _folderRepository.GetFolderTreeAsync(CurrentTenant.Id, includeShared: true);
        var accessContext = await _accessContextProvider.GetContextAsync();
        return FolderVisibility.Build(folders, accessContext, _accessResolver);
    }

    private void EnsureBlobPathAllowed(
        GetFolderContentsInput input,
        IReadOnlyList<FileFolder> folders,
        FolderAccessContext context,
        FolderVisibility.Plan plan,
        IReadOnlyDictionary<Guid, FileFolder> byId)
    {
        if (context.IsHost || context.IsAdmin)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(input.StructureKey))
        {
            throw new AbpAuthorizationException();
        }

        var root = folders.FirstOrDefault(folder =>
            string.Equals(folder.StructureKey, input.StructureKey, StringComparison.OrdinalIgnoreCase) &&
            (folder.ParentId == null || folder.Type == FolderType.Structure));
        if (root == null ||
            !FolderVisibility.CanRead(FolderVisibility.Chain(root, byId), context, _accessResolver))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task<FolderTreeNodeDto> ToListedNodeAsync(
        FileFolder folder,
        FolderVisibility.Plan plan,
        FolderAccessContext accessContext,
        IReadOnlyDictionary<Guid, FileFolder> byId)
    {
        var nameOnly = plan.NameOnly(folder.Id);
        var visibleChildren = 0;
        foreach (var candidate in byId.Values)
        {
            if (candidate.ParentId == folder.Id && plan.Shows(candidate.Id))
            {
                visibleChildren++;
            }
        }

        var (canWrite, canDelete) = nameOnly
            ? (false, false)
            : await EvaluateNodeAccessAsync(folder, accessContext);
        var fileCount = nameOnly
            ? 0
            : await GetFileCountForFolderContentsAsync(folder, folder.StructureKey ?? ResolveStructureKey(folder));

        return new FolderTreeNodeDto
        {
            Id = folder.Id,
            Name = folder.Name,
            LocalizationResourceName = GetLocalizationResourceName(folder),
            Path = folder.Path,
            ParentId = folder.ParentId,
            Type = MapFolderType(folder.Type),
            Icon = folder.Icon ?? GetDefaultIcon(folder.Type),
            Color = folder.Color,
            HasChildren = visibleChildren > 0,
            ChildFolderCount = visibleChildren,
            FileCount = fileCount,
            IsVirtual = folder.Type != FolderType.Custom,
            StructureKey = folder.StructureKey,
            TenantId = folder.TenantId,
            IsShared = folder.IsShared,
            CanWrite = canWrite,
            CanDelete = canDelete
        };
    }

    private List<FolderTreeNodeDto> BuildTreeNodes(
        List<FileFolder> rootFolders,
        List<FileFolder> allFolders,
        FolderAccessContext? accessContext,
        IReadOnlySet<string> existingStructureKeys,
        FolderVisibility.Plan plan)
    {
        var result = new List<FolderTreeNodeDto>();

        foreach (var folder in rootFolders.OrderBy(f => f.SortOrder).ThenBy(f => f.Name))
        {
            if (!plan.Shows(folder.Id))
            {
                continue;
            }

            var nameOnly = plan.NameOnly(folder.Id);
            var (canWrite, canDelete) = nameOnly
                ? (false, false)
                : FastNodeAccess(folder, accessContext, existingStructureKeys);
            var node = new FolderTreeNodeDto
            {
                Id = folder.Id,
                Name = folder.Name,
                LocalizationResourceName = GetLocalizationResourceName(folder),
                Path = folder.Path,
                ParentId = folder.ParentId,
                Type = MapFolderType(folder.Type),
                Icon = folder.Icon ?? GetDefaultIcon(folder.Type),
                Color = folder.Color,
                IsVirtual = folder.Type != FolderType.Custom,
                StructureKey = folder.StructureKey,
                TenantId = folder.TenantId,
                IsShared = folder.IsShared,
                CanWrite = canWrite,
                CanDelete = canDelete
            };
            if (nameOnly)
            {
                node.FileCount = 0;
            }

            var children = allFolders.Where(f => f.ParentId == folder.Id && plan.Shows(f.Id)).ToList();
            if (children.Count > 0)
            {
                node.HasChildren = true;
                node.ChildFolderCount = children.Count;
                node.Children = BuildTreeNodes(children, allFolders, accessContext, existingStructureKeys, plan);
            }

            result.Add(node);
        }

        return result;
    }

   private async Task<FolderTreeNodeDto> BuildFolderTreeNodeAsync(FileFolder folder)
   {
       var hasChildren = await _folderRepository.HasChildrenAsync(folder.Id);
       var fileCount = await GetFileCountForFolderContentsAsync(folder, folder.StructureKey ?? ResolveStructureKey(folder));
        var accessContext = await _accessContextProvider.GetContextAsync();
        var (canWrite, canDelete) = await EvaluateNodeAccessAsync(folder, accessContext);

        return new FolderTreeNodeDto
        {
            Id = folder.Id,
            Name = folder.Name,
            LocalizationResourceName = GetLocalizationResourceName(folder),
            Path = folder.Path,
            ParentId = folder.ParentId,
            Type = MapFolderType(folder.Type),
            Icon = folder.Icon ?? GetDefaultIcon(folder.Type),
            Color = folder.Color,
            HasChildren = hasChildren,
            FileCount = fileCount,
            IsVirtual = folder.Type != FolderType.Custom,
            StructureKey = folder.StructureKey,
            TenantId = folder.TenantId,
           IsShared = folder.IsShared,
            CanWrite = canWrite,
            CanDelete = canDelete
       };
   }

    private async Task<List<BreadcrumbItemDto>> BuildBreadcrumbsAsync(FileFolder folder)
    {
        var breadcrumbs = new List<BreadcrumbItemDto>();
        var current = folder;

        while (current != null)
        {
            breadcrumbs.Insert(0, new BreadcrumbItemDto
            {
                Id = current.Id,
                Name = current.Name,
                LocalizationResourceName = GetLocalizationResourceName(current),
                StructureKey = current.StructureKey,
                Path = current.Path,
                Icon = current.Icon ?? GetDefaultIcon(current.Type),
                IsCurrent = current.Id == folder.Id
            });

            if (current.ParentId.HasValue)
            {
                current = await _folderRepository.GetAsync(current.ParentId.Value);
            }
            else
            {
                break;
            }
        }

        if (breadcrumbs.Count == 0 || breadcrumbs[0].Path != "/")
        {
            breadcrumbs.Insert(0, new BreadcrumbItemDto
            {
                Name = VirtualRootNode().Name,
                Path = "/",
                Icon = "building",
                IsCurrent = false
            });
        }

        return breadcrumbs;
    }

    private async Task<List<FileItem>> GetFilesForFolderContentsAsync(
        FileFolder folder,
        string? structureKey,
        int skipCount = 0,
        int maxResultCount = 50,
        string? sorting = null,
        string? filter = null)
    {
        if (folder.Type == FolderType.Structure && !string.IsNullOrEmpty(structureKey))
        {
            return await GetFilesInStructureRootAsync(
                folder.Id, structureKey, skipCount, maxResultCount, sorting, filter);
        }

        return await GetFilesInFolderAsync(
            folder.Id, skipCount, maxResultCount, sorting, filter, structureKey);
    }

    private async Task<int> GetFileCountForFolderContentsAsync(FileFolder folder, string? structureKey)
    {
        if (folder.Type == FolderType.Structure && !string.IsNullOrEmpty(structureKey))
        {
            return await GetFileCountInStructureRootAsync(folder.Id, structureKey);
        }

        return await GetFileCountInFolderAsync(folder.Id, structureKey);
    }

    /// <summary>
    /// Files belonging to a structure root: either linked to the root folder id, or
    /// structure-scoped uploads with FolderId unset (integration uploads).
    /// </summary>
    private async Task<List<FileItem>> GetFilesInStructureRootAsync(
        Guid folderId,
        string structureKey,
        int skipCount = 0,
        int maxResultCount = 50,
        string? sorting = null,
        string? filter = null)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m =>
            m.StructureKey == structureKey &&
            (m.FolderId == folderId ||
             (m.FolderId == null && m.TenantId == CurrentTenant.Id)));

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(m => m.OriginalName.Contains(filter) || m.Name.Contains(filter));
        }

        query = ApplySorting(query, sorting);

        return await AsyncExecuter.ToListAsync(query.Skip(skipCount).Take(maxResultCount));
    }

    private async Task<int> GetFileCountInStructureRootAsync(Guid folderId, string structureKey)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m =>
            m.StructureKey == structureKey &&
            (m.FolderId == folderId ||
             (m.FolderId == null && m.TenantId == CurrentTenant.Id)));
        return await AsyncExecuter.CountAsync(query);
    }

    private async Task<List<FileItem>> GetFilesInFolderAsync(
        Guid folderId,
        int skipCount = 0,
        int maxResultCount = 50,
        string? sorting = null,
        string? filter = null,
        string? structureKey = null)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m => m.FolderId == folderId);

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(m => m.OriginalName.Contains(filter) || m.Name.Contains(filter));
        }

        if (!string.IsNullOrEmpty(structureKey))
        {
            query = query.Where(m => m.StructureKey == structureKey);
        }

        query = ApplySorting(query, sorting);

        return await AsyncExecuter.ToListAsync(query.Skip(skipCount).Take(maxResultCount));
    }

    private async Task<int> GetFileCountInFolderAsync(Guid folderId, string? structureKey = null)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m => m.FolderId == folderId);
        if (!string.IsNullOrEmpty(structureKey))
        {
            query = query.Where(m => m.StructureKey == structureKey);
        }
        return await AsyncExecuter.CountAsync(query);
    }

    private async Task<List<FileItem>> GetFilesInFolderAndDescendantsAsync(Guid folderId)
    {
        var descendants = await _folderRepository.GetDescendantsAsync(folderId);
        var folderIds = descendants.Select(d => d.Id).ToList();

        var query = await _fileItemRepository.GetQueryableAsync();
        return await AsyncExecuter.ToListAsync(query.Where(m => m.FolderId != null && folderIds.Contains(m.FolderId.Value)));
    }

    private async Task DeleteFilesInFolderAsync(Guid folderId)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        var files = await AsyncExecuter.ToListAsync(query.Where(m => m.FolderId == folderId));

        foreach (var file in files)
        {
            await _fileItemRepository.DeleteAsync(file.Id);
        }
    }

    private async Task UpdateDescendantPathsAsync(Guid folderId, string oldPath, string newPath)
    {
        var descendants = await _folderRepository.GetDescendantsAsync(folderId);
        
        foreach (var descendant in descendants.Where(d => d.Id != folderId))
        {
            var updatedPath = newPath + descendant.Path.Substring(oldPath.Length);
            descendant.Path = updatedPath;
            await _folderRepository.UpdateAsync(descendant);
        }
    }

    private IQueryable<FileItem> ApplySorting(IQueryable<FileItem> query, string? sorting)
    {
        if (string.IsNullOrEmpty(sorting))
        {
            return query.OrderByDescending(m => m.CreationTime);
        }

        var parts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLower();
        var desc = parts.Length > 1 && parts[1].ToUpper() == "DESC";

        return field switch
        {
            "name" => desc ? query.OrderByDescending(m => m.OriginalName) : query.OrderBy(m => m.OriginalName),
            "size" => desc ? query.OrderByDescending(m => m.Size) : query.OrderBy(m => m.Size),
            "type" => desc ? query.OrderByDescending(m => m.FileType) : query.OrderBy(m => m.FileType),
            "date" or "creationtime" => desc ? query.OrderByDescending(m => m.CreationTime) : query.OrderBy(m => m.CreationTime),
            _ => query.OrderByDescending(m => m.CreationTime)
        };
    }

    private IQueryable<FileItem> ApplyFileSorting(IQueryable<FileItem> query, string? sorting) => ApplySorting(query, sorting);

    private string? ResolveStructureKey(FileFolder folder)
    {
        if (!string.IsNullOrWhiteSpace(folder.StructureKey))
        {
            return folder.StructureKey;
        }

        var segments = folder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return null;
        }

        return segments[0];
    }

    private static string NormalizeVirtualPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            return "/";
        }

        return "/" + path.Trim().Trim('/');
    }

   private static string NormalizeBlobName(string blobName) => blobName.Trim().TrimStart('/');

   /// <summary>
   /// Evaluates (CanWrite, CanDelete) for a folder node against the current user's
   /// access context. System folders stay protected, except an orphaned structure root
   /// whose structure no longer exists: an admin can delete that folder.
   /// </summary>
   private async Task<(bool CanWrite, bool CanDelete)> EvaluateNodeAccessAsync(FileFolder folder, FolderAccessContext context)
   {
       if (folder.Type != FolderType.Custom)
       {
           var canRead = await _accessResolver.HasPermissionAsync(folder, context, FolderPermissionLevel.Read);
           var canDeleteOrphan = await CanDeleteOrphanStructureRootAsync(folder);
           return (canRead, canDeleteOrphan);
       }

       var canWrite = await _accessResolver.HasPermissionAsync(folder, context, FolderPermissionLevel.Write);
       var canDelete = await _accessResolver.HasPermissionAsync(folder, context, FolderPermissionLevel.Delete);
       return (canWrite, canDelete);
   }

    /// <summary>
    /// Synchronous fast-path access evaluation for tree building where per-node
    /// permission loading would cause N+1 queries. Resolves host/admin/owner directly,
    /// and falls back to the loaded <see cref="FileFolder.Permissions"/> collection
    /// when present. Non-fast-path users with unloaded permissions get (false, false)
    /// until the node is opened (lazy children use the async path).
    /// </summary>
    private (bool CanWrite, bool CanDelete) FastNodeAccess(
        FileFolder folder,
        FolderAccessContext? context,
        IReadOnlySet<string> existingStructureKeys)
    {
        var isCustom = folder.Type == FolderType.Custom;

        if (context == null || context.IsAnonymous)
        {
            return (false, false);
        }

        var structureExists = !string.IsNullOrWhiteSpace(folder.StructureKey) &&
                              existingStructureKeys.Contains(folder.StructureKey);
        var orphanDeletable = OrphanStructureFolder.CanDelete(
            folder.Type,
            folder.ParentId,
            folder.StructureKey,
            structureExists,
            context.IsAdmin || context.IsHost);

        // Host/admin/owner fast paths.
        if (context.IsHost || context.IsAdmin || folder.CreatorId == context.UserId)
        {
            return (true, isCustom || orphanDeletable);
        }

        // Tenant isolation: a tenant user cannot access another tenant's folder.
        if (folder.TenantId != context.TenantId)
        {
            return (false, false);
        }

        // If permissions happen to be loaded on this node, evaluate them synchronously.
        if (folder.Permissions != null && folder.Permissions.Count > 0)
        {
            var chain = new List<FileFolder> { folder };
            var canWrite = _accessResolver.HasPermission(chain, context, FolderPermissionLevel.Write);
            var canDelete = orphanDeletable ||
                            (isCustom && _accessResolver.HasPermission(chain, context, FolderPermissionLevel.Delete));
            return (canWrite, canDelete);
        }

        // No fast path and no loaded permissions: deny write/delete in the tree view.
        return (false, false);
    }

   private static FolderTypeDto MapFolderType(FolderType type) => type switch
    {
        FolderType.TenantRoot => FolderTypeDto.TenantRoot,
        FolderType.Structure => FolderTypeDto.Structure,
        FolderType.YearMonth => FolderTypeDto.YearMonth,
        FolderType.Custom => FolderTypeDto.Custom,
        _ => FolderTypeDto.Custom
    };

    private static FolderPermissionLevel MapPermissionLevel(FolderPermissionLevelDto level) => level switch
    {
        FolderPermissionLevelDto.None => FolderPermissionLevel.None,
        FolderPermissionLevelDto.Read => FolderPermissionLevel.Read,
        FolderPermissionLevelDto.Write => FolderPermissionLevel.Write,
        FolderPermissionLevelDto.Delete => FolderPermissionLevel.Delete,
        FolderPermissionLevelDto.Share => FolderPermissionLevel.Share,
        FolderPermissionLevelDto.Full => FolderPermissionLevel.Full,
        _ => FolderPermissionLevel.None
    };

    private static FolderPermissionLevelDto MapPermissionLevelDto(FolderPermissionLevel level) => level switch
    {
        FolderPermissionLevel.None => FolderPermissionLevelDto.None,
        FolderPermissionLevel.Read => FolderPermissionLevelDto.Read,
        FolderPermissionLevel.Write => FolderPermissionLevelDto.Write,
        FolderPermissionLevel.Delete => FolderPermissionLevelDto.Delete,
        FolderPermissionLevel.Share => FolderPermissionLevelDto.Share,
        FolderPermissionLevel.Full => FolderPermissionLevelDto.Full,
        _ => FolderPermissionLevelDto.None
    };

    private static string? GetLocalizationResourceName(FileFolder folder)
    {
        return folder.GetProperty<string?>(FileFolderPropertyNames.LocalizationResourceName);
    }

    private static string GetDefaultIcon(FolderType type) => type switch
    {
        FolderType.TenantRoot => "building",
        FolderType.Structure => "layers",
        FolderType.YearMonth => "calendar",
        FolderType.Custom => "folder",
        _ => "folder"
    };

    private FolderTreeNodeDto VirtualRootNode() => new()
    {
        Name = L["Menu:AssetManager"],
        Path = "/",
        Type = FolderTypeDto.TenantRoot,
        Icon = "building",
        IsVirtual = true,
        CanWrite = false,
        CanDelete = false
    };

    private async Task<bool> CanDeleteOrphanStructureRootAsync(FileFolder folder)
    {
        var context = await _accessContextProvider.GetContextAsync();
        if (!(context.IsAdmin || context.IsHost) || !await _structureRootCleanup.IsOrphanAsync(folder))
        {
            return false;
        }

        return true;
    }

    private static string SanitizeFolderName(string name)
    {
        return FilePathSecurity.SanitizeFolderName(name);
    }

    private static string GetParentPath(string path)
    {
        var lastSlash = path.LastIndexOf('/');
        return lastSlash <= 0 ? "" : path.Substring(0, lastSlash);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int i = 0;
        double size = bytes;

        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }

        return $"{size:0.##} {suffixes[i]}";
    }

    #endregion
}
