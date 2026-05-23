namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Defines the type of folder in the file manager hierarchy
/// </summary>
public enum FolderType
{
    /// <summary>
    /// Virtual folder representing a tenant root
    /// </summary>
    TenantRoot = 0,

    /// <summary>
    /// Virtual folder representing a FileStructure
    /// </summary>
    Structure = 1,

    /// <summary>
    /// Virtual folder for year/month date grouping
    /// </summary>
    YearMonth = 2,

    /// <summary>
    /// Real user-created custom folder
    /// </summary>
    Custom = 3
}
