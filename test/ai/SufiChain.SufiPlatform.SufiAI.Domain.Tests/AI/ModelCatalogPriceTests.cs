using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class ModelCatalogPriceTests
{
    [Fact]
    public void Ten_percent_markup_stores_one_dollar_ten()
    {
        ModelCatalogPriceCalculator.MarkUpPerMillion(0.000001m, 10).ShouldBe(1.10000000m);
    }

    [Fact]
    public void Zero_percent_keeps_the_catalog_rate()
    {
        ModelCatalogPriceCalculator.MarkUpPerMillion(0.000001m, 0).ShouldBe(1m);
    }

    [Fact]
    public void Missing_catalog_does_not_replace_a_saved_price()
    {
        var route = new AIModelConfiguration(Guid.NewGuid(), Guid.NewGuid(), AICapabilityType.ChatCompletion, "openai/gpt-4o");
        route.ReplaceCatalogPrices(new CatalogPriceQuote(2m, AIPriceUnit.PerMillionTokens, 3m, AIPriceUnit.PerMillionTokens, true));

        ModelCatalogPriceReview.TryGetReplacement(route, null, 10, out _).ShouldBeFalse();
        route.InputPrice.ShouldBe(2m);
    }

    [Fact]
    public void Bare_id_matches_one_suffix_and_nitro_uses_the_base()
    {
        var catalog = new List<ModelCatalogEntry>
        {
            new() { Id = "openai/gpt-4o", PromptPricePerToken = 0.000001m, CompletionPricePerToken = 0.000002m },
            new() { Id = "other/gpt-4o" }
        };

        ModelCatalogMatcher.Match(catalog, "gpt-4o").ShouldBeNull();
        ModelCatalogMatcher.Match(new List<ModelCatalogEntry> { catalog[0] }, "gpt-4o")!.Id.ShouldBe("openai/gpt-4o");
        ModelCatalogMatcher.Match(catalog, "openai/gpt-4o:nitro")!.Id.ShouldBe("openai/gpt-4o");
    }

    [Fact]
    public void Whisper_prompt_is_per_second_and_is_shown_per_minute()
    {
        var quote = CatalogPriceQuote.From(AICapabilityType.AudioTranscription, 0.0001m, 0m, 0);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerMinute);
        quote.InputPrice.ShouldBe(0.006m);
        quote.ShowOutput.ShouldBeFalse();
        quote.OutputPrice.ShouldBeNull();
    }

    [Fact]
    public void Ten_percent_markup_applies_after_the_minute_conversion()
    {
        var quote = CatalogPriceQuote.From(AICapabilityType.AudioTranscription, 0.0001m, 0m, 10);

        quote.InputPrice.ShouldBe(0.0066m);
    }

    [Fact]
    public void Token_transcription_stays_per_million_tokens()
    {
        var quote = CatalogPriceQuote.From(
            AICapabilityType.AudioTranscription,
            0.00000125m,
            0.000005m,
            0);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerMillionTokens);
        quote.InputPrice.ShouldBe(1.25m);
        quote.OutputPrice.ShouldBe(5m);
        quote.ShowOutput.ShouldBeTrue();
    }

    [Fact]
    public void Large_prompt_without_completion_is_per_hour()
    {
        var quote = CatalogPriceQuote.From(AICapabilityType.AudioTranscription, 0.1m, 0m, 0);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerHour);
        quote.InputPrice.ShouldBe(0.1m);
    }

    [Fact]
    public void Character_speech_is_shown_per_million_characters()
    {
        var quote = CatalogPriceQuote.From(AICapabilityType.TextToSpeech, 0.000015m, 0m, 0);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerMillionCharacters);
        quote.InputPrice.ShouldBe(15m);
        quote.ShowOutput.ShouldBeFalse();
    }

    [Fact]
    public void Gemini_speech_with_both_rates_stays_per_token()
    {
        var quote = CatalogPriceQuote.From(AICapabilityType.TextToSpeech, 0.0000005m, 0.000006m, 0);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerMillionTokens);
        quote.InputPrice.ShouldBe(0.5m);
        quote.OutputPrice.ShouldBe(6m);
    }

    [Fact]
    public void Web_search_uses_the_request_rate()
    {
        var quote = CatalogPriceQuote.From(
            AICapabilityType.WebSearch,
            prompt: null,
            completion: null,
            markupPercent: 0,
            webSearch: 0.01m);

        quote.InputUnit.ShouldBe(AIPriceUnit.PerRequest);
        quote.InputPrice.ShouldBe(0.01m);
        quote.ShowOutput.ShouldBeFalse();
    }
}
