using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiAbp.FileManager.FileTypes;

namespace SufiChain.SufiAbp.FileManager.Configuration;

/// <summary>
/// Options for configuring file management module
/// </summary>
public class FileManagerOptions
{
    /// <summary>
    /// List of configured file structures
    /// </summary>
    public List<FileStructureConfig> Structures { get; set; } = new();

    /// <summary>
    /// Base URL for generating file download links (e.g., "https://yourapp.com/")
    /// If not set, relative URLs will be used
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Default storage quota per tenant in MB (0 = unlimited)
    /// </summary>
    public long DefaultStorageQuotaMB { get; set; } = 0;

    /// <summary>
    /// Maximum file size in MB to keep in memory during upload.
    /// Files larger than this will be buffered to disk.
    /// Default is 10MB.
    /// </summary>
    public int MaxInMemoryFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Maximum upload file size in MB.
    /// Default is 500MB.
    /// </summary>
    public int MaxUploadFileSizeMB { get; set; } = 500;

    /// <summary>
    /// Default image quality for WebP conversion (1-100)
    /// </summary>
    public int DefaultWebPQuality { get; set; } = 80;

    /// <summary>
    /// Default maximum image dimensions
    /// </summary>
    public int DefaultMaxImageWidth { get; set; } = 4096;
    public int DefaultMaxImageHeight { get; set; } = 4096;

    /// <summary>
    /// Whether to enable automatic WebP conversion by default
    /// </summary>
    public bool EnableWebPConversionByDefault { get; set; } = true;

    /// <summary>
    /// Whether to resize large images automatically by default
    /// </summary>
    public bool ResizeLargeImagesByDefault { get; set; } = true;

   /// <summary>
   /// Whether to seed the default "General" file structure.
   /// Default is true.
   /// </summary>
   public bool SeedDefaultStructures { get; set; } = true;

    /// <summary>
    /// The role name that grants administrative (full) access to all folders
    /// within the user's own tenant. Host users always bypass this check.
    /// Default is "admin".
    /// </summary>
    public string FolderAdminRoleName { get; set; } = "admin";

    /// <summary>
    /// Secret key for signing file access tokens (thumbnail/stream URLs).
    /// When img/video elements load media, they don't send Authorization headers, so we use
    /// signed tokens in the URL. Set a strong secret in production.
    /// </summary>
    public string? FileAccessTokenSecret { get; set; }

    /// <summary>
    /// Validity of file access tokens in minutes. Default is 60.
    /// </summary>
    public int FileAccessTokenValidityMinutes { get; set; } = 60;

    /// <summary>
    /// Start defining a new file structure
    /// </summary>
    public FileStructureBuilder DefineStructure(string key)
    {
        return new FileStructureBuilder(this, key);
    }

    /// <summary>
    /// Adds the default "General" file structure if it doesn't already exist.
    /// This structure supports all common file types (images, videos, documents, audio) including
    /// PDF, Word, Excel, CSV, PowerPoint, JSON, XML, and other formats, with permissive settings.
    /// </summary>
    public FileManagerOptions AddDefaultStructures()
    {
        if (!Structures.Any(s => s.Key == FileStructureKeys.General))
        {
            DefineStructure(FileStructureKeys.General)
                .WithDisplayName("Structure:General:DisplayName")
                .WithDescription("Structure:General:Description")
                .ForFileTypes(FileType.Image | FileType.Video | FileType.Document | FileType.Audio)
                .WithMaxSize(100.MB())
                .MultipleFiles()
                .GenerateThumbnail(true, 200, 200)
                .EnableWebPConversion(true, 80)
                .ResizeLargeImages(true)
                .IsPublic(false);
        }

        return this;
    }
}



