using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB;

public static class CalendarMongoDbContextExtensions
{
    public static void ConfigureSufiAbpCalendar(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Calendars.Calendar>(b =>
        {
            b.CollectionName = CalendarConsts.DbTablePrefix + "Calendars";
        });

        builder.Entity<CalendarEvent>(b =>
        {
            b.CollectionName = CalendarConsts.DbTablePrefix + "Events";
        });
    }
}
