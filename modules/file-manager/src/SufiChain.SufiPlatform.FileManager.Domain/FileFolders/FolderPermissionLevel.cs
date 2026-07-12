using System;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Permission levels for folder access control
/// </summary>
[Flags]
public enum FolderPermissionLevel
{
    /// <summary>
    /// No access
    /// </summary>
    None = 0,

    /// <summary>
    /// Can view folder contents
    /// </summary>
    Read = 1,

    /// <summary>
    /// Can add/modify files in folder
    /// </summary>
    Write = 2,

    /// <summary>
    /// Can delete files and folders
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Can share folder with others
    /// </summary>
    Share = 8,

    /// <summary>
    /// Full access including all permissions
    /// </summary>
    Full = Read | Write | Delete | Share
}
