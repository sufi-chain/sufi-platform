using Microsoft.AspNetCore.Components;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class CalendarEditorFaultNotice : CalendarPublicComponentBase
{
    [Parameter]
    public EventCallback OnReported { get; set; }

    private bool _reported;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _reported)
        {
            return;
        }

        _reported = true;
        await Notify.ErrorAsync(L["CalendarActionFailed"]);
        if (OnReported.HasDelegate)
        {
            await OnReported.InvokeAsync();
        }
    }
}
