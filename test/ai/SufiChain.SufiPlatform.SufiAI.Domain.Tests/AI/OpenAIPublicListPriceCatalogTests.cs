using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class OpenAIPublicListPriceCatalogTests
{
    [Fact]
    public void Should_Resolve_Exact_Chat_Model()
    {
        OpenAIPublicListPriceCatalog.TryGet("gpt-4o-mini", out var price).ShouldBeTrue();
        price.InputCostPer1MTokens.ShouldBe(0.15m);
        price.OutputCostPer1MTokens.ShouldBe(0.60m);
    }

    [Fact]
    public void Should_Resolve_Dated_Snapshot_To_Family_Rate()
    {
        OpenAIPublicListPriceCatalog.TryGet("gpt-4o-mini-2024-07-18", out var price).ShouldBeTrue();
        price.InputCostPer1MTokens.ShouldBe(0.15m);
        price.OutputCostPer1MTokens.ShouldBe(0.60m);
    }

    [Fact]
    public void Should_Keep_Explicit_Dated_Exception()
    {
        OpenAIPublicListPriceCatalog.TryGet("gpt-4o-2024-05-13", out var price).ShouldBeTrue();
        price.InputCostPer1MTokens.ShouldBe(5.00m);
        price.OutputCostPer1MTokens.ShouldBe(15.00m);
    }

    [Fact]
    public void Should_Resolve_Embedding_Input_Only()
    {
        OpenAIPublicListPriceCatalog.TryGet("text-embedding-3-small", out var price).ShouldBeTrue();
        price.InputCostPer1MTokens.ShouldBe(0.02m);
        price.OutputCostPer1MTokens.ShouldBeNull();
    }

    [Fact]
    public void Should_Return_False_For_Unknown_Model()
    {
        OpenAIPublicListPriceCatalog.TryGet("not-a-real-model", out _).ShouldBeFalse();
    }
}
