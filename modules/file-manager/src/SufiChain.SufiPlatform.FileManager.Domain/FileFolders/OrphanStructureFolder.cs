using System;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// A top-level structure folder whose structure record is gone. An admin can delete it.
/// </summary>
public static class OrphanStructureFolder
{
    public static bool IsCandidate(FolderType type, Guid? parentId)
    {
        return type == FolderType.Structure && parentId == null;
    }

    public static bool CanDelete(
        FolderType type,
        Guid? parentId,
        string? structureKey,
        bool structureExists,
        bool isAdminOrHost)
    {
        if (!isAdminOrHost || !IsCandidate(type, parentId))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(structureKey) || !structureExists;
    }
}
