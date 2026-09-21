using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class EmbeddingModelDefaultsTests
{
    [Theory]
    [InlineData("openrouter/nvidia/nemotron-3-embed-1b:free", "https://ai.sufichain.com/v1")]
    [InlineData("openrouter/nvidia/nemotron-3-embed-1b:free", "https://openrouter.ai/api/v1")]
    [InlineData("nvidia/nemotron-3-embed-1b:free", null)]
    [InlineData("nvidia/nv-embed-v1", "https://example.invalid/v1")]
    [InlineData("Nemotron-3-Embed-1B", "https://ai.sufichain.com/v1")]
    public void Nvidia_embedding_models_use_float_encoding(string modelId, string? apiBaseUrl)
    {
        EmbeddingModelDefaults.GetEncodingFormat(modelId, apiBaseUrl).ShouldBe("float");
        EmbeddingModelDefaults.RequiresCustomTransport(modelId, apiBaseUrl).ShouldBeTrue();
    }

    [Theory]
    [InlineData("openrouter/openai/text-embedding-3-small", "https://ai.sufichain.com/v1")]
    [InlineData("text-embedding-3-small", "https://api.openai.com/v1")]
    [InlineData("text-embedding-3-small", "https://openrouter.ai/api/v1")]
    [InlineData("oc/nemotron-3-ultra-free", "https://ai.sufichain.com/v1")]
    public void Non_nvidia_embedding_models_do_not_force_float_encoding(string modelId, string? apiBaseUrl)
    {
        EmbeddingModelDefaults.GetEncodingFormat(modelId, apiBaseUrl).ShouldBeNull();
        EmbeddingModelDefaults.RequiresCustomTransport(modelId, apiBaseUrl).ShouldBeFalse();
    }

    [Theory]
    [InlineData("openai/text-embedding-3-small", 1536)]
    [InlineData("openrouter/openai/text-embedding-3-small", 1536)]
    [InlineData("openai/text-embedding-3-small:batch", 1536)]
    [InlineData("openai/text-embedding-ada-002", 1536)]
    [InlineData("openai/text-embedding-3-large", 3072)]
    [InlineData("openai/text-embedding-3-large:batch", 3072)]
    [InlineData("openrouter/nvidia/nemotron-3-embed-1b:free", 2048)]
    [InlineData("nvidia/nemotron-3-embed-1b:free", 2048)]
    [InlineData("nvidia/llama-nemotron-embed-vl-1b-v2:free", 2048)]
    [InlineData("voyageai/voyage-4", 2048)]
    [InlineData("voyageai/voyage-4-lite", 2048)]
    [InlineData("voyageai/voyage-4-large", 2048)]
    [InlineData("voyageai/voyage-code-4", 2048)]
    [InlineData("voyageai/voyage-multimodal-3.5", 2048)]
    [InlineData("qwen/qwen3-embedding-8b", 4096)]
    [InlineData("qwen/qwen3-embedding-4b", 2560)]
    [InlineData("perplexity/pplx-embed-v1-4b", 2560)]
    [InlineData("perplexity/pplx-embed-v1-0.6b", 1024)]
    [InlineData("google/gemini-embedding-2", 3072)]
    [InlineData("google/gemini-embedding-2:batch", 3072)]
    [InlineData("google/gemini-embedding-2-preview", 3072)]
    [InlineData("google/gemini-embedding-001", 3072)]
    [InlineData("mistralai/mistral-embed-2312", 1024)]
    [InlineData("mistralai/codestral-embed-2505", 1536)]
    [InlineData("liquid/lfm-2.5-embedding-350m:free", 1024)]
    [InlineData("thenlper/gte-base", 768)]
    [InlineData("thenlper/gte-large", 1024)]
    [InlineData("intfloat/e5-base-v2", 768)]
    [InlineData("intfloat/e5-large-v2", 1024)]
    [InlineData("intfloat/multilingual-e5-large", 1024)]
    [InlineData("baai/bge-base-en-v1.5", 768)]
    [InlineData("baai/bge-large-en-v1.5", 1024)]
    [InlineData("baai/bge-m3", 1024)]
    [InlineData("sentence-transformers/all-minilm-l6-v2", 384)]
    [InlineData("sentence-transformers/all-minilm-l12-v2", 384)]
    [InlineData("sentence-transformers/paraphrase-minilm-l6-v2", 384)]
    [InlineData("sentence-transformers/all-mpnet-base-v2", 768)]
    [InlineData("sentence-transformers/multi-qa-mpnet-base-dot-v1", 768)]
    public void OpenRouter_embedding_models_use_native_dimensions(string modelId, int dimensions)
    {
        EmbeddingModelDefaults.GetDimensions(modelId).ShouldBe(dimensions);
    }

    [Fact]
    public void Unknown_embedding_models_use_the_1536_fallback()
    {
        EmbeddingModelDefaults.GetDimensions("unknown-embedder").ShouldBe(1536);
        EmbeddingModelDefaults.GetDimensions(null).ShouldBe(1536);
        EmbeddingModelDefaults.IsKnownModel("unknown-embedder").ShouldBeFalse();
        EmbeddingModelDefaults.IsKnownModel("openai/text-embedding-3-small").ShouldBeTrue();
    }

    [Fact]
    public void Nemotron_embed_max_input_tokens_are_4096()
    {
        EmbeddingModelDefaults.GetMaxInputTokens("openrouter/nvidia/nemotron-3-embed-1b:free")
            .ShouldBe(4096);
        EmbeddingModelDefaults.SupportsInputTruncation("openrouter/nvidia/nemotron-3-embed-1b:free")
            .ShouldBeTrue();
    }

    [Fact]
    public void OpenAI_embedding_max_input_tokens_are_8191()
    {
        EmbeddingModelDefaults.GetMaxInputTokens("openrouter/openai/text-embedding-3-small")
            .ShouldBe(8191);
        EmbeddingModelDefaults.SupportsInputTruncation("text-embedding-3-small")
            .ShouldBeFalse();
    }
}
