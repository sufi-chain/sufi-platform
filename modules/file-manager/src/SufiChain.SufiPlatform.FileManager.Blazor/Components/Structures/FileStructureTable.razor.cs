using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Blazor.Helpers;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Structures;

/// <summary>
/// Table component for displaying file structures with actions.
/// </summary>
public partial class FileStructureTable : FileManagerComponentBase
{

    [Parameter, EditorRequired] 
    public IEnumerable<FileStructureDto> FileStructures { get; set; } = Enumerable.Empty<FileStructureDto>();
    
    [Parameter] 
    public EventCallback<FileStructureDto> OnView { get; set; }
    
    [Parameter] 
    public EventCallback<FileStructureDto> OnEdit { get; set; }
    
    [Parameter] 
    public EventCallback<FileStructureDto> OnReset { get; set; }
    
    [Parameter] 
    public EventCallback<FileStructureDto> OnDelete { get; set; }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text[..maxLength] + "...";
    }

    private IEnumerable<FileType> GetFileTypes(FileType allowedTypes)
    {
        var types = new List<FileType>();
        foreach (FileType type in Enum.GetValues<FileType>())
        {
            if (type != FileType.None && allowedTypes.HasFlag(type))
            {
                types.Add(type);
            }
        }
        return types;
    }

    private SbColor GetFileTypeColor(FileType type) => FileManagerHelpers.GetFileTypeColor(type);
}
