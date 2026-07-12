using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;

namespace SufiChain.SufiPlatform.Localization.Blazor.Public.Components;

public partial class BusinessLocalizationModeSwitch
{
    [Parameter] public BusinessTextEditorMode Mode { get; set; }
    [Parameter] public EventCallback<BusinessTextEditorMode> ModeChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private async Task SetModeAsync(BusinessTextEditorMode mode)
    {
        if (Disabled || Mode == mode)
        {
            return;
        }

        Mode = mode;
        await ModeChanged.InvokeAsync(mode);
    }
}
