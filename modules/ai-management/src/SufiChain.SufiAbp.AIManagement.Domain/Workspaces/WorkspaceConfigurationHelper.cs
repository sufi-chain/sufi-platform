using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Embeddings;

namespace SufiChain.SufiAbp.AIManagement.Workspaces;

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
        EnsureOpenAIProvider(workspace.Provider);
        ConfigureOpenAIKernel(builder, workspace, apiKey);
    }

    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(Workspace workspace, string? embeddingModel = null)
    {
        var config = ParseEmbedderConfig(workspace);
        var provider = config?.Provider ?? workspace.Provider;
        EnsureOpenAIProvider(provider);

        var model = embeddingModel ?? config?.Model ?? "text-embedding-3-small";
        return CreateOpenAIEmbeddingGenerator(workspace, model);
    }

    private static EmbedderConfig? ParseEmbedderConfig(Workspace workspace)
    {
        return string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson)
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<EmbedderConfig>(workspace.EmbedderConfigJson);
    }

    private static void EnsureOpenAIProvider(AIProviderType provider)
    {
        if (provider != AIProviderType.OpenAI)
        {
            throw new ArgumentException($"Unsupported provider type: {provider}");
        }
    }

    private static IEmbeddingGenerator<string, Embedding<float>> CreateOpenAIEmbeddingGenerator(Workspace workspace, string model)
    {
        var apiKey = workspace.ApiKey ?? throw new InvalidOperationException("OpenAI API key is required");
        var kernelBuilder = Kernel.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(workspace.ApiBaseUrl))
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(workspace.ApiBaseUrl) };
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

    private static void ConfigureOpenAIKernel(IKernelBuilder builder, Workspace workspace, string? apiKeyOverride)
    {
        var apiKey = apiKeyOverride ?? workspace.ApiKey ?? throw new InvalidOperationException("OpenAI API key is required");

        if (!string.IsNullOrWhiteSpace(workspace.ApiBaseUrl))
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(workspace.ApiBaseUrl) };
            builder.AddOpenAIChatCompletion(
                modelId: workspace.Model,
                apiKey: apiKey,
                httpClient: httpClient
            );
        }
        else
        {
            builder.AddOpenAIChatCompletion(
                modelId: workspace.Model,
                apiKey: apiKey
            );
        }
    }

    private class EmbedderConfig
    {
        public AIProviderType Provider { get; set; }
        public string? Model { get; set; }
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
}
