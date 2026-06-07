namespace SufiChain.SufiAbp.FileManager;

/// <summary>
/// Error codes for the FileManager module.
/// Format: "SufiChain.SufiAbp.FileManager:CategoryCode" following ABP's convention.
/// </summary>
public static class FileManagerErrorCodes
{
    // Folder errors (010xxx) - Following ABP's numbering convention
    public const string FolderNotFound = "SufiChain.SufiAbp.FileManager:010001";
    public const string FolderNameAlreadyExists = "SufiChain.SufiAbp.FileManager:010002";
    public const string CannotRenameSystemFolders = "SufiChain.SufiAbp.FileManager:010003";
    public const string CannotDeleteSystemFolders = "SufiChain.SufiAbp.FileManager:010004";
    public const string FolderNotEmpty = "SufiChain.SufiAbp.FileManager:010005";
    public const string CannotMoveSystemFolders = "SufiChain.SufiAbp.FileManager:010006";
    public const string CannotMoveFolderIntoItself = "SufiChain.SufiAbp.FileManager:010007";
    public const string CannotMoveFolderIntoDescendant = "SufiChain.SufiAbp.FileManager:010008";
    public const string CannotShareSystemFolders = "SufiChain.SufiAbp.FileManager:010009";
    
    // File errors (020xxx)
    public const string NoFileProvided = "SufiChain.SufiAbp.FileManager:020001";
    public const string FileSizeExceedsLimit = "SufiChain.SufiAbp.FileManager:020002";
    public const string FileTypeNotAllowed = "SufiChain.SufiAbp.FileManager:020003";
    public const string FileExtensionNotAllowed = "SufiChain.SufiAbp.FileManager:020004";
    public const string FailedToProcessFile = "SufiChain.SufiAbp.FileManager:020005";
    public const string ThumbnailNotAvailable = "SufiChain.SufiAbp.FileManager:020006";
    public const string FileItemNotFound = "SufiChain.SufiAbp.FileManager:020007";
    
    // Image validation errors (030xxx)
    public const string ImageWidthTooSmall = "SufiChain.SufiAbp.FileManager:030001";
    public const string ImageHeightTooSmall = "SufiChain.SufiAbp.FileManager:030002";
    public const string ImageWidthTooLarge = "SufiChain.SufiAbp.FileManager:030003";
    public const string ImageHeightTooLarge = "SufiChain.SufiAbp.FileManager:030004";
    
    // Structure errors (040xxx)
    public const string FileStructureNotFound = "SufiChain.SufiAbp.FileManager:040001";
    public const string FileStructureAlreadyExists = "SufiChain.SufiAbp.FileManager:040002";
    public const string CannotResetManualStructure = "SufiChain.SufiAbp.FileManager:040003";
    
    // Processing errors (050xxx)
    public const string ProcessingFailed = "SufiChain.SufiAbp.FileManager:050001";
    public const string ThumbnailGenerationFailed = "SufiChain.SufiAbp.FileManager:050002";
    public const string WebPConversionFailed = "SufiChain.SufiAbp.FileManager:050003";
    
    // Storage errors (060xxx)
    public const string StorageQuotaExceeded = "SufiChain.SufiAbp.FileManager:060001";
}
