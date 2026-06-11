using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Domain.Entities;

namespace SufiChain.SufiAbp.Calendar.Events;

public class EventAttendee : Entity<Guid>
{
    public virtual Guid EventId { get; private set; }

    public virtual Guid? UserId { get; private set; }

    public virtual string? Email { get; private set; }

    public virtual string DisplayName { get; private set; } = default!;

    public virtual AttendeeRole Role { get; private set; }

    public virtual RsvpStatus RsvpStatus { get; private set; }

    protected EventAttendee()
    {
    }

    public EventAttendee(Guid id, Guid eventId, Guid? userId, string? email, string displayName, AttendeeRole role, RsvpStatus rsvpStatus = RsvpStatus.NeedsAction)
        : base(id)
    {
        EventId = eventId;
        SetIdentity(userId, email, displayName);
        SetRole(role);
        SetRsvpStatus(rsvpStatus);
    }

    public virtual void SetIdentity(Guid? userId, string? email, string displayName)
    {
        if (!userId.HasValue && string.IsNullOrWhiteSpace(email))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidAttendee);
        }

        UserId = userId;
        Email = ValidateOptionalLength(email, nameof(email), EventConsts.MaxAttendeeEmailLength);
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), EventConsts.MaxAttendeeDisplayNameLength);
    }

    public virtual void SetRole(AttendeeRole role)
    {
        Role = role;
    }

    public virtual void SetRsvpStatus(RsvpStatus rsvpStatus)
    {
        RsvpStatus = rsvpStatus;
    }

    private static string? ValidateOptionalLength(string? value, string parameterName, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
        {
            throw new ArgumentException($"{parameterName} can not be longer than {maxLength} characters.", parameterName);
        }

        return value;
    }
}
