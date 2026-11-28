using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.Calendar;
using SufiChain.SufiPlatform.Licensing;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare;

public class CalendarAssistantLicenseStampTests
{
    [Fact]
    public void Assistant_assembly_is_stamped_with_the_Calendar_product()
    {
        var stamp = typeof(SufiCalendarAssistantModule).Assembly
            .GetCustomAttribute<LicenseProductAttribute>();

        stamp.ShouldNotBeNull();
        stamp!.ProductCode.ShouldBe(LicenseProducts.Calendar);
        stamp.ProductCode.ShouldNotBe(LicenseProducts.CalendarHooshvare);
    }
}
