using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Application service for folder management
/// </summary>
[RequiresFeature(SufiFileManagerFeatures.Enable, SufiFileManagerFeatures.FileItems)]
public class FolderAppService : SufiApplicationService, IFolderAppService
{
   private readonly IFileFolderRepository _folderRepository;
   private readonly IFileItemRepository _fileItemRepository;
    private readonly IFolderAccessResolver _accessResolver;
    private readonly IUserFolderAccessContextProvider _accessContextProvider;

   public FolderAppService(
       IFileFolderRepository folderRepository,
        IFileItemRepository fileItemRepository,
        IFolderAccessResolver accessResolver,
        IUserFolderAccessContextProvider accessContextProvider)
   {
       _folderRepository = folderRepository;
       _fileItemRepository = fileItemRepository;
        _accessResolver = accessResolver;
        _accessContextProvider = accessContextProvider;
   }

    #region Tree Operations

    public async Task<List<FolderTreeNodeDto>> GetTreeAsync(Guid? tenantId = null)
    {
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

       var rootFolders = folders.Where(f => f.ParentId == null).ToList();

        var accessContext = await _accessContextProvider.GetContextAsync();
        return BuildTreeNodes(rootFolders, folders, accessContext);
   }

    public async Task<List<FolderTreeNodeDto>> GetChildrenAsync(Guid? parentId, string? parentPath = null)
    {
        List<FileFolder> children;

        if (parentId.HasValue)
        {
            children = await _folderRepository.GetChildrenAsync(parentId.Value);
        }
        else if (!string.IsNullOrEmpty(parentPath))
        {
            var parent = await _folderRepository.FindByPathAsync(parentPath, CurrentTenant.Id);
            if (parent == null)
            {
                return new List<FolderTreeNodeDto>();
            }
            children = await _folderRepository.GetChildrenAsync(parent.Id);
        }
        else
        {
            children = await _folderRepository.GetRootFoldersAsync(CurrentTenant.Id);
        }

       var result = new List<FolderTreeNodeDto>();
        var accessContext = await _accessContextProvider.GetContextAsync();
       foreach (var child in children)
       {
           var hasChildren = await _folderRepository.HasChildrenAsync(child.Id);
           var fileCount = await GetFileCountInFolderAsync(child.Id);
            var (canWrite, canDelete) = await EvaluateNodeAccessAsync(child, accessContext);

           result.Add(new FolderTreeNodeDto
           {
               Id = child.Id,
               Name = child.Name,
               Path = child.Path,
               ParentId = child.ParentId,
               Type = MapFolderType(child.Type),
               Icon = child.Icon ?? GetDefaultIcon(child.Type),
               Color = child.Color,
               HasChildren = hasChildren,
               FileCount = fileCount,
               IsVirtual = child.Type != FolderType.Custom,
               StructureKey = child.StructureKey,
               TenantId = child.TenantId,
               IsShared = child.IsShared,
                CanWrite = canWrite,
                CanDelete = canDelete
           });
       }

        return result;
    }

    public async Task<FolderContentsDto> GetContentsAsync(GetFolderContentsInput input)
    {
        if (input.SourceMode == FileExplorerSourceMode.BlobPath)
        {
            return await GetBlobPathContentsAsync(input);
        }

        FileFolder? folder = null;

        if (input.FolderId.HasValue)
        {
            folder = await _folderRepository.GetAsync(input.FolderId.Value);
        }
        else if (!string.IsNullOrEmpty(input.VirtualPath))
        {
            folder = await _folderRepository.FindByPathAsync(input.VirtualPath, CurrentTenant.Id);
        }

        var result = new FolderContentsDto();

        // Build current folder info
        if (folder != null)
        {
            result.CurrentFolder = await BuildFolderTreeNodeAsync(folder);
            
            // Get parent
            if (folder.ParentId.HasValue)
            {
                var parent = await _folderRepository.GetAsync(folder.ParentId.Value);
                result.ParentFolder = await BuildFolderTreeNodeAsync(parent);
            }

            // Build breadcrumbs
            result.Breadcrumbs = await BuildBreadcrumbsAsync(folder);

            // Get subfolders
            var subfolders = await _folderRepository.GetChildrenAsync(folder.Id);
            foreach (var subfolder in subfolders)
            {
                result.Folders.Add(await BuildFolderTreeNodeAsync(subfolder));
            }

            // Get files in folder
            var structureKey = input.StructureKey ?? ResolveStructureKey(folder);
            var files = await GetFilesInFolderAsync(folder.Id, input.SkipCount, input.MaxResultCount, input.Sorting, input.Filter, structureKey);
            result.Files = ObjectMapper.Map<List<FileItem>, List<FileItemDto>>(files);
            result.TotalFileCount = await GetFileCountInFolderAsync(folder.Id, structureKey);
        }
        else
        {
            var rootFolders = await _folderRepository.GetRootFoldersAsync(CurrentTenant.Id);
            if (rootFolders.Count > 0)
            {
                var firstRoot = rootFolders.First();
                result.CurrentFolder = await BuildFolderTreeNodeAsync(firstRoot);
                result.Breadcrumbs = await BuildBreadcrumbsAsync(firstRoot);

                var subfolders = await _folderRepository.GetChildrenAsync(firstRoot.Id);
                foreach (var subfolder in subfolders)
                {
                    result.Folders.Add(await BuildFolderTreeNodeAsync(subfolder));
                }

                var structureKey = input.StructureKey ?? ResolveStructureKey(firstRoot);
                var files = await GetFilesInFolderAsync(firstRoot.Id, input.SkipCount, input.MaxResultCount, input.Sorting, input.Filter, structureKey);
                result.Files = ObjectMapper.Map<List<FileItem>, List<FileItemDto>>(files);
                result.TotalFileCount = await GetFileCountInFolderAsync(firstRoot.Id, structureKey);
                result.TotalFolderCount = result.Folders.Count;
                result.TotalSize = result.Files.Sum(f => f.Size);
                return result;
            }

            result.Files = new List<FileItemDto>();
            result.TotalFileCount = 0;
        }

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
        var folder = await _folderRepository.GetAsync(id);
        return ObjectMapper.Map<FileFolder, FileFolderDto>(folder);
    }

    public async Task<FileFolderDto?> GetByPathAsync(string path)
    {
        var folder = await _folderRepository.FindByPathAsync(path, CurrentTenant.Id);
        return folder != null ? ObjectMapper.Map<FileFolder, FileFolderDto>(folder) : null;
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
                continue;
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
            throw new UserFriendlyException("Cannot delete system folders.");
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

        var permissions = input.Permissions.Select(permDto => new FolderPermission(
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
        var folder = await _folderRepository.GetWithPermissionsAsync(folderId)
            ?? throw new UserFriendlyException("Folder not found.");

       return folder.Permissions.Select(p => new FolderPermissionDto
       {
           Id = p.Id,
           UserId = p.UserId,
           RoleId = p.RoleId,
            OrganizationUnitId = p.OrganizationUnitId,
           Level = MapPermissionLevelDto(p.Level),
           InheritToChildren = p.InheritToChildren
       }).ToList();
    }

   public async Task<bool> HasPermissionAsync(Guid folderId, FolderPermissionLevelDto level)
   {
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
        if (!CurrentTenant.Id.HasValue)
        {
            return new List<FolderTreeNodeDto>();
        }

        var sharedFolders = await _folderRepository.GetSharedFoldersAsync(CurrentTenant.Id.Value);
        var result = new List<FolderTreeNodeDto>();

        foreach (var folder in sharedFolders)
        {
            result.Add(await BuildFolderTreeNodeAsync(folder));
        }

        return result;
    }

    #endregion

    #region Statistics

    public async Task<FolderStatisticsDto> GetStatisticsAsync(Guid? folderId, string? path = null)
    {
        FileFolder? folder = null;

        if (folderId.HasValue)
        {
            folder = await _folderRepository.GetAsync(folderId.Value);
        }
        else if (!string.IsNullOrEmpty(path))
        {
            folder = await _folderRepository.FindByPathAsync(path, CurrentTenant.Id);
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

    private List<FolderTreeNodeDto> BuildTreeNodes(List<FileFolder> rootFolders, List<FileFolder> allFolders, FolderAccessContext? accessContext)
    {
        var result = new List<FolderTreeNodeDto>();

        foreach (var folder in rootFolders.OrderBy(f => f.SortOrder).ThenBy(f => f.Name))
        {
            var (canWrite, canDelete) = FastNodeAccess(folder, accessContext);
            var node = new FolderTreeNodeDto
            {
                Id = folder.Id,
                Name = folder.Name,
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

            var children = allFolders.Where(f => f.ParentId == folder.Id).ToList();
            if (children.Any())
            {
                node.HasChildren = true;
                node.ChildFolderCount = children.Count;
                node.Children = BuildTreeNodes(children, allFolders, accessContext);
            }

            result.Add(node);
        }

        return result;
   }

   private async Task<FolderTreeNodeDto> BuildFolderTreeNodeAsync(FileFolder folder)
   {
       var hasChildren = await _folderRepository.HasChildrenAsync(folder.Id);
       var fileCount = await GetFileCountInFolderAsync(folder.Id);
        var accessContext = await _accessContextProvider.GetContextAsync();
        var (canWrite, canDelete) = await EvaluateNodeAccessAsync(folder, accessContext);

        return new FolderTreeNodeDto
        {
            Id = folder.Id,
            Name = folder.Name,
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

        return breadcrumbs;
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

    private async Task<List<FileItem>> GetFilesWithoutFolderAsync(
        int skipCount = 0,
        int maxResultCount = 50,
        string? sorting = null,
        string? filter = null,
        string? structureKey = null)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m => m.FolderId == null && m.TenantId == CurrentTenant.Id);

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

    private async Task<int> GetFileCountWithoutFolderAsync(string? structureKey = null)
    {
        var query = await _fileItemRepository.GetQueryableAsync();
        query = query.Where(m => m.FolderId == null && m.TenantId == CurrentTenant.Id);
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
   /// access context. Virtual/system folders are never deletable.
   /// </summary>
   private async Task<(bool CanWrite, bool CanDelete)> EvaluateNodeAccessAsync(FileFolder folder, FolderAccessContext context)
   {
       // Virtual folders are never deletable.
       if (folder.Type != FolderType.Custom)
       {
           var canRead = await _accessResolver.HasPermissionAsync(folder, context, FolderPermissionLevel.Read);
           return (canRead, false);
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
    private (bool CanWrite, bool CanDelete) FastNodeAccess(FileFolder folder, FolderAccessContext? context)
    {
        var isCustom = folder.Type == FolderType.Custom;

        if (context == null || context.IsAnonymous)
        {
            return (false, false);
        }

        // Host/admin/owner fast paths.
        if (context.IsHost || context.IsAdmin || folder.CreatorId == context.UserId)
        {
            return (true, isCustom);
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
            var canDelete = isCustom && _accessResolver.HasPermission(chain, context, FolderPermissionLevel.Delete);
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

    private static string GetDefaultIcon(FolderType type) => type switch
    {
        FolderType.TenantRoot => "building",
        FolderType.Structure => "layers",
        FolderType.YearMonth => "calendar",
        FolderType.Custom => "folder",
        _ => "folder"
    };

    private static string SanitizeFolderName(string name)
    {
        // Remove invalid path characters
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return sanitized.Trim().Replace(' ', '-').ToLowerInvariant();
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