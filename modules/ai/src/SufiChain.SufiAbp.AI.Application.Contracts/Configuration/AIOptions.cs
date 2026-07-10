using System.Collections.Generic;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileTypes;

namespace SufiChain.SufiAbp.AI.Configuration;

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
    /// Whether to seed the default host AI workspace used by platform copilots.
    /// Default is true.
    /// </summary>
    public bool SeedDefaultWorkspace { get; set; } = true;

    /// <summary>
    /// Default workspace seed payload. Bind from configuration section <c>AI:DefaultWorkspace</c>.
    /// </summary>
    public DefaultWorkspaceSeedOptions DefaultWorkspace { get; set; } = new();

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
            .WithMaxSize(100.MB())
            .MultipleFiles()
            .GenerateThumbnail(true, 200, 200)
            .EnableWebPConversion(true, 80)
            .ResizeLargeImages(true)
            .IsPublic(false)
            .IsStatic(true);
    }
}
