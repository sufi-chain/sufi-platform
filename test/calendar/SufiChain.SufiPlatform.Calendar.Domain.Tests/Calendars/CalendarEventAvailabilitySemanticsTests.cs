using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarEventAvailabilitySemanticsTests
{
    [Theory]
    [InlineData(CalendarKind.Personal, true, CalendarEventAvailabilitySemantics.PersonalBusyRole)]
    [InlineData(CalendarKind.Public, true, CalendarEventAvailabilitySemantics.WorkCommitmentRole)]
    [InlineData(CalendarKind.Default, false, CalendarEventAvailabilitySemantics.PublicObservanceRole)]
    public void Should_Treat_Personal_And_Public_Events_As_Busy_Time(
        CalendarKind sourceKind,
        bool blocksPersonalTime,
        string availabilityRole)
    {
        CalendarEventAvailabilitySemantics.BlocksPersonalTime(sourceKind).ShouldBe(blocksPersonalTime);
        CalendarEventAvailabilitySemantics.GetAvailabilityRole(sourceKind).ShouldBe(availabilityRole);
    }

    [Fact]
    public void Should_Mark_Inherited_And_Shared_Source_Calendars()
    {
        var personalId = Guid.NewGuid();
        var inheritedId = Guid.NewGuid();
        var sharedId = Guid.NewGuid();
        var inheritedIds = new HashSet<Guid> { inheritedId };

        CalendarEventAvailabilitySemantics
            .GetVisibilityRelation(personalId, personalId, inheritedIds)
            .ShouldBe(CalendarEventAvailabilitySemantics.OwnRelation);
        CalendarEventAvailabilitySemantics
            .GetVisibilityRelation(inheritedId, personalId, inheritedIds)
            .ShouldBe(CalendarEventAvailabilitySemantics.InheritedRelation);
        CalendarEventAvailabilitySemantics
            .GetVisibilityRelation(sharedId, personalId, inheritedIds)
            .ShouldBe(CalendarEventAvailabilitySemantics.SharedRelation);
    }
}
