using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

/// <summary>
/// Classifies events from inherited or shared calendars for personal availability.
/// Default (Hijri Shamsi holidays) stays visible and does not occupy personal time.
/// Personal meetings and Public calendars occupy time when that calendar
/// is owned, inherited, or shared with the user.
/// </summary>
public static class CalendarEventAvailabilitySemantics
{
    public const string PersonalBusyRole = "PersonalBusy";

    public const string WorkCommitmentRole = "WorkCommitment";

    public const string PublicObservanceRole = "PublicObservance";

    public const string OwnRelation = "Own";

    public const string InheritedRelation = "Inherited";

    public const string SharedRelation = "Shared";

    public static bool BlocksPersonalTime(CalendarKind sourceKind)
    {
        return sourceKind is CalendarKind.Personal or CalendarKind.Public;
    }

    public static string GetAvailabilityRole(CalendarKind sourceKind)
    {
        return sourceKind switch
        {
            CalendarKind.Personal => PersonalBusyRole,
            CalendarKind.Public => WorkCommitmentRole,
            _ => PublicObservanceRole
        };
    }

    public static string GetVisibilityRelation(
        Guid eventCalendarId,
        Guid? activeCalendarId,
        IReadOnlyCollection<Guid> inheritedCalendarIds)
    {
        if (activeCalendarId.HasValue && eventCalendarId == activeCalendarId.Value)
        {
            return OwnRelation;
        }

        if (inheritedCalendarIds.Contains(eventCalendarId))
        {
            return InheritedRelation;
        }

        return SharedRelation;
    }
}
