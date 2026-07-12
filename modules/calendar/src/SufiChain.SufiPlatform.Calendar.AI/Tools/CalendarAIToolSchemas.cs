namespace SufiChain.SufiPlatform.Calendar.AI.Tools;

/// <summary>
/// JSON parameter schemas for Calendar AI tools.
/// </summary>
public static class CalendarAIToolSchemas
{
    public const string FreeBusy = """
        {
          "type": "object",
          "properties": {
            "calendarIds": { "type": "array", "items": { "type": "string", "format": "uuid" } },
            "fromUtc": { "type": "string", "format": "date-time" },
            "toUtc": { "type": "string", "format": "date-time" }
          },
          "required": [ "calendarIds", "fromUtc", "toUtc" ]
        }
        """;

    public const string FindFreeSlots = """
        {
          "type": "object",
          "properties": {
            "calendarIds": { "type": "array", "items": { "type": "string", "format": "uuid" } },
            "fromUtc": { "type": "string", "format": "date-time" },
            "toUtc": { "type": "string", "format": "date-time" },
            "duration": { "type": "string", "description": "Slot duration as a TimeSpan, for example 00:30:00." }
          },
          "required": [ "calendarIds", "fromUtc", "toUtc", "duration" ]
        }
        """;

    public const string CreateEvent = """
        {
          "type": "object",
          "properties": {
            "calendarId": { "type": "string", "format": "uuid", "description": "Target calendar id. If unknown, first call calendar.list_calendars and use the default or first matching calendar when the user asks for defaults." },
            "title": { "type": "string", "description": "Event title from the conversation. A concise natural title may be formed from the user's subject." },
            "startUtc": { "type": "string", "format": "date-time", "description": "Exact event start converted to UTC from the user's requested local time. Use the selected calendar's TimeZoneId when the user asks for the default timezone." },
            "endUtc": { "type": "string", "format": "date-time", "description": "Exact event end converted to UTC from the user's requested local time. If the user gives a range, use that range; if missing, ask one short question for end time or duration." },
            "isAllDay": { "type": "boolean", "description": "True only when the user explicitly asks for an all-day event." },
            "timeZoneId": { "type": "string", "description": "User's intended timezone. If the user says default timezone, inherit the selected calendar's TimeZoneId. Do not ask separately for timezone unless no calendar timezone is available." },
            "location": { "type": "string" },
            "description": { "type": "string" },
            "availabilityCalendarId": { "type": "string", "format": "uuid" },
            "sourceType": { "type": "string" },
            "sourceId": { "type": "string" }
          },
          "required": [ "calendarId", "title", "startUtc", "endUtc" ]
        }
        """;

    public const string SearchEvents = """
        {
          "type": "object",
          "properties": {
            "calendarId": { "type": "string", "format": "uuid", "description": "Optional calendar id. If unknown and the user refers to a personal/default calendar, first call calendar.list_calendars." },
            "fromUtc": { "type": "string", "format": "date-time", "description": "Optional UTC lower bound. Use a reasonable narrow range from the user's conversation, such as the mentioned day or week." },
            "toUtc": { "type": "string", "format": "date-time", "description": "Optional UTC upper bound. Use with fromUtc to avoid broad searches." },
            "titleContains": { "type": "string", "description": "Optional title text from the user's wording." },
            "maxResultCount": { "type": "integer", "minimum": 1, "maximum": 20, "description": "Maximum events to return. Keep this small; default is 10." }
          }
        }
        """;

    public const string MoveEvent = """
        {
          "type": "object",
          "properties": {
            "eventId": { "type": "string", "format": "uuid", "description": "Existing non-recurring event id from calendar.search_events or a previous tool result. Do not guess." },
            "movedStartUtc": { "type": "string", "format": "date-time", "description": "New start time in UTC, converted from the user's requested local time using the event or calendar timezone." },
            "movedEndUtc": { "type": "string", "format": "date-time", "description": "New end time in UTC. Preserve the existing duration unless the user asks for a different end time or duration." }
          },
          "required": [ "eventId", "movedStartUtc", "movedEndUtc" ]
        }
        """;

    public const string CancelEvent = """
        {
          "type": "object",
          "properties": {
            "eventId": { "type": "string", "format": "uuid", "description": "Existing event id from calendar.search_events or a previous tool result. Do not guess." }
          },
          "required": [ "eventId" ]
        }
        """;

    public const string MoveOccurrence = """
        {
          "type": "object",
          "properties": {
            "eventId": { "type": "string", "format": "uuid" },
            "originalStartUtc": { "type": "string", "format": "date-time" },
            "movedStartUtc": { "type": "string", "format": "date-time" },
            "movedEndUtc": { "type": "string", "format": "date-time" },
            "thisAndFollowing": { "type": "boolean" }
          },
          "required": [ "eventId", "originalStartUtc", "movedStartUtc", "movedEndUtc" ]
        }
        """;

    public const string CancelOccurrence = """
        {
          "type": "object",
          "properties": {
            "eventId": { "type": "string", "format": "uuid" },
            "originalStartUtc": { "type": "string", "format": "date-time" },
            "thisAndFollowing": { "type": "boolean" }
          },
          "required": [ "eventId", "originalStartUtc" ]
        }
        """;

    public const string TestAvailability = """
        {
          "type": "object",
          "properties": {
            "calendarId": { "type": "string", "format": "uuid" },
            "utcInstant": { "type": "string", "format": "date-time" }
          },
          "required": [ "calendarId", "utcInstant" ]
        }
        """;

    public const string GetCurrentTime = """
        {
          "type": "object",
          "properties": {
            "calendarId": { "type": "string", "format": "uuid", "description": "Optional calendar id. Provide it when the user asks relative to a calendar or default calendar so the tool can use that calendar's TimeZoneId." },
            "timeZoneId": { "type": "string", "description": "Optional explicit timezone id. Use only when the user names a timezone and no calendar timezone should be inherited." }
          }
        }
        """;
}
