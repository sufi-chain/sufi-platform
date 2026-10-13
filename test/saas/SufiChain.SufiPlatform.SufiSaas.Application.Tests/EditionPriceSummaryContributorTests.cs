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
        var priced = new Plan(Guid.NewGuid(), "GROWTH", "Growth", pricedEditionId, price: 1_000_000m, currencyCode: "IRR");
        var unpriced = new Plan(Guid.NewGuid(), "STARTER", "Starter", unpricedEditionId, price: 0m, currencyCode: "IRR", allowUnpaidUse: true);

        var plans = Substitute.For<IPlanRepository>();
        plans.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([priced, unpriced]);

        var contributor = new SaasEditionPriceSummaryContributor(plans);
        var dtos = new List<EditionsEditionDto>
        {
            new() { Id = pricedEditionId, Code = "GROWTH" },
            new() { Id = unpricedEditionId, Code = "STARTER" },
            new() { Id = Guid.NewGuid(), Code = "ORPHAN" }
        };

        await contributor.ContributeAsync(dtos);

        dtos[0].PriceSummary.ShouldNotBeNull();
        dtos[0].PriceSummary!.IsPriced.ShouldBeTrue();
        dtos[0].PriceSummary.Amount.ShouldBe(1_000_000m);
        dtos[0].PriceSummary.CurrencyCode.ShouldBe("IRR");

        dtos[1].PriceSummary.ShouldNotBeNull();
        dtos[1].PriceSummary!.IsPriced.ShouldBeFalse();

        dtos[2].PriceSummary.ShouldBeNull();
    }
}
