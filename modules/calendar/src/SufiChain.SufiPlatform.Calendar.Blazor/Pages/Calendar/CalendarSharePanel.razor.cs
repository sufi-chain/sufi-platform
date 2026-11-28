using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Localization;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Pages.Calendar;

public partial class CalendarSharePanel
{
    [Parameter]
    public Guid CalendarId { get; set; }

    [Inject]
    protected ICalendarShareAppService CalendarShareAppService { get; set; } = default!;

    private readonly List<CalendarShareDto> _shares = [];

    private string _userIdText = string.Empty;

    private string? _error;

    protected override async Task OnParametersSetAsync()
    {
        if (CalendarId == Guid.Empty)
        {
            _shares.Clear();
            return;
        }

        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var result = await CalendarShareAppService.GetListAsync(CalendarId);
        _shares.Clear();
        _shares.AddRange(result.Items);
    }

    private async Task GrantAsync()
    {
        _error = null;
        if (!Guid.TryParse(_userIdText, out var userId) || userId == Guid.Empty)
        {
            _error = L["UserRequired"];
            return;
        }

        try
        {
            await CalendarShareAppService.GrantAsync(CalendarId, new GrantCalendarShareInput { UserId = userId });
            _userIdText = string.Empty;
            await ReloadAsync();
        }
        catch (BusinessException exception)
        {
            _error = L[exception.Code ?? "CalendarShareReadOnly"];
        }
    }

    private async Task RevokeAsync(CalendarShareDto share)
    {
        _error = null;
        await CalendarShareAppService.RevokeAsync(CalendarId, share.Id);
        await ReloadAsync();
    }
}
