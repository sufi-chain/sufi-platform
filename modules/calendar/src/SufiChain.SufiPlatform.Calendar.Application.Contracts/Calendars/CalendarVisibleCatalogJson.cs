using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

/// <summary>
/// Compact visible-calendar catalog for hooshvare request context.
/// </summary>
public static class CalendarVisibleCatalogJson
{
    public const string ContextKey = "calendars";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(IReadOnlyList<CalendarLookupDto> calendars)
    {
        var payload = calendars.Select(calendar => new
        {
            id = calendar.Id,
            name = calendar.Name,
            kind = calendar.Kind.ToString(),
            timeZoneId = calendar.TimeZoneId,
            isDefault = calendar.IsDefault,
            inheritances = calendar.Inheritances.Select(inheritance => new
            {
                parentCalendarId = inheritance.ParentCalendarId,
                parentCalendarName = inheritance.ParentCalendarName,
                parentKind = inheritance.ParentCalendarKind?.ToString(),
                isInheritedByDefault = inheritance.IsInheritedByDefault
            })
        }).ToList();

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }
}
