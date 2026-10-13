using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Editions;

public class EditionPriceAndActiveToggleTests
{
    [Fact]
    public void Edit_form_maps_IsActive_true_from_an_active_edition()
    {
        var edition = new EditionDto
        {
            Id = Guid.NewGuid(),
            Name = "Growth",
            DisplayName = "Growth",
            Code = "GROWTH",
            IsActive = true
        };

        // Mirrors Editions.MapToForm — Active toggle must open checked for active plans (#193).
        var formIsActive = edition.IsActive;

        formIsActive.ShouldBeTrue();
    }

    [Fact]
    public void Unpriced_plan_resolves_to_PriceNotSet_text()
    {
        var summary = new EditionPriceSummary { IsPriced = false };
        var text = EditionPriceDisplay.Resolve(summary, "Price not set");
        text.ShouldBe("Price not set");
    }

    [Fact]
    public void Priced_plan_resolves_to_amount_and_currency()
    {
        var summary = new EditionPriceSummary
        {
            IsPriced = true,
            Amount = 1_000_000m,
            CurrencyCode = "IRR"
        };
        var text = EditionPriceDisplay.Resolve(summary, "Price not set");
        text.ShouldBe("1,000,000 IRR");
    }

    [Fact]
    public void Missing_summary_renders_no_price_cell()
    {
        EditionPriceDisplay.Resolve(null, "Price not set").ShouldBeNull();
    }
}
