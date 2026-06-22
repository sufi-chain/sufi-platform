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
            .WithDisplayName("Structure:AI:DisplayName")
            .WithDescription("Structure:AI:Description")
            .ForFileTypes(FileType.Image | FileType.Video | FileType.Document | FileType.Audio)
            .WithMaxSize(100.MB())
            .MultipleFiles()
            .GenerateThumbnail(true, 200, 200)
            .EnableWebPConversion(true, 80)
            .ResizeLargeImages(true)
            .IsPublic(false);
    }
}
