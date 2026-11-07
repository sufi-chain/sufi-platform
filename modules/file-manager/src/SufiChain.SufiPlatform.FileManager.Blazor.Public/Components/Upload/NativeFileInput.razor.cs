using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components.Forms;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;

/// <summary>
/// Native file input whose <c>accept</c> attribute is a comma-separated list of dotted extensions and MIME tokens.
/// </summary>
public partial class NativeFileInput
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// Raw extension or MIME list. Stored structure values may omit dots or use spaces.
    /// </summary>
    [Parameter]
    public string? Accept { get; set; }

    private string? NormalizedAccept
    {
        get
        {
            var normalized = HtmlFileAccept.Normalize(Accept);
            return normalized.Length == 0 ? null : normalized;
        }
    }
}
