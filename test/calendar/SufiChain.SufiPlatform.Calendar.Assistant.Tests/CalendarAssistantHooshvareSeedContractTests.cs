using System.Reflection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Calendar;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Data;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare;

public class CalendarAssistantHooshvareSeedContractTests
{
    [Fact]
    public async Task Contributor_Should_Require_CalendarId_And_Expose_Calendar_Tools()
    {
        var definitions = new List<PlatformHooshvareSeedDefinition>();
        var seeder = Substitute.For<IPlatformHooshvareDefinitionSeeder>();
        seeder.SeedAsync(Arg.Any<PlatformHooshvareSeedDefinition>(), Arg.Any<DataSeedContext>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                definitions.Add(call.Arg<PlatformHooshvareSeedDefinition>());
                return Task.CompletedTask;
            });
        var localization = Substitute.For<ILocalizationTextSeeder>();
        localization.UpsertAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<Guid?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var contributor = new CalendarAssistantHooshvareDataSeedContributor(
            seeder,
            new HooshvareLocalizedShortcutPromptsBuilder(Options.Create(new AbpLocalizationOptions())),
            localization);

        await contributor.SeedAsync(new DataSeedContext(null));

        var definition = definitions.ShouldHaveSingleItem();
        definition.Key.ShouldBe(CalendarAssistantHooshvareKeys.Key);
        definition.EntityVersion.ShouldBe(CalendarAssistantHooshvareKeys.EntityVersion);
        definition.RequiredContextKeys.ShouldBe(["calendarId"]);
        definition.RuntimeOptions.UseMcpTools.ShouldBeTrue();
        definition.RuntimeOptions.AllowedMcpToolNames.ShouldContain("calendar.list_calendars");
        definition.SourceModule.ShouldBe("Calendar");
        typeof(CalendarAssistantHooshvareDataSeedContributor).Assembly.GetName().Name
            .ShouldBe("SufiChain.SufiPlatform.Calendar.Assistant");
    }

    [Fact]
    public void Shim_Module_Depends_On_The_Calendar_Assistant_Module()
    {
        var depended = typeof(CalendarHooshvareApplicationModule)
            .GetCustomAttributes<DependsOnAttribute>(inherit: false)
            .SelectMany(attribute => attribute.DependedTypes);

        depended.ShouldContain(typeof(SufiCalendarAssistantModule));
        typeof(SufiCalendarAssistantModule).Assembly
            .GetType(typeof(CalendarAssistantHooshvareDataSeedContributor).FullName!)
            .ShouldNotBeNull();
    }
}
