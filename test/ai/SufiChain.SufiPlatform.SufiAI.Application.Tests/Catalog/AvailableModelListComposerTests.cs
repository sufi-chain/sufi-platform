using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.BackgroundJobs;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Catalog;

public class AvailableModelListComposerTests
{
    [Fact]
    public void Embeddings_come_from_the_catalog_when_the_workspace_list_is_text_only()
    {
        var workspace = new List<OpenAIModelDto> { new() { Id = "openai/gpt-4o" } };
        var catalog = new List<ModelCatalogEntry>
        {
            new()
            {
                Id = "openai/text-embedding-3-small",
                OutputModalities = new List<string> { "embeddings" },
                PromptPricePerToken = 0.00000002m
            }
        };

        var models = AvailableModelListComposer.Compose(workspace, catalog, AICapabilityType.Embeddings, 0);

        models.Select(model => model.Id).ShouldBe(new[] { "openai/text-embedding-3-small" });
        models[0].SuggestedInputPrice.ShouldBe(0.02m);
        models[0].SuggestedInputPriceUnit.ShouldBe(AIPriceUnit.PerMillionTokens);
    }

    [Fact]
    public void Transcription_catalog_price_is_per_minute_not_per_million_tokens()
    {
        var catalog = new List<ModelCatalogEntry>
        {
            new()
            {
                Id = "openai/whisper-1",
                OutputModalities = new List<string> { "transcription" },
                PromptPricePerToken = 0.0001m,
                CompletionPricePerToken = 0m
            }
        };

        var model = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto>(),
            catalog,
            AICapabilityType.AudioTranscription,
            10).Single();

        model.SuggestedInputPriceUnit.ShouldBe(AIPriceUnit.PerMinute);
        model.SuggestedInputPrice.ShouldBe(0.0066m);
        model.SuggestedInputCostPer1MTokens.ShouldBeNull();
        model.SuggestOutputPrice.ShouldBeFalse();
    }

    [Fact]
    public void Catalog_match_fills_price_and_reasoning_for_a_bare_provider_id()
    {
        var catalog = new List<ModelCatalogEntry>
        {
            new()
            {
                Id = "openai/gpt-6-astra",
                InputModalities = new List<string> { "text", "image", "file" },
                OutputModalities = new List<string> { "text" },
                SupportedParameters = new List<string> { "reasoning" },
                PromptPricePerToken = 0.00001m,
                CompletionPricePerToken = 0.00005m
            }
        };

        var models = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto> { new() { Id = "gpt-6-astra" } },
            catalog,
            AICapabilityType.ChatCompletion,
            0);

        var astra = models.Single();
        astra.SupportsReasoning.ShouldBe(true);
        astra.AcceptsImageInput.ShouldBe(true);
        astra.AcceptsFileInput.ShouldBe(true);
        astra.SuggestedInputCostPer1MTokens.ShouldBe(10m);
        astra.SuggestedOutputCostPer1MTokens.ShouldBe(50m);
    }

    [Fact]
    public void Missing_catalog_does_not_invent_price_or_reasoning()
    {
        var models = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto> { new() { Id = "gpt-6-astra" } },
            catalog: null,
            AICapabilityType.ChatCompletion,
            0);

        var astra = models.Single();
        astra.SupportsReasoning.ShouldNotBe(true);
        astra.SuggestedInputCostPer1MTokens.ShouldBeNull();
        astra.SuggestedOutputCostPer1MTokens.ShouldBeNull();
    }

    [Fact]
    public void Catalog_failure_keeps_the_workspace_chat_list_and_does_not_invent_embeddings()
    {
        var workspace = new List<OpenAIModelDto>
        {
            new() { Id = "gpt-4o" },
            new() { Id = "text-embedding-3-small" }
        };

        AvailableModelListComposer.Compose(workspace, null, AICapabilityType.ChatCompletion, 0)
            .Select(model => model.Id)
            .ShouldBe(new[] { "gpt-4o", "text-embedding-3-small" });
        AvailableModelListComposer.Compose(workspace, null, AICapabilityType.Embeddings, 10)
            .ShouldBeEmpty();
    }

    [Fact]
    public void OpenRouter_list_json_parses_without_callers_knowing_the_url()
    {
        var models = OpenRouterModelCatalogSource.ParseList("""
            {"data":[{"id":"openai/gpt-4o","architecture":{"input_modalities":["text","image","file"],"output_modalities":["text"]},"supported_parameters":["reasoning"],"pricing":{"prompt":"0.000001","completion":"0.000002"}}]}
            """);

        models.Count.ShouldBe(1);
        models[0].OutputModalities.ShouldContain("text");
        models[0].PromptPricePerToken.ShouldBe(0.000001m);
    }

    [Fact]
    public void OpenRouter_model_object_fills_reasoning_context_and_price()
    {
        var catalog = OpenRouterModelCatalogSource.ParseList("""
            {"data":[{
              "id":"openai/gpt-5",
              "context_length":400000,
              "architecture":{"input_modalities":["text","image","file"],"output_modalities":["text"]},
              "pricing":{"prompt":"0.00000125","completion":"0.00001"},
              "supported_parameters":["reasoning","reasoning_effort"],
              "reasoning":{"supported_efforts":["high","medium","low","minimal"],"default_effort":"medium","mandatory":true}
            }]}
            """);

        var entry = catalog.Single();
        entry.ContextLength.ShouldBe(400000);
        entry.ReasoningEfforts.ShouldBe(new[] { "high", "medium", "low", "minimal" });
        entry.DefaultReasoningEffort.ShouldBe("medium");
        ReasoningEffortSelection.ChooseDefault(entry.ReasoningEfforts, entry.DefaultReasoningEffort)
            .ShouldBe("medium");
        ReasoningEffortSelection.ChooseDefault(new[] { "high", "medium", "low", "none" }, "none")
            .ShouldBe("medium");

        var composed = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto> { new() { Id = "openai/gpt-5" } },
            catalog,
            AICapabilityType.ChatCompletion,
            0).Single();
        composed.SupportsReasoning.ShouldBe(true);
        composed.AcceptsImageInput.ShouldBe(true);
        composed.AcceptsFileInput.ShouldBe(true);
        composed.ContextLength.ShouldBe(400000);
        composed.ReasoningEfforts.ShouldBe(new[] { "high", "medium", "low", "minimal" });
        composed.DefaultReasoningEffort.ShouldBe("medium");
        composed.SuggestedInputCostPer1MTokens.ShouldBe(1.25m);
        composed.SuggestedOutputCostPer1MTokens.ShouldBe(10m);
    }

    [Fact]
    public void Account_model_payload_keeps_reasoning_context_and_price_without_another_catalog()
    {
        var model = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto>
            {
                new()
                {
                    Id = "openai/gpt-5",
                    InputModalities = new List<string> { "text", "image", "file" },
                    OutputModalities = new List<string> { "text" },
                    SupportedParameters = new List<string> { "reasoning", "reasoning_effort" },
                    ContextLength = 400000,
                    ReasoningEfforts = new List<string> { "high", "medium", "low" },
                    DefaultReasoningEffort = "medium",
                    PromptPricePerToken = 0.00000125m,
                    CompletionPricePerToken = 0.00001m
                }
            },
            catalog: null,
            AICapabilityType.ChatCompletion,
            0).Single();

        model.SupportsReasoning.ShouldBe(true);
        model.ContextLength.ShouldBe(400000);
        model.SuggestedInputCostPer1MTokens.ShouldBe(1.25m);
        model.SuggestedOutputCostPer1MTokens.ShouldBe(10m);
    }

    [Fact]
    public void OpenRouter_list_keeps_name_and_https_icon_and_drops_other_schemes()
    {
        var models = OpenRouterModelCatalogSource.ParseList("""
            {"data":[
              {"id":"openai/gpt-4o","name":"GPT-4o","icon":"https://cdn.example/openai.svg","architecture":{"output_modalities":["text"]}},
              {"id":"http-icon","icon":"http://cdn.example/x.svg"},
              {"id":"bad-icon","logo":"javascript:alert(1)"}
            ]}
            """);

        models[0].Name.ShouldBe("GPT-4o");
        models[0].IconUrl.ShouldBe("https://cdn.example/openai.svg");
        models[1].IconUrl.ShouldBeNull();
        models[2].IconUrl.ShouldBeNull();

        var composed = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto> { new() { Id = "gpt-4o" } },
            new List<ModelCatalogEntry> { models[0] },
            AICapabilityType.ChatCompletion,
            0);
        composed.Single().DisplayName.ShouldBe("GPT-4o");
        composed.Single().IconUrl.ShouldBe("https://cdn.example/openai.svg");

        var embeddings = AvailableModelListComposer.Compose(
            new List<OpenAIModelDto>(),
            new List<ModelCatalogEntry>
            {
                new()
                {
                    Id = "openai/text-embedding-3-small",
                    Name = "Embedding small",
                    IconUrl = "https://cdn.example/emb.svg",
                    OutputModalities = new List<string> { "embeddings" }
                }
            },
            AICapabilityType.Embeddings,
            0);
        embeddings.Single().DisplayName.ShouldBe("Embedding small");
        embeddings.Single().IconUrl.ShouldBe("https://cdn.example/emb.svg");
    }

    [Fact]
    public void OpenRouter_decisions_rows_stay_on_the_decisions_list_when_the_account_catalog_is_missing()
    {
        var catalog = OpenRouterModelCatalogSource.ParseList("""
            {"data":[{"id":"typesafe/jev-1.13","architecture":{"modality":"text->decisions","input_modalities":["text"],"output_modalities":["decisions"]}}]}
            """);
        catalog.Single().Mode.ShouldBe("decisions");

        AvailableModelListComposer.Compose(
                new List<OpenAIModelDto>(),
                catalog,
                AICapabilityType.Decisions,
                0)
            .Select(model => model.Id)
            .ShouldBe(new[] { "typesafe/jev-1.13" });

        var accountOnly = new List<OpenAIModelDto>
        {
            new()
            {
                Id = "typesafe/jev-1.13",
                Mode = "decisions",
                Modality = "text->decisions",
                InputModalities = new List<string> { "text" },
                OutputModalities = new List<string> { "decisions" }
            }
        };
        AvailableModelListComposer.Compose(accountOnly, null, AICapabilityType.Decisions, 0)
            .Select(model => model.Id)
            .ShouldBe(new[] { "typesafe/jev-1.13" });
    }

    [Fact]
    public void Load_models_and_the_price_worker_depend_on_the_catalog_interface()
    {
        typeof(IModelCatalogSource).Assembly.GetType("SufiChain.SufiPlatform.SufiAI.Workspaces.WorkspaceAppService")!
            .GetConstructors().Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ShouldContain(typeof(IModelCatalogSource));
        typeof(ModelCatalogPriceReviewWorker).GetConstructors().Single()
            .GetParameters()
            .Select(parameter => parameter.ParameterType.Name)
            .ShouldNotContain("OpenRouterModelCatalogSource");

        var workspaceSource = File.ReadAllText(FindSource("sufi-chain/sufi-platform/modules/ai/src/SufiChain.SufiPlatform.SufiAI.Application/Workspaces/WorkspaceAppService.cs"));
        var workerSource = File.ReadAllText(FindSource("sufi-chain/sufi-platform/modules/ai/src/SufiChain.SufiPlatform.SufiAI.Application/BackgroundJobs/ModelCatalogPriceReviewWorker.cs"));
        workspaceSource.ShouldNotContain("openrouter.ai");
        workerSource.ShouldNotContain("openrouter.ai");
        workspaceSource.ShouldContain("IModelCatalogSource");
        workspaceSource.ShouldContain("OpenRouterModelCatalogSource.ModelsUri");
        workspaceSource.ShouldContain("workspace.ApiBaseUrl");
        workerSource.ShouldContain("IModelCatalogSource");
    }

    [Fact]
    public void OpenRouter_connection_catalog_uses_the_workspace_base_url()
    {
        OpenRouterModelCatalogSource.ModelsUri("https://or-gateway.sufichain.com/v1", "output_modalities=all")
            .ShouldBe("https://or-gateway.sufichain.com/v1/models?output_modalities=all");
        OpenRouterModelCatalogSource.ModelsUri("https://or-gateway.sufichain.com/v1/", "output_modalities=decisions")
            .ShouldBe("https://or-gateway.sufichain.com/v1/models?output_modalities=decisions");
        OpenRouterModelCatalogSource.ModelsUri("https://openrouter.ai/api/v1")
            .ShouldBe("https://openrouter.ai/api/v1/models");
        new Uri(OpenRouterModelCatalogSource.CatalogBaseAddress(null), OpenRouterModelCatalogSource.DecisionsListPath)
            .ShouldBe(new Uri("https://openrouter.ai/api/v1/models?output_modalities=decisions"));
        new Uri(OpenRouterModelCatalogSource.CatalogBaseAddress("https://or-gateway.sufichain.com"), OpenRouterModelCatalogSource.DecisionsListPath)
            .ShouldBe(new Uri("https://or-gateway.sufichain.com/v1/models?output_modalities=decisions"));
    }

    [Fact]
    public async Task Two_callers_share_one_catalog_fetch()
    {
        var provider = new CountingCatalogProvider();
        var source = new CachingModelCatalogSource(
            new IModelCatalogProvider[] { provider },
            new MemoryCatalogCache(),
            new DistributedRefreshGate(new SingleFlightLock()),
            Options.Create(new AIOptions
            {
                ProviderCatalogCacheSeconds = 300
            }),
            NullLogger<CachingModelCatalogSource>.Instance);

        var first = source.GetModelsAsync(OpenRouterModelCatalogSource.NameValue);
        var second = source.GetModelsAsync(OpenRouterModelCatalogSource.NameValue);
        var results = await Task.WhenAll(first, second);

        provider.ListCalls.ShouldBe(1);
        results[0]!.Single().Id.ShouldBe("openai/catalog-model");
        results[1]!.Single().Id.ShouldBe("openai/catalog-model");
    }

    [Fact]
    public async Task Price_worker_writes_nothing_when_the_catalog_returns_no_models()
    {
        var services = new RecordingServiceProvider();
        var worker = new CatalogProbeWorker(new SingleScopeFactory(services));

        await worker.RunAsync(services);

        services.Requested.ShouldContain(typeof(IModelCatalogSource));
        services.Requested.ShouldNotContain(typeof(IAIModelConfigurationRepository));
    }

    private static string FindSource(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }

    private sealed class CountingCatalogProvider : IModelCatalogProvider
    {
        public int ListCalls;

        public string Name => OpenRouterModelCatalogSource.NameValue;

        public async Task<IReadOnlyList<ModelCatalogEntry>?> TryListAsync(CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref ListCalls);
            await Task.Delay(30, cancellationToken);
            return new List<ModelCatalogEntry>
            {
                new() { Id = "openai/catalog-model", OutputModalities = new List<string> { "text" } }
            };
        }

        public Task<ModelCatalogLookup> TryFindAsync(string modelId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ModelCatalogLookup { Status = ModelCatalogLookupStatus.Unknown });
        }
    }

    private sealed class EmptyCatalogSource : IModelCatalogSource
    {
        public Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            return GetModelsAsync(catalogName: null, cancellationToken);
        }

        public Task<IReadOnlyList<ModelCatalogEntry>?> GetModelsAsync(string? catalogName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ModelCatalogEntry>?>(null);
        }

        public Task<ModelCatalogEntry?> FindAsync(string modelId, CancellationToken cancellationToken = default)
        {
            return FindAsync(modelId, catalogName: null, cancellationToken);
        }

        public Task<ModelCatalogEntry?> FindAsync(string modelId, string? catalogName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ModelCatalogEntry?>(null);
        }
    }

    private sealed class CatalogProbeWorker : ModelCatalogPriceReviewWorker
    {
        public CatalogProbeWorker(IServiceScopeFactory scopes)
            : base(new AbpAsyncTimer(), scopes)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, object>>()).Returns(NullLogger.Instance);
            LazyServiceProvider = lazy;
        }

        public Task RunAsync(IServiceProvider services)
        {
            return DoWorkAsync(new PeriodicBackgroundWorkerContext(services));
        }
    }

    private sealed class SingleScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _services;

        public SingleScopeFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IServiceScope CreateScope()
        {
            return new Scope(_services);
        }

        private sealed class Scope : IServiceScope
        {
            public Scope(IServiceProvider services)
            {
                ServiceProvider = services;
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
            }
        }
    }

    private sealed class RecordingServiceProvider : IServiceProvider
    {
        public List<Type> Requested { get; } = new();

        public object? GetService(Type serviceType)
        {
            Requested.Add(serviceType);
            if (serviceType == typeof(IModelCatalogSource))
            {
                return new EmptyCatalogSource();
            }

            if (serviceType == typeof(HostPriceMarkup))
            {
                return new HostPriceMarkup(null!, null!);
            }

            if (serviceType == typeof(IServiceScopeFactory))
            {
                return new SingleScopeFactory(this);
            }

            if (serviceType == typeof(ICurrentTenant))
            {
                return Substitute.For<ICurrentTenant>();
            }

            return null;
        }
    }

    private sealed class SingleFlightLock : IAbpDistributedLock
    {
        private readonly SemaphoreSlim _gate = new(1, 1);

        public async Task<IAbpDistributedLockHandle?> TryAcquireAsync(
            string name,
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            return new Handle(_gate);
        }

        private sealed class Handle : IAbpDistributedLockHandle
        {
            private readonly SemaphoreSlim _gate;

            public Handle(SemaphoreSlim gate)
            {
                _gate = gate;
            }

            public ValueTask DisposeAsync()
            {
                _gate.Release();
                return ValueTask.CompletedTask;
            }
        }
    }

    /// <summary>
    /// In-memory stand-in for <see cref="IDistributedCache{TCacheItem}"/>.
    /// Signatures follow https://abp.io/docs/api/10.4/Volo.Abp.Caching.IDistributedCache-1.html.
    /// </summary>
    private sealed class MemoryCatalogCache : IDistributedCache<ModelCatalogCacheItem>
    {
        private readonly Dictionary<string, ModelCatalogCacheItem> _items = new();

        private ModelCatalogCacheItem? Find(string key)
        {
            return _items.TryGetValue(key, out var item) ? item : null;
        }

        public IDistributedCache<ModelCatalogCacheItem, string> InternalCache => this;

        public ModelCatalogCacheItem Get(string key, bool? hideErrors = null, bool considerUow = false)
        {
            return Find(key)!;
        }

        public Task<ModelCatalogCacheItem?> GetAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return Task.FromResult(Find(key));
        }

        public KeyValuePair<string, ModelCatalogCacheItem?>[] GetMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
        {
            return keys.Select(key => new KeyValuePair<string, ModelCatalogCacheItem?>(key, Find(key))).ToArray();
        }

        public Task<KeyValuePair<string, ModelCatalogCacheItem?>[]> GetManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return Task.FromResult(GetMany(keys, hideErrors, considerUow));
        }

        public ModelCatalogCacheItem GetOrAdd(string key, Func<ModelCatalogCacheItem> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false)
        {
            return Find(key) ?? factory();
        }

        public async Task<ModelCatalogCacheItem?> GetOrAddAsync(string key, Func<Task<ModelCatalogCacheItem>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return Find(key) ?? await factory();
        }

        public KeyValuePair<string, ModelCatalogCacheItem?>[] GetOrAddMany(IEnumerable<string> keys, Func<IEnumerable<string>, List<KeyValuePair<string, ModelCatalogCacheItem>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false)
        {
            return GetMany(keys);
        }

        public Task<KeyValuePair<string, ModelCatalogCacheItem?>[]> GetOrAddManyAsync(IEnumerable<string> keys, Func<IEnumerable<string>, Task<List<KeyValuePair<string, ModelCatalogCacheItem>>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return Task.FromResult(GetMany(keys));
        }

        public void Refresh(string key, bool? hideErrors = null)
        {
        }

        public Task RefreshAsync(string key, bool? hideErrors = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void RefreshMany(IEnumerable<string> keys, bool? hideErrors = null)
        {
        }

        public Task RefreshManyAsync(IEnumerable<string> keys, bool? hideErrors = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key, bool? hideErrors = null, bool considerUow = false)
        {
            _items.Remove(key);
        }

        public Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            Remove(key, hideErrors, considerUow);
            return Task.CompletedTask;
        }

        public void RemoveMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
        {
            foreach (var key in keys)
            {
                _items.Remove(key);
            }
        }

        public Task RemoveManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            RemoveMany(keys, hideErrors, considerUow);
            return Task.CompletedTask;
        }

        public void Set(string key, ModelCatalogCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
        {
            _items[key] = value;
        }

        public Task SetAsync(string key, ModelCatalogCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            Set(key, value, options, hideErrors, considerUow);
            return Task.CompletedTask;
        }

        public void SetMany(IEnumerable<KeyValuePair<string, ModelCatalogCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
        {
            foreach (var item in items)
            {
                _items[item.Key] = item.Value;
            }
        }

        public Task SetManyAsync(IEnumerable<KeyValuePair<string, ModelCatalogCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            SetMany(items, options, hideErrors, considerUow);
            return Task.CompletedTask;
        }
    }
}
