using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Soft-deletes a structure root, its descendants, and the files in that tree.
/// Callers run this inside the same unit of work as the structure delete.
/// </summary>
public class StructureRootFolderCleanup : DomainService
{
    private readonly IFileFolderRepository _folders;
    private readonly IFileItemRepository _files;
    private readonly IFileStructureRepository _structures;

    public StructureRootFolderCleanup(
        IFileFolderRepository folders,
        IFileItemRepository files,
        IFileStructureRepository structures)
    {
        _folders = folders;
        _files = files;
        _structures = structures;
    }

    public virtual async Task<HashSet<string>> GetExistingKeysAsync()
    {
        var query = await _structures.GetQueryableAsync();
        var keys = await AsyncExecuter.ToListAsync(query.Select(structure => structure.Key));
        return new HashSet<string>(keys, StringComparer.Ordinal);
    }

    public virtual async Task<bool> IsOrphanAsync(FileFolder folder)
    {
        if (!OrphanStructureFolder.IsCandidate(folder.Type, folder.ParentId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(folder.StructureKey))
        {
            return true;
        }

        return !await _structures.KeyExistsAsync(folder.StructureKey);
    }

    public virtual async Task SoftDeleteAsync(FileFolder root, string? structureKey, Guid? tenantId)
    {
        var descendants = await _folders.GetDescendantsAsync(root.Id);
        var folderIds = descendants.Select(folder => folder.Id).ToList();
        if (!folderIds.Contains(root.Id))
        {
            folderIds.Add(root.Id);
            descendants.Add(root);
        }

        await SoftDeleteFilesAsync(folderIds, structureKey, tenantId);

        foreach (var folder in descendants.OrderByDescending(folder => folder.Path.Length))
        {
            await _folders.DeleteAsync(folder, autoSave: false);
        }
    }

    public virtual Task SoftDeleteLooseFilesAsync(string structureKey, Guid? tenantId)
    {
        return SoftDeleteFilesAsync(new List<Guid>(), structureKey, tenantId);
    }

    private async Task SoftDeleteFilesAsync(List<Guid> folderIds, string? structureKey, Guid? tenantId)
    {
        var query = await _files.GetQueryableAsync();
        var files = await AsyncExecuter.ToListAsync(query.Where(file =>
            (folderIds.Count > 0 && file.FolderId != null && folderIds.Contains(file.FolderId.Value)) ||
            (structureKey != null &&
             file.StructureKey == structureKey &&
             file.FolderId == null &&
             file.TenantId == tenantId)));

        foreach (var file in files)
        {
            await _files.DeleteAsync(file, autoSave: false);
        }
    }
}
