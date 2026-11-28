using Shouldly;
using SufiChain.SufiPlatform.CLI.Modules;
using Xunit;

namespace SufiChain.SufiPlatform.CLI.Tests;

public class ProModuleCatalogCalendarHooshvareTests
{
    [Fact]
    public void Catalog_Still_Lists_Calendar_Hooshvare_Shim_Types()
    {
        var module = ProModuleCatalog.CreateAll().Single(item => item.Key == "calendar-hooshvare");

        module.DisplayName.ShouldBe("Calendar Hooshvare");
        module.DependsOn.ShouldBe(["calendar", "ai-hooshvare"]);
        module.Bindings.Select(binding => binding.ModuleType).ShouldBe(
        [
            "SufiChain.SufiPlatform.Calendar.Hooshvare.CalendarHooshvareApplicationModule",
            "SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.CalendarHooshvareBlazorModule"
        ]);
    }
}
