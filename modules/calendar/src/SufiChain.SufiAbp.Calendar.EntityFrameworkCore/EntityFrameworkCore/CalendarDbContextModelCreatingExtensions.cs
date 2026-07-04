using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore;

public static class CalendarDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpCalendar(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Calendars.Calendar>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "Calendars", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(CalendarConsts.MaxNameLength);
            b.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(CalendarConsts.MaxTimeZoneIdLength);
            b.Property(x => x.OwnerName).HasMaxLength(CalendarConsts.MaxOwnerNameLength);
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.Kind);
            b.HasIndex(x => new { x.TenantId, x.Kind, x.IsDefault });
            b.HasIndex(x => x.OwnerUserId);

            b.HasMany(x => x.WorkingHourRules).WithOne().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Exceptions).WithOne().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Inheritances).WithOne().HasForeignKey(x => x.CalendarId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CalendarInheritance>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "CalendarInheritances", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => x.CalendarId);
            b.HasIndex(x => x.ParentCalendarId);
            b.HasIndex(x => new { x.CalendarId, x.ParentCalendarId }).IsUnique();
        });

        builder.Entity<WorkingHourRule>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "WorkingHourRules", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.DayOfWeek).HasConversion<string>().HasMaxLength(16);
            b.Property(x => x.StartTime).HasConversion(v => v.ToTimeSpan(), v => TimeOnly.FromTimeSpan(v));
            b.Property(x => x.EndTime).HasConversion(v => v.ToTimeSpan(), v => TimeOnly.FromTimeSpan(v));
            b.Property(x => x.DisplayOrder);
            b.HasIndex(x => x.CalendarId);
            b.HasIndex(x => new { x.CalendarId, x.DisplayOrder });
            b.HasIndex(x => new { x.CalendarId, x.DayOfWeek });
        });

        builder.Entity<CalendarException>(b =>
        {
            var workingHourRangeListComparer = new ValueComparer<List<WorkingHourRange>>(
                (left, right) => ReferenceEquals(left, right) || (left != null && right != null && left.SequenceEqual(right)),
                ranges => ranges == null ? 0 : ranges.Aggregate(0, (hashCode, range) => HashCode.Combine(hashCode, range.GetHashCode())),
                ranges => ranges == null ? new List<WorkingHourRange>() : ranges.ToList());

            b.ToTable(CalendarConsts.DbTablePrefix + "Exceptions", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Date).HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v));
            b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(CalendarConsts.MaxDescriptionLength);
            var rangesProperty = b.Property(x => x.Ranges)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<WorkingHourRange>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<WorkingHourRange>());
            rangesProperty.Metadata.SetValueComparer(workingHourRangeListComparer);

            b.HasIndex(x => x.CalendarId);
            b.HasIndex(x => new { x.CalendarId, x.Date });
        });

        builder.Entity<CalendarEvent>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "Events", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(EventConsts.MaxTimeZoneIdLength);
            b.Property(x => x.Location).HasMaxLength(EventConsts.MaxLocationLength);
            b.Property(x => x.Description).HasMaxLength(EventConsts.MaxDescriptionLength);
            b.Property(x => x.Color).HasMaxLength(EventConsts.MaxColorLength);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.SourceType).HasMaxLength(EventConsts.MaxSourceTypeLength);
            b.Property(x => x.SourceId).HasMaxLength(EventConsts.MaxSourceIdLength);

            b.HasOne(x => x.RecurrenceRule).WithOne().HasForeignKey<RecurrenceRule>(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.OccurrenceExceptions).WithOne().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Attendees).WithOne().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Reminders).WithOne().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.CalendarId);
            b.HasIndex(x => new { x.CalendarId, x.StartUtc, x.EndUtc });
            b.HasIndex(x => new { x.SourceType, x.SourceId });
        });

        builder.Entity<RecurrenceRule>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "RecurrenceRules", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Rule).IsRequired().HasMaxLength(EventConsts.MaxRecurrenceRuleLength);
            b.Property(x => x.Frequency).IsRequired().HasMaxLength(EventConsts.MaxRecurrenceFrequencyLength);
            b.HasIndex(x => x.EventId).IsUnique();
        });

        builder.Entity<EventOccurrenceException>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "EventOccurrenceExceptions", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => x.EventId);
            b.HasIndex(x => new { x.EventId, x.OriginalStartUtc });
        });

        builder.Entity<EventAttendee>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "EventAttendees", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Email).HasMaxLength(EventConsts.MaxAttendeeEmailLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(EventConsts.MaxAttendeeDisplayNameLength);
            b.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
            b.Property(x => x.RsvpStatus).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => x.EventId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Email);
        });

        builder.Entity<EventReminder>(b =>
        {
            b.ToTable(CalendarConsts.DbTablePrefix + "EventReminders", CalendarConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32);
            b.HasIndex(x => x.EventId);
            b.HasIndex(x => x.AttendeeId);
            b.HasIndex(x => x.SentAtUtc);
        });
    }
}
