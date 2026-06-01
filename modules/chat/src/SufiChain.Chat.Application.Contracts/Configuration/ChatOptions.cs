using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileTypes;

namespace SufiChain.Chat.Configuration;

/// <summary>
/// Chat module startup options.
/// </summary>
public class ChatOptions
{
    /// <summary>
    /// Whether to seed the default Chat attachments file structure when FileManager is configured.
    /// </summary>
    public bool SeedFileStructure { get; set; } = true;

    /// <summary>
    /// Registers the shared <see cref="ChatFileStructureKeys.Attachments"/> structure with FileManager.
    /// Files are scoped by <see cref="ChatEntityTypes.Session"/> and session id — not per-session structure keys.
    /// </summary>
    public void AddDefaultFileStructure(FileManagerOptions fileManagerOptions)
    {
        fileManagerOptions.DefineStructure(ChatFileStructureKeys.Attachments)
            .WithDisplayName("Structure:ChatAttachments:DisplayName")
            .WithDescription("Structure:ChatAttachments:Description")
            .ForFileTypes(FileType.Image | FileType.Video | FileType.Document | FileType.Audio)
            .WithMaxSize(25.MB())
            .MultipleFiles()
            .GenerateThumbnail(true, 200, 200)
            .EnableWebPConversion(true, 80)
            .ResizeLargeImages(true)
            .IsPublic(false);
    }
}
