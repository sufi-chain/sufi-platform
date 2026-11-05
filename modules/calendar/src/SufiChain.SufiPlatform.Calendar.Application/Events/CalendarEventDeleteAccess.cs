using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Events;

/// <summary>
/// The event owner is the user who created the event, or the owner of its calendar.
/// Either may delete without the calendar-wide delete permission. Everyone else needs that permission.
/// </summary>
public static class CalendarEventDeleteAccess
{
    public static bool IsOwner(Guid? currentUserId, Guid? creatorId, Guid? calendarOwnerUserId)
    {
        return currentUserId.HasValue
               && (currentUserId == creatorId || currentUserId == calendarOwnerUserId);
    }

    public static void Ensure(bool isOwner, bool hasDeletePermission)
    {
        if (isOwner || hasDeletePermission)
        {
            return;
        }

        throw new BusinessException(CalendarErrorCodes.DeleteNotAllowed);
    }
}
