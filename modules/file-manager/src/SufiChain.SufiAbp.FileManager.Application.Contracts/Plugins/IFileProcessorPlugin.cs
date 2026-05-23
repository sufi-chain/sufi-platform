using System.IO;
using System.Threading.Tasks;
using SufiChain.SufiAbp.FileManager.FileTypes;
using SufiChain.SufiAbp.FileManager.Plugins.Dtos;

namespace SufiChain.SufiAbp.FileManager.Plugins;

/// <summary>
/// Interface for custom file processors that can handle specific file types
/// </summary>
public interface IFileProcessorPlugin : IFilePlugin
{
    /// <summary>
    /// Determines if this plugin can process the given file type and MIME type
    /// </summary>
    bool CanProcess(FileType fileType, string mimeType);

    /// <summary>
    /// Processes the input stream with the given options
    /// </summary>
    Task<ProcessingResult> ProcessAsync(Stream inputStream, ProcessingOptions options);
}
