using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

/// <summary>
/// Cross-module calendar list for hooshvares and other consumers that need
/// the current user's visible calendars without the full availability CRUD surface.
/// </summary>
[IntegrationService]
public interface ICalendarCatalogIntegrationService : IApplicationService
{
    /// <summary>
    /// Lists calendars the current user can see. Ensures the caller's personal
    /// calendar exists first (same bootstrap as the portal calendar selector).
    /// </summary>
    Task<List<CalendarLookupDto>> GetVisibleCalendarsAsync(string? filter = null);
}
