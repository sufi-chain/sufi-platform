using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Theming;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.Blazor.Components.Common;

/// <summary>
/// Component for displaying storage quota usage with progress bar.
/// </summary>
public partial class StorageQuotaMeter : ComponentBase
{
    [Parameter] 
    public StorageQuotaDto? Quota { get; set; }
    
    [Parameter] 
    public bool ShowPercentage { get; set; } = true;
    
    [Parameter] 
    public bool ShowDetails { get; set; } = true;
    
    [Parameter] 
    public string CssClass { get; set; } = "";
    
    [Parameter] 
    public string Class
    {
        get => CssClass;
        set => CssClass = value;
    }

    private SbColor GetQuotaColor()
    {
        if (Quota == null) return SbColor.Default;
        
        if (Quota.PercentageUsed >= 100)
            return SbColor.Danger;
        if (Quota.PercentageUsed >= 90)
            return SbColor.Warning;
        if (Quota.PercentageUsed >= 75)
            return SbColor.Info;
        return SbColor.Success;
    }
}
