using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Theming;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Common;

/// <summary>
/// Component for displaying file upload progress with cancel option.
/// </summary>
public partial class UploadProgress : ComponentBase
{
    [Parameter, EditorRequired] 
    public string FileName { get; set; } = default!;
    
    [Parameter] 
    public long FileSize { get; set; }
    
    [Parameter] 
    public int Progress { get; set; } = 0;
    
    [Parameter] 
    public bool IsCompleted { get; set; } = false;
    
    [Parameter] 
    public bool HasError { get; set; } = false;
    
    [Parameter] 
    public string? ErrorMessage { get; set; }
    
    [Parameter] 
    public string? StatusMessage { get; set; }
    
    [Parameter] 
    public bool ShowSize { get; set; } = true;
    
    [Parameter] 
    public bool ShowPercentage { get; set; } = true;
    
    [Parameter] 
    public bool ShowCancel { get; set; } = true;
    
    [Parameter] 
    public EventCallback OnCancel { get; set; }
    
    [Parameter] 
    public string CssClass { get; set; } = "";

    private SbColor GetProgressColor()
    {
        if (HasError)
            return SbColor.Danger;
        if (IsCompleted)
            return SbColor.Success;
        return SbColor.Primary;
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
