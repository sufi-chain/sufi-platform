using System.Globalization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Editions;
using SufiChain.SufiPlatform.SufiSaas.Editions;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using Xunit;
using EditionsEditionDto = SufiChain.SufiPlatform.Editions.EditionDto;

namespace SufiChain.SufiPlatform.SufiSaas;

public class EditionPriceSummaryContributorTests
{
    [Fact]
    public async Task Contributor_marks_unpriced_plans_and_formats_priced_ones()
    {
        var pricedEditionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var unpricedEditionId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var priced = new Plan(Guid.NewGuid(), "GROWTH", "Growth", pricedEditionId, price: 29m, currencyCode: "USD");
        priced.SetMonthlyPricing(1_000_000m, "IRR");
        var legacyOnlyEditionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var legacyOnly = new Plan(Guid.NewGuid(), "SCALE", "Scale", legacyOnlyEditionId, price: 99m, currencyCode: "USD");
        var unpriced = new Plan(Guid.NewGuid(), "STARTER", "Starter", unpricedEditionId, price: 0m, currencyCode: "IRR", allowUnpaidUse: true);

        var plans = Substitute.For<IPlanRepository>();
        plans.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([priced, unpriced, legacyOnly]);

        var contributor = new SaasEditionPriceSummaryContributor(plans);
        var dtos = new List<EditionsEditionDto>
        {
            new() { Id = pricedEditionId, Code = "GROWTH" },
            new() { Id = unpricedEditionId, Code = "STARTER" },
            new() { Id = Guid.NewGuid(), Code = "ORPHAN" },
            new() { Id = legacyOnlyEditionId, Code = "SCALE" }
        };

        await contributor.ContributeAsync(dtos);

        dtos[0].PriceSummary.ShouldNotBeNull();
        dtos[0].PriceSummary!.IsPriced.ShouldBeTrue();
        dtos[0].PriceSummary.Amount.ShouldBe(1_000_000m);
        dtos[0].PriceSummary.CurrencyCode.ShouldBe("IRR");

        dtos[1].PriceSummary.ShouldNotBeNull();
        dtos[1].PriceSummary!.IsPriced.ShouldBeFalse();

        dtos[2].PriceSummary.ShouldBeNull();

        // The editions list shows the plan base price when monthly and yearly are empty.
        dtos[3].PriceSummary.ShouldNotBeNull();
        dtos[3].PriceSummary!.IsPriced.ShouldBeTrue();
        dtos[3].PriceSummary.Amount.ShouldBe(99m);
        dtos[3].PriceSummary.CurrencyCode.ShouldBe("USD");
        EditionPriceDisplay.Resolve(dtos[3].PriceSummary, "Price not set", CultureInfo.InvariantCulture)
            .ShouldBe("99.00 USD");
    }

    [Fact]
    public async Task Contributor_matches_a_plan_by_edition_code_when_the_id_was_recreated()
    {
        var storedEditionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var listedEditionId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var plan = new Plan(Guid.NewGuid(), "GROWTH", "Growth", storedEditionId, price: 29m, currencyCode: "USD");
        plan.SetMonthlyPricing(1_000_000m, "IRR");
        var plans = Substitute.For<IPlanRepository>();
        plans.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([plan]);

        var contributor = new SaasEditionPriceSummaryContributor(plans);
        var dtos = new List<EditionsEditionDto>
        {
            new() { Id = listedEditionId, Code = "GROWTH" }
        };

        await contributor.ContributeAsync(dtos);

        dtos[0].PriceSummary.ShouldNotBeNull();
        dtos[0].PriceSummary!.IsPriced.ShouldBeTrue();
        dtos[0].PriceSummary.Amount.ShouldBe(1_000_000m);
        dtos[0].PriceSummary.CurrencyCode.ShouldBe("IRR");
        EditionPriceDisplay.Resolve(dtos[0].PriceSummary, "Price not set", CultureInfo.InvariantCulture)
            .ShouldBe("1,000,000.00 IRR");
    }
}
