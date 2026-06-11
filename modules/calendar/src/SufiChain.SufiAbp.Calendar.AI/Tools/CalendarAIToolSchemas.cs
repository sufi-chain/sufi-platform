namespace SufiChain.SufiAbp.Calendar.AI.Tools;

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
            "calendarId": { "type": "string", "format": "uuid" },
            "title": { "type": "string" },
            "startUtc": { "type": "string", "format": "date-time" },
            "endUtc": { "type": "string", "format": "date-time" },
            "isAllDay": { "type": "boolean" },
            "timeZoneId": { "type": "string", "default": "UTC" },
            "location": { "type": "string" },
            "description": { "type": "string" },
            "availabilityCalendarId": { "type": "string", "format": "uuid" },
            "sourceType": { "type": "string" },
            "sourceId": { "type": "string" }
          },
          "required": [ "calendarId", "title", "startUtc", "endUtc" ]
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
}
