namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Error codes for the FileManager module.
/// Format: "SufiChain.SufiPlatform.FileManager:CategoryCode" following ABP's convention.
/// </summary>
public static class FileManagerErrorCodes
{
    // Folder errors (010xxx) - Following ABP's numbering convention
    public const string FolderNotFound = "SufiChain.SufiPlatform.FileManager:010001";
    public const string FolderNameAlreadyExists = "SufiChain.SufiPlatform.FileManager:010002";
    public const string CannotRenameSystemFolders = "SufiChain.SufiPlatform.FileManager:010003";
    public const string CannotDeleteSystemFolders = "SufiChain.SufiPlatform.FileManager:010004";
    public const string FolderNotEmpty = "SufiChain.SufiPlatform.FileManager:010005";
    public const string CannotMoveSystemFolders = "SufiChain.SufiPlatform.FileManager:010006";
    public const string CannotMoveFolderIntoItself = "SufiChain.SufiPlatform.FileManager:010007";
    public const string CannotMoveFolderIntoDescendant = "SufiChain.SufiPlatform.FileManager:010008";
    public const string CannotShareSystemFolders = "SufiChain.SufiPlatform.FileManager:010009";
    
    // File errors (020xxx)
    public const string NoFileProvided = "SufiChain.SufiPlatform.FileManager:020001";
    public const string FileSizeExceedsLimit = "SufiChain.SufiPlatform.FileManager:020002";
    public const string FileTypeNotAllowed = "SufiChain.SufiPlatform.FileManager:020003";
    public const string FileExtensionNotAllowed = "SufiChain.SufiPlatform.FileManager:020004";
    public const string FailedToProcessFile = "SufiChain.SufiPlatform.FileManager:020005";
    public const string ThumbnailNotAvailable = "SufiChain.SufiPlatform.FileManager:020006";
    public const string FileItemNotFound = "SufiChain.SufiPlatform.FileManager:020007";
    
    // Image validation errors (030xxx)
    public const string ImageWidthTooSmall = "SufiChain.SufiPlatform.FileManager:030001";
    public const string ImageHeightTooSmall = "SufiChain.SufiPlatform.FileManager:030002";
    public const string ImageWidthTooLarge = "SufiChain.SufiPlatform.FileManager:030003";
    public const string ImageHeightTooLarge = "SufiChain.SufiPlatform.FileManager:030004";
    
    // Structure errors (040xxx)
    public const string FileStructureNotFound = "SufiChain.SufiPlatform.FileManager:040001";
    public const string FileStructureAlreadyExists = "SufiChain.SufiPlatform.FileManager:040002";
    public const string CannotResetManualStructure = "SufiChain.SufiPlatform.FileManager:040003";
    public const string CannotDeleteStaticStructure = "SufiChain.SufiPlatform.FileManager:040004";
    
    // Processing errors (050xxx)
    public const string ProcessingFailed = "SufiChain.SufiPlatform.FileManager:050001";
    public const string ThumbnailGenerationFailed = "SufiChain.SufiPlatform.FileManager:050002";
    public const string WebPConversionFailed = "SufiChain.SufiPlatform.FileManager:050003";
    
    // Storage errors (060xxx)
    public const string StorageQuotaExceeded = "SufiChain.SufiPlatform.FileManager:060001";
    public const string ZipDownloadNoFiles = "SufiChain.SufiPlatform.FileManager:060002";
}
