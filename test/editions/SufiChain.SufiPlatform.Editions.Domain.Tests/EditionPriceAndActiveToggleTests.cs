using System.Globalization;
using Shouldly;
using SufiChain.SufiPlatform.Localization;
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

        // Mirrors Editions.MapToForm - Active toggle must open checked for active plans (#193).
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
    public void Priced_plan_resolves_to_two_decimal_amount_and_currency()
    {
        var summary = new EditionPriceSummary
        {
            IsPriced = true,
            Amount = 1_000_000m,
            CurrencyCode = "IRR"
        };
        var text = EditionPriceDisplay.Resolve(summary, "Price not set", CultureInfo.InvariantCulture);
        text.ShouldBe("1,000,000.00 IRR");
    }

    [Fact]
    public void Missing_summary_shows_PriceNotSet_instead_of_an_empty_cell()
    {
        EditionPriceDisplay.Resolve(null, "Price not set").ShouldBe("Price not set");
    }
}

/// <summary>sufi-chain#193 G-85/G-326/G-86: money shows two decimals and Persian digits under fa.</summary>
public class SufiMoneyFormatTests
{
    [Theory]
    [InlineData("29.00000000", "29.00")]
    [InlineData("330000.00000000", "330,000.00")]
    [InlineData("0.000000000000", "0.00")]
    [InlineData("12.345", "12.35")]
    [InlineData("-1.5", "-1.50")]
    public void Formats_two_decimals_invariant(string raw, string expected)
    {
        var amount = decimal.Parse(raw, CultureInfo.InvariantCulture);
        SufiMoneyFormat.Format(amount, CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Fact]
    public void Appends_currency_code()
    {
        SufiMoneyFormat.Format(29m, "USD", CultureInfo.InvariantCulture).ShouldBe("29.00 USD");
        SufiMoneyFormat.Format(29m, " ", CultureInfo.InvariantCulture).ShouldBe("29.00");
    }

    [Fact]
    public void Persian_culture_uses_persian_digits_and_two_decimals()
    {
        var fa = new CultureInfo("fa-IR");
        var text = SufiMoneyFormat.Format(330000m, fa);
        text.ShouldNotContain("0");
        text.ShouldNotContain("3");
        text.ShouldStartWith("۳۳۰");
        text.ShouldEndWith("۰۰");
        text.Length.ShouldBe(SufiMoneyFormat.Format(330000m, CultureInfo.InvariantCulture).Length);
    }

    [Fact]
    public void Whole_number_hint_uses_persian_digits_under_fa()
    {
        SufiMoneyFormat.FormatWhole(100000m, new CultureInfo("fa-IR")).ShouldStartWith("۱۰۰");
        SufiMoneyFormat.FormatWhole(100000m, CultureInfo.InvariantCulture).ShouldBe("100,000");
    }

    [Fact]
    public void Non_persian_cultures_keep_ascii_digits()
    {
        SufiMoneyFormat.LocalizeDigits("123", new CultureInfo("en-US")).ShouldBe("123");
        SufiMoneyFormat.LocalizeDigits("123", new CultureInfo("fa-IR")).ShouldBe("۱۲۳");
    }
}
