# Calendar Module

Calendars, events, availability, free/busy, recurrence, and reminders — including public scheduler components and **12 MCP tools** for copilot scheduling.

## Code location

`sufi-platform/modules/calendar/`

## Packages

| Layer | Project |
|-------|---------|
| Domain.Shared | `SufiChain.SufiPlatform.Calendar.Domain.Shared` |
| Domain | `SufiChain.SufiPlatform.Calendar.Domain` |
| Application.Contracts / Application | `…Calendar.Application[.Contracts]` |
| Calendar.AI | `SufiChain.SufiPlatform.Calendar.AI` |
| HttpApi / HttpApi.Client | `…Calendar.HttpApi[.Client]` |
| Blazor / Blazor.Public | `…Calendar.Blazor[.Public]` |
| EF Core / MongoDB | `…Calendar.EntityFrameworkCore` / `…Calendar.MongoDB` |

## Capabilities

- Calendars and events with recurrence and exceptions (RRULE)
- Working hours, availability, free/busy search
- Reminders (email / in-app channels)
- Public components: `SufiCalendarScheduler`, `SufiCalendarView`, `SufiCalendarSelect`, `SufiEventEditor`, `SufiFreeBusyPicker`, `SufiAvailabilityBadge`
- Admin page: calendar management

## MCP tools (12)

| Tool | Purpose |
|------|---------|
| `calendar.list_calendars` | List visible calendars |
| `calendar.get_current_time` | Server time (Jalali/Gregorian) |
| `calendar.get_working_hours` | Working hours for a calendar |
| `calendar.get_free_busy` | Free/busy blocks in a range |
| `calendar.find_free_slots` | Find open slots by duration |
| `calendar.search_events` | Search events |
| `calendar.create_event` | Create event |
| `calendar.move_event` | Move entire event |
| `calendar.move_occurrence` | Move single occurrence |
| `calendar.cancel_event` | Cancel entire event |
| `calendar.cancel_occurrence` | Cancel single occurrence |
| `calendar.test_availability` | Check open/busy at a UTC instant |

## Permissions

- `Calendar.Calendars` — hours, exceptions, share
- `Calendar.Events` — attendees and event operations

## Start in source

- `SufiChain.SufiPlatform.Calendar.Application` — app services, MCP seed contributor
- `SufiChain.SufiPlatform.Calendar.AI` — MCP tool implementations
- `SufiChain.SufiPlatform.Calendar.Blazor.Public` — public UI components

## Related

- [AI Module](../ai/index.md)
- [Architecture decisions](../../architecture/decisions.md) (ADR-M003)
