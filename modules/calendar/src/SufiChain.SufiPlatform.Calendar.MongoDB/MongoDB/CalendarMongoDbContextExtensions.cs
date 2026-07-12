using Volo.Abp;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.Calendar;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

public static class CalendarMongoDbContextExtensions
{
    public static void ConfigureSufiCalendar(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Calendars.Calendar>(b =>
        {
            b.CollectionName = SufiCalendarDbProperties.DbTablePrefix + "Calendars";
        });

        builder.Entity<CalendarEvent>(b =>
        {
            b.CollectionName = SufiCalendarDbProperties.DbTablePrefix + "Events";
        });
    }
}