using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public class CalendarLicenseAliasTests
{
    [Fact]
    public void License_with_only_the_old_id_passes_the_Calendar_check()
    {
        var entitlement = new LicenseEntitlement
        {
            Products = new[] { LicenseProducts.CalendarHooshvare }
        };

        entitlement.AllowsProduct(LicenseProducts.Calendar).ShouldBeTrue();
        entitlement.AllowsProduct(LicenseProducts.CalendarHooshvare).ShouldBeTrue();
    }

    [Fact]
    public void License_with_Calendar_and_without_the_old_id_passes_both_checks()
    {
        var entitlement = new LicenseEntitlement
        {
            Products = new[] { LicenseProducts.Calendar }
        };

        entitlement.AllowsProduct(LicenseProducts.Calendar).ShouldBeTrue();
        entitlement.AllowsProduct(LicenseProducts.CalendarHooshvare).ShouldBeTrue();
        entitlement.Products.ShouldNotContain(LicenseProducts.CalendarHooshvare);
    }

    [Fact]
    public void An_unrelated_product_does_not_allow_Calendar()
    {
        var entitlement = new LicenseEntitlement
        {
            Products = new[] { LicenseProducts.SufiCom }
        };

        entitlement.AllowsProduct(LicenseProducts.Calendar).ShouldBeFalse();
        entitlement.AllowsProduct(LicenseProducts.CalendarHooshvare).ShouldBeFalse();
    }
}
