using System.Collections.Generic;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileTypes;

namespace SufiChain.SufiPlatform.SufiAI.Configuration;

/// <summary>
/// Options for configuring AI Management module
/// </summary>
public class AIOptions
{
    /// <summary>
    /// Whether to seed the default AI file structure.
    /// Only takes effect if file-manager module is configured.
    /// Default is true.
    /// </summary>
    public bool SeedFileStructure { get; set; } = true;

    /// <summary>
    /// Whether to seed the default host AI workspace used by platform hooshvares.
    /// Default is true.
    /// </summary>
    public bool SeedDefaultWorkspace { get; set; } = true;

    /// <summary>
    /// Default workspace seed payload. Bind from configuration section <c>SufiAI:DefaultWorkspace</c>.
    /// </summary>
    public DefaultWorkspaceSeedOptions DefaultWorkspace { get; set; } = new();

    /// <summary>
    /// Shortest TTL for provider <c>/models</c> discovery. Positive configuration below this is raised to 12 hours.
    /// </summary>
    public const int MinimumProviderModelDiscoveryCacheSeconds = 12 * 60 * 60;

    /// <summary>
    /// TTL for cached OpenAI-compatible <c>/models</c> discovery used by admin workspace UI.
    /// The cache key is the normalized connection base URL. Set to 0 to disable caching.
    /// Values above 0 are at least <see cref="MinimumProviderModelDiscoveryCacheSeconds"/>.
    /// </summary>
    public int ProviderModelDiscoveryCacheSeconds { get; set; } = MinimumProviderModelDiscoveryCacheSeconds;

    /// <summary>
    /// TTL for the shared public model catalog. Set to 0 to disable caching. Default is 3600 seconds.
    /// </summary>
    public int ProviderCatalogCacheSeconds { get; set; } = 3600;

    /// <summary>
    /// Retained so existing <c>SufiAI:ProviderModelCacheSalt</c> configuration still binds.
    /// Provider model discovery is keyed by base URL and does not store the API key.
    /// </summary>
    public string ProviderModelCacheSalt { get; set; } = "SufiAI.ProviderModelDiscovery";

    /// <summary>
    /// Name kept for existing configuration. Catalog selection now follows the workspace provider profile.
    /// </summary>
    public string ModelCatalogProvider { get; set; } = "OpenRouter";

    /// <summary>
    /// Text formats accepted for knowledge libraries (converted to Markdown before RAG indexing).
    /// </summary>
    public static readonly string[] KnowledgeTextExtensions = { "md", "markdown", "txt" };

    public static readonly string[] KnowledgeTextMimeTypes =
    {
        "text/plain",
        "text/markdown",
        "text/x-markdown"
    };

    /// <summary>
    /// Adds the default "AI" file structure configuration.
    /// This structure supports all AI-related file types (images, audio, video, documents)
    /// with permissive settings suitable for AI workspaces.
    /// </summary>
    public void AddDefaultFileStructure(FileManagerOptions fileManagerOptions)
    {
        if (fileManagerOptions.Structures.Exists(s => s.Key == AIFileStructureKeys.AI))
        {
            return;
        }

        fileManagerOptions.DefineStructure(AIFileStructureKeys.AI)
            .WithDisplayNameKey("Structure:AI:DisplayName")
            .WithDescriptionKey("Structure:AI:Description")
            .WithLocalizationResource("AI")
            .ForFileTypes(FileType.Image | FileType.Video | FileType.Document | FileType.Audio)
            .AlsoAllowExtensions(KnowledgeTextExtensions)
            .AlsoAllowMimeTypes(KnowledgeTextMimeTypes)
            .WithMaxSize(100.MB())
            .MultipleFiles()
            .GenerateThumbnail(true, 200, 200)
            .EnableWebPConversion(true, 80)
            .ResizeLargeImages(true)
            .IsPublic(false)
            .IsStatic(true);
    }
}
