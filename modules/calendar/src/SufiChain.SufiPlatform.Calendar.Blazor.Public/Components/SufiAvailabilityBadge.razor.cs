using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiAvailabilityBadge : CalendarPublicComponentBase
{
    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    [Parameter]
    public Guid CalendarId { get; set; }

    [Parameter]
    public DateTime UtcInstant { get; set; } = DateTime.UtcNow;

    private bool _isOpen;

    protected SbColor BadgeColor => _isOpen ? SbColor.Success : SbColor.Danger;

    protected override async Task OnParametersSetAsync()
    {
        if (CalendarId == Guid.Empty)
        {
            _isOpen = false;
            return;
        }

        var result = await AvailabilityCalendarAppService.TestAsync(CalendarId, new TestAvailabilityInput
        {
            UtcInstant = DateTime.SpecifyKind(UtcInstant, DateTimeKind.Utc)
        });
        _isOpen = result.IsOpen;
    }
}
