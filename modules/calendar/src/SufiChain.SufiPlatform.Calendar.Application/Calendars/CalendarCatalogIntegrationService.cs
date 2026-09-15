using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Calendar.Availability;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

[Authorize]
public class CalendarCatalogIntegrationService :
    SufiApplicationService,
    ICalendarCatalogIntegrationService
{
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public CalendarCatalogIntegrationService(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    public virtual async Task<List<CalendarLookupDto>> GetVisibleCalendarsAsync(string? filter = null)
    {
        var personal = await _availabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
        var visible = await _availabilityCalendarAppService.GetMyVisibleCalendarsAsync();

        var items = new List<CalendarLookupDto>(visible.Items.Count + 1)
        {
            ToLookup(personal)
        };

        items.AddRange(visible.Items.Where(calendar => calendar.Id != personal.Id));

        if (!string.IsNullOrWhiteSpace(filter))
        {
            items = items
                .Where(calendar => MatchesFilter(calendar, filter))
                .ToList();
        }

        ResolveInheritanceParents(items);

        return items
            .OrderByDescending(calendar => calendar.IsDefault)
            .ThenBy(calendar => calendar.Kind == CalendarKind.Personal ? 0 : 1)
            .ThenBy(calendar => calendar.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(calendar => calendar.Id)
            .ToList();
    }

    private static CalendarLookupDto ToLookup(CalendarDto calendar)
    {
        return new CalendarLookupDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            Kind = calendar.Kind,
            TimeZoneId = calendar.TimeZoneId,
            OwnerUserId = calendar.OwnerUserId,
            OwnerName = calendar.OwnerName,
            IsDefault = calendar.IsDefault,
            Color = calendar.Color,
            Inheritances = calendar.Inheritances.Select(inheritance => new CalendarInheritanceDto
            {
                Id = inheritance.Id,
                CalendarId = inheritance.CalendarId,
                ParentCalendarId = inheritance.ParentCalendarId,
                ParentCalendarName = inheritance.ParentCalendarName,
                ParentCalendarKind = inheritance.ParentCalendarKind,
                IsInheritedByDefault = inheritance.IsInheritedByDefault
            }).ToList()
        };
    }

    private static void ResolveInheritanceParents(IReadOnlyList<CalendarLookupDto> items)
    {
        var byId = items.ToDictionary(calendar => calendar.Id);
        foreach (var calendar in items)
        {
            foreach (var inheritance in calendar.Inheritances)
            {
                if (!byId.TryGetValue(inheritance.ParentCalendarId, out var parent))
                {
                    continue;
                }

                inheritance.ParentCalendarName ??= parent.Name;
                inheritance.ParentCalendarKind ??= parent.Kind;
            }
        }
    }

    private static bool MatchesFilter(CalendarLookupDto calendar, string filter)
    {
        return calendar.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
               || calendar.Kind.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
               || calendar.TimeZoneId.Contains(filter, StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(calendar.OwnerName)
                   && calendar.OwnerName.Contains(filter, StringComparison.OrdinalIgnoreCase))
               || calendar.Inheritances.Any(inheritance =>
                   !string.IsNullOrWhiteSpace(inheritance.ParentCalendarName)
                   && inheritance.ParentCalendarName.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }
}
