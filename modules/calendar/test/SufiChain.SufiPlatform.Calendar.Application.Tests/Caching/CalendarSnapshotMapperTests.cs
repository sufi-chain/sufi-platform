using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using Xunit;
using CalendarAggregate = SufiChain.SufiPlatform.Calendar.Calendars.Calendar;

namespace SufiChain.SufiPlatform.Calendar.Caching;

public class CalendarSnapshotMapperTests
{
    [Fact]
    public void Should_Inherit_Exceptions_But_Not_Working_Hours_By_Default()
    {
        var parent = CreateParentCalendar();
        var child = CreateChildCalendar(parent, inheritWorkingHours: false);

        var snapshot = CalendarSnapshotMapper.ToSnapshot(child, [parent]);

        snapshot.Rules.ShouldBeEmpty();
        snapshot.Exceptions.Count.ShouldBe(1);
        snapshot.Exceptions[0].Date.ShouldBe(new DateOnly(2026, 8, 14));
        snapshot.Exceptions[0].Kind.ShouldBe(CalendarExceptionKind.Closed);
    }

    [Fact]
    public void Should_Inherit_Working_Hours_When_Explicitly_Enabled()
    {
        var parent = CreateParentCalendar();
        var child = CreateChildCalendar(parent, inheritWorkingHours: true);

        var snapshot = CalendarSnapshotMapper.ToSnapshot(child, [parent]);

        snapshot.Rules.Count.ShouldBe(1);
        snapshot.Rules[0].DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        snapshot.Exceptions.Count.ShouldBe(1);
    }

    private static CalendarAggregate CreateParentCalendar()
    {
        var parent = new CalendarAggregate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hijri Shamsi",
            CalendarKind.Default,
            "Iran Standard Time",
            isDefault: true);

        parent.ReplaceWorkingHours(
        [
            new WorkingHourRule(
                Guid.NewGuid(),
                parent.Id,
                DayOfWeek.Saturday,
                new TimeOnly(8, 0),
                new TimeOnly(16, 0))
        ]);
        parent.AddOrReplaceException(new CalendarException(
            Guid.NewGuid(),
            parent.Id,
            new DateOnly(2026, 8, 14),
            CalendarExceptionKind.Closed));
        return parent;
    }

    private static CalendarAggregate CreateChildCalendar(
        CalendarAggregate parent,
        bool inheritWorkingHours)
    {
        var child = new CalendarAggregate(
            Guid.NewGuid(),
            parent.TenantId,
            "Personal",
            CalendarKind.Personal,
            "Iran Standard Time");
        child.AddInheritance(new CalendarInheritance(
            Guid.NewGuid(),
            child.Id,
            parent.Id,
            inheritWorkingHours));
        return child;
    }
}
