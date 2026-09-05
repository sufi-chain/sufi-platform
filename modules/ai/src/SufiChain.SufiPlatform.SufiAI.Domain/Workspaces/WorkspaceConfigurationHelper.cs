using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using SufiChain.SufiPlatform.SufiAI.RAG;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Static helper to build ChatClient, Kernel, and EmbeddingGenerator configurations from Workspace entity.
/// Only OpenAI-compatible providers are supported.
/// </summary>
public static class WorkspaceConfigurationHelper
{
    public static void ConfigureChatClient(ChatClientBuilder builder, Workspace workspace)
    {
        if (workspace.Provider != AIProviderType.OpenAI)
        {
            throw new ArgumentException($"Unsupported provider type: {workspace.Provider}");
        }
    }

    public static void ConfigureKernel(IKernelBuilder builder, Workspace workspace, string? apiKey = null)
    {
        var configuration = workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion);
        ConfigureKernel(
            builder,
            new WorkspaceRuntimeConfiguration
            {
                Workspace = workspace,
                ModelConfiguration = configuration,
                CapabilityType = AICapabilityType.ChatCompletion,
                Provider = workspace.Provider,
                ModelId = configuration?.ModelId ?? workspace.DefaultModel,
                ApiEndpoint = configuration?.ApiEndpoint ?? workspace.ApiBaseUrl,
                ApiKey = apiKey ?? configuration?.ApiKey ?? workspace.ApiKey,
                OpenAIApiMode = configuration?.OpenAIApiMode ?? workspace.OpenAIApiMode,
                InputCostPer1MTokens = configuration?.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens,
                OutputCostPer1MTokens = configuration?.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens,
                IsFallback = configuration == null,
                IsConfigured = !string.IsNullOrWhiteSpace(configuration?.ModelId ?? workspace.DefaultModel),
                IsReady = true
            });
    }

    public static void ConfigureKernel(
        IKernelBuilder builder,
        WorkspaceRuntimeConfiguration configuration)
    {
        EnsureOpenAIProvider(configuration.Provider);
        ConfigureOpenAIKernel(builder, configuration);
    }

    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(
        Workspace workspace,
        EmbedderConfiguration embedderConfiguration)
    {
        ArgumentNullException.ThrowIfNull(embedderConfiguration);

        var provider = embedderConfiguration.Provider;
        EnsureOpenAIProvider(provider);

        var model = string.IsNullOrWhiteSpace(embedderConfiguration.Model)
            ? "text-embedding-3-small"
            : embedderConfiguration.Model;

        return string.Equals(embedderConfiguration.EncodingFormat, "float", StringComparison.OrdinalIgnoreCase)
            ? CreateCompatibleEmbeddingGenerator(workspace, embedderConfiguration, model)
            : CreateOpenAIEmbeddingGenerator(workspace, embedderConfiguration, model);
    }

    private static void EnsureOpenAIProvider(AIProviderType provider)
    {
        if (provider != AIProviderType.OpenAI)
        {
            throw new ArgumentException($"Unsupported provider type: {provider}");
        }
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateOpenAIEmbeddingGenerator(
        Workspace workspace,
        EmbedderConfiguration embedderConfiguration,
        string model)
    {
        var apiKey = embedderConfiguration.ApiKey
            ?? workspace.ApiKey
            ?? throw new InvalidOperationException("OpenAI API key is required");
        var kernelBuilder = Kernel.CreateBuilder();
        var apiBaseUrl = embedderConfiguration.ApiBaseUrl ?? workspace.ApiBaseUrl;

        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = TimeSpan.FromMinutes(5)
            };
            kernelBuilder.AddOpenAITextEmbeddingGeneration(
                modelId: model,
                apiKey: apiKey,
                httpClient: httpClient
            );
        }
        else
        {
            kernelBuilder.AddOpenAITextEmbeddingGeneration(
                modelId: model,
                apiKey: apiKey
            );
        }

        var kernel = kernelBuilder.Build();
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        return new SemanticKernelEmbeddingGenerator(embeddingService);
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateCompatibleEmbeddingGenerator(
        Workspace workspace,
        EmbedderConfiguration configuration,
        string model)
    {
        var apiKey = configuration.ApiKey
            ?? workspace.ApiKey
            ?? throw new InvalidOperationException("OpenAI API key is required");
        var baseUrl = (configuration.ApiBaseUrl ?? workspace.ApiBaseUrl ?? "https://api.openai.com/v1").TrimEnd('/');
        return new CompatibleEmbeddingGenerator(baseUrl, apiKey, model);
    }

    private static void ConfigureOpenAIKernel(
        IKernelBuilder builder,
        WorkspaceRuntimeConfiguration configuration)
    {
        var apiKey = configuration.ApiKey
            ?? throw new InvalidOperationException("OpenAI API key is required");

        if (!string.IsNullOrWhiteSpace(configuration.ApiEndpoint))
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.ApiEndpoint),
                Timeout = TimeSpan.FromMinutes(5)
            };
            builder.AddOpenAIChatCompletion(
                modelId: configuration.ModelId,
                apiKey: apiKey,
                httpClient: httpClient
            );
        }
        else
        {
            builder.AddOpenAIChatCompletion(
                modelId: configuration.ModelId,
                apiKey: apiKey
            );
        }
    }

    private class SemanticKernelEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public SemanticKernelEmbeddingGenerator(ITextEmbeddingGenerationService embeddingService)
        {
            _embeddingService = embeddingService;
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var embeddings = await _embeddingService.GenerateEmbeddingsAsync(values.ToList(), cancellationToken: cancellationToken);
            var result = embeddings.Select(e => new Embedding<float>(e.ToArray())).ToList();

            return new GeneratedEmbeddings<Embedding<float>>(result);
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return serviceKey is null && serviceType.IsInstanceOfType(_embeddingService) ? _embeddingService : null;
        }

        public TService? GetService<TService>(object? key = null) where TService : class
        {
            return GetService(typeof(TService), key) as TService;
        }

        void IDisposable.Dispose()
        {
        }
    }

    private sealed class CompatibleEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _model;

        public CompatibleEmbeddingGenerator(string baseUrl, string apiKey, string model)
        {
            _model = model;
            _httpClient.BaseAddress = new Uri(baseUrl + "/");
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var inputs = values.ToList();
            var payload = JsonSerializer.Serialize(new
            {
                model = _model,
                input = inputs,
                encoding_format = "float"
            });

            using var response = await _httpClient.PostAsync(
                "embeddings",
                new StringContent(payload, Encoding.UTF8, "application/json"),
                cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Embedding provider returned HTTP {(int)response.StatusCode}: {body}");
            }

            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("data", out var data) ||
                data.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Embedding provider returned no data.");
            }

            var result = data.EnumerateArray()
                .OrderBy(item => item.TryGetProperty("index", out var index) ? index.GetInt32() : 0)
                .Select(item => item.GetProperty("embedding")
                    .EnumerateArray()
                    .Select(value => value.GetSingle())
                    .ToArray())
                .Select(vector => new Embedding<float>(vector))
                .ToList();

            if (result.Count != inputs.Count)
            {
                throw new InvalidOperationException(
                    $"Expected {inputs.Count} text embedding(s), but received {result.Count}.");
            }

            return new GeneratedEmbeddings<Embedding<float>>(result);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public TService? GetService<TService>(object? key = null) where TService : class =>
            GetService(typeof(TService), key) as TService;

        void IDisposable.Dispose() => _httpClient.Dispose();
    }
}
