using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class SharedWorkspaceRoutePriceReviewTests
{
    [Fact]
    public void Zero_route_price_is_a_calculated_cost_of_zero_until_the_floor_replaces_it()
    {
        var route = ChatRoute(input: 0m, output: 0m);
        var recorder = new TestableAIUsageRecorder();
        var before = recorder.Calculate(Configuration(route), 1_000_000, 1_000_000, null);
        before.IsCostCalculated.ShouldBeTrue();
        before.EstimatedCost.ShouldBe(0m);

        var replaced = SharedWorkspaceRoutePriceReview.TryGetReplacement(
            route,
            catalog: null,
            markupPercent: 0,
            Floor(input: 1m, output: 2m),
            out var quote);
        replaced.ShouldBeTrue();
        route.ReplaceCatalogPrices(quote);

        var after = recorder.Calculate(Configuration(route), 1_000_000, 1_000_000, null);
        after.IsCostCalculated.ShouldBeTrue();
        after.EstimatedCost.ShouldBe(3m);
        SharedWorkspaceRoutePriceReview.TryGetReplacement(
            route,
            catalog: null,
            markupPercent: 0,
            Floor(input: 1m, output: 2m),
            out _).ShouldBeFalse();
    }

    [Fact]
    public void Positive_catalog_price_wins_over_the_floor_and_a_stored_zero()
    {
        var route = ChatRoute(input: 0m, output: 0m, modelId: "qwen/qwen-2.5-72b-instruct");
        var catalog = new List<ModelCatalogEntry>
        {
            new()
            {
                Id = "qwen/qwen-2.5-72b-instruct",
                PromptPricePerToken = 0.00000036m,
                CompletionPricePerToken = 0.0000004m
            }
        };

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog, 0, Floor(9m, 9m), out var quote).ShouldBeTrue();

        quote.InputPrice.ShouldBe(0.36m);
        quote.OutputPrice.ShouldBe(0.4m);
        quote.InputUnit.ShouldBe(AIPriceUnit.PerMillionTokens);
    }

    [Fact]
    public void Stored_positive_price_survives_a_zero_catalog_quote()
    {
        var route = ChatRoute(input: 2.5m, output: 4m, modelId: "openrouter/free");
        var catalog = new List<ModelCatalogEntry>
        {
            new() { Id = "openrouter/free", PromptPricePerToken = 0m, CompletionPricePerToken = 0m }
        };

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog, 0, Floor(1m, 2m), out _).ShouldBeFalse();
        route.InputPrice.ShouldBe(2.5m);
        route.OutputPrice.ShouldBe(4m);
    }

    [Fact]
    public void Disabled_floor_clears_a_zero_instead_of_keeping_a_fake_price()
    {
        var route = ChatRoute(input: 0m, output: 0m);
        var floor = new SharedWorkspaceRoutePriceSchedule().For(AICapabilityType.ChatCompletion);

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog: null, 0, floor, out var quote).ShouldBeTrue();
        quote.InputPrice.ShouldBeNull();
        quote.OutputPrice.ShouldBeNull();
    }

    [Fact]
    public void Other_workspaces_still_take_a_zero_catalog_price()
    {
        var route = ChatRoute(input: 2.5m, output: 4m, modelId: "openrouter/free");
        var catalog = new List<ModelCatalogEntry>
        {
            new() { Id = "openrouter/free", PromptPricePerToken = 0m, CompletionPricePerToken = 0m }
        };

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog, 0, sharedFloor: null, out var quote).ShouldBeTrue();
        quote.InputPrice.ShouldBe(0m);
        quote.OutputPrice.ShouldBe(0m);
    }

    [Fact]
    public void Positive_audio_catalog_price_is_kept_per_minute()
    {
        var workspace = new Workspace(Guid.NewGuid(), "shared", AIProviderType.OpenRouter, "chat");
        var route = workspace.AddModelConfiguration(
            AICapabilityType.AudioTranscription,
            "nvidia/nemotron-3.5-asr-streaming-multilingual-0.6b");
        var catalog = new List<ModelCatalogEntry>
        {
            new()
            {
                Id = route.ModelId,
                PromptPricePerToken = 0.00000333m,
                CompletionPricePerToken = 0m
            }
        };
        var floor = new SharedWorkspaceRoutePriceSchedule { AudioPerMinute = 9m }
            .For(AICapabilityType.AudioTranscription);

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog, 0, floor, out var quote).ShouldBeTrue();
        quote.InputUnit.ShouldBe(AIPriceUnit.PerMinute);
        quote.InputPrice.ShouldBe(0.0001998m);
        quote.OutputPrice.ShouldBeNull();
    }

    [Fact]
    public void Image_floor_sets_a_per_image_output_and_leaves_input_empty()
    {
        var workspace = new Workspace(Guid.NewGuid(), "shared", AIProviderType.OpenRouter, "chat");
        var route = workspace.AddModelConfiguration(AICapabilityType.ImageGeneration, "image-model", inputPrice: 0m);
        var floor = new SharedWorkspaceRoutePriceSchedule { ImagePerImage = 0.04m }
            .For(AICapabilityType.ImageGeneration);

        SharedWorkspaceRoutePriceReview.TryGetReplacement(route, catalog: null, 0, floor, out var quote).ShouldBeTrue();
        quote.InputPrice.ShouldBeNull();
        quote.OutputPrice.ShouldBe(0.04m);
        quote.OutputUnit.ShouldBe(AIPriceUnit.PerImage);
    }

    private static SharedWorkspaceRoutePriceFloor Floor(decimal input, decimal output)
    {
        return new SharedWorkspaceRoutePriceSchedule
        {
            ChatInputPerMillionTokens = input,
            ChatOutputPerMillionTokens = output
        }.For(AICapabilityType.ChatCompletion);
    }

    private static AIModelConfiguration ChatRoute(decimal input, decimal output, string modelId = "openrouter/free")
    {
        var workspace = new Workspace(Guid.NewGuid(), "shared", AIProviderType.OpenRouter, modelId);
        return workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            modelId,
            inputPrice: input,
            outputPrice: output);
    }

    private static WorkspaceRuntimeConfiguration Configuration(AIModelConfiguration route)
    {
        return new WorkspaceRuntimeConfiguration
        {
            Workspace = new Workspace(route.WorkspaceId, "shared", AIProviderType.OpenRouter, route.ModelId),
            ModelConfiguration = route,
            CapabilityType = route.CapabilityType,
            Provider = AIProviderType.OpenRouter,
            ModelId = route.ModelId,
            InputPrice = route.InputPrice,
            InputPriceUnit = route.InputPriceUnit,
            OutputPrice = route.OutputPrice,
            OutputPriceUnit = route.OutputPriceUnit,
            ModelConfigurationId = route.Id,
            IsConfigured = true,
            IsReady = true
        };
    }

    private sealed class TestableAIUsageRecorder : AIUsageRecorder
    {
        public TestableAIUsageRecorder()
            : base(Substitute.For<IAIUsageLogRepository>(), NullLogger<AIUsageRecorder>.Instance)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService(Arg.Any<IGuidGenerator>())
                .Returns(callInfo => callInfo.ArgAt<IGuidGenerator>(0));
            LazyServiceProvider = lazy;
        }

        public (decimal EstimatedCost, bool IsCostCalculated, string? CostCalculationNote) Calculate(
            WorkspaceRuntimeConfiguration configuration,
            int? inputTokens,
            int? outputTokens,
            int? totalTokens)
        {
            var result = CalculateCost(configuration, inputTokens, outputTokens, totalTokens);
            return (result.EstimatedCost, result.IsCostCalculated, result.CostCalculationNote);
        }
    }
}

public class SharedWorkspaceRoutePriceReaderTests
{
    [Fact]
    public async Task Explicit_zero_disables_the_floor_and_a_blank_setting_uses_the_definition_default()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns(call => call.Arg<string>() switch
        {
            SharedWorkspaceRoutePriceSettings.ChatInputPerMillionTokens => "1.25",
            SharedWorkspaceRoutePriceSettings.ChatOutputPerMillionTokens => "0",
            SharedWorkspaceRoutePriceSettings.AudioPerMinute => "nope",
            _ => null
        });

        var schedule = await new SharedWorkspaceRoutePriceReader(settings).GetAsync();

        schedule.ChatInputPerMillionTokens.ShouldBe(1.25m);
        schedule.ChatOutputPerMillionTokens.ShouldBeNull();
        schedule.AudioPerMinute.ShouldBeNull();
        schedule.EmbeddingsInputPerMillionTokens.ShouldBe(
            decimal.Parse(SharedWorkspaceRoutePriceSettings.DefaultEmbeddingsInputPerMillionTokens, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Setting_defaults_are_positive_and_registered_for_the_host()
    {
        var context = new DictionarySettingDefinitionContext();
        new SharedWorkspaceRoutePriceSettingDefinitionProvider().Define(context);

        context.Settings.Count.ShouldBe(SharedWorkspaceRoutePriceSettings.All.Length);
        foreach (var (name, value) in SharedWorkspaceRoutePriceSettings.All)
        {
            var definition = context.Settings[name];
            definition.DefaultValue.ShouldBe(value);
            decimal.Parse(definition.DefaultValue!, CultureInfo.InvariantCulture).ShouldBeGreaterThan(0m);
            definition.Providers.ShouldContain(GlobalSettingValueProvider.ProviderName);
            definition.Providers.ShouldNotContain(TenantSettingValueProvider.ProviderName);
            definition.IsVisibleToClients.ShouldBeFalse();
        }
    }

    private sealed class DictionarySettingDefinitionContext : ISettingDefinitionContext
    {
        public Dictionary<string, SettingDefinition> Settings { get; } = new();

        public SettingDefinition GetOrNull(string name)
        {
            return Settings.TryGetValue(name, out var definition) ? definition : null!;
        }

        public IReadOnlyList<SettingDefinition> GetAll()
        {
            return Settings.Values.ToList();
        }

        public void Add(params SettingDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                Settings[definition.Name] = definition;
            }
        }
    }
}
