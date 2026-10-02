using Microsoft.AspNetCore.Components;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public;

public static class CalendarPageRoutes
{
    public const string AdminCalendar = "/panel/admin/calendar";
    public const string PortalCalendar = "/panel/portal/calendar";
    public const string PortalCalendarPro = "/panel/portal/calendar-pro";

    public static string CurrentLocation(NavigationManager navigation)
    {
        var relative = navigation.ToBaseRelativePath(navigation.Uri);
        var withoutHash = relative.Split('#')[0];
        return "/" + withoutHash.TrimStart('/');
    }

    public static string ResolveEditorPrefix(string location)
    {
        var path = location.Split('?', '#')[0];
        if (!path.StartsWith('/'))
        {
            path = "/" + path.TrimStart('/');
        }

        if (path.StartsWith(PortalCalendarPro, StringComparison.OrdinalIgnoreCase))
        {
            return PortalCalendarPro;
        }

        if (path.StartsWith(AdminCalendar, StringComparison.OrdinalIgnoreCase))
        {
            return AdminCalendar;
        }

        return PortalCalendar;
    }

    public static string ResolveReturnUrl(string? returnUrl, string currentLocation)
    {
        if (IsSafeReturnUrl(returnUrl))
        {
            return returnUrl!;
        }

        return ResolveEditorPrefix(currentLocation);
    }

    public static bool IsSafeReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/panel/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !url.StartsWith("//", StringComparison.Ordinal)
            && !url.Contains("://", StringComparison.Ordinal)
            && !url.Contains('\\')
            && !url.Contains("..", StringComparison.Ordinal);
    }
}
