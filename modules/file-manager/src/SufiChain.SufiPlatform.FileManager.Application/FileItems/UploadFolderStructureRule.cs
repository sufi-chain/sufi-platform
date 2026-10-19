using System;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// An upload may target a folder that belongs to a structure only with that structure's key. A folder lists
/// only files of its own structure, so any other key would store a file nobody can see (sufi-chain#479).
/// </summary>
public static class UploadFolderStructureRule
{
    public static bool Accepts(string? folderStructureKey, string? uploadStructureKey)
    {
        if (string.IsNullOrWhiteSpace(folderStructureKey) || string.IsNullOrWhiteSpace(uploadStructureKey))
        {
            return true;
        }

        return string.Equals(folderStructureKey.Trim(), uploadStructureKey.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
