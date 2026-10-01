using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class ModelFamilyKeyTests
{
    [Theory]
    [InlineData("openai/gpt-4.1", ModelFamilyKey.OpenAI)]
    [InlineData("gpt-4.1-mini", ModelFamilyKey.OpenAI)]
    [InlineData("chatgpt-4o-latest", ModelFamilyKey.OpenAI)]
    [InlineData("o3-mini", ModelFamilyKey.OpenAI)]
    [InlineData("o4", ModelFamilyKey.OpenAI)]
    [InlineData("claude-sonnet-4.5", ModelFamilyKey.Anthropic)]
    [InlineData("anthropic/claude-sonnet-4.5", ModelFamilyKey.Anthropic)]
    [InlineData("gemini-2.5-flash", ModelFamilyKey.Google)]
    [InlineData("google/gemini-2.5-pro", ModelFamilyKey.Google)]
    [InlineData("llama-3.3-70b", ModelFamilyKey.Meta)]
    [InlineData("meta-llama/Llama-3.3-70B-Instruct", ModelFamilyKey.Meta)]
    [InlineData("nvidia/nemotron-3-ultra", ModelFamilyKey.Nvidia)]
    [InlineData("nemotron-3-ultra", ModelFamilyKey.Nvidia)]
    [InlineData("mistralai/mistral-small", ModelFamilyKey.Mistral)]
    [InlineData("qwen/qwen2.5-72b", ModelFamilyKey.Qwen)]
    [InlineData("deepseek/deepseek-chat", ModelFamilyKey.DeepSeek)]
    [InlineData("cohere/command-r", ModelFamilyKey.Cohere)]
    [InlineData("x-ai/grok-2", ModelFamilyKey.Xai)]
    [InlineData("openrouter/openai/gpt-4.1", ModelFamilyKey.OpenAI)]
    [InlineData("openrouter/nvidia/nemotron-3-ultra:free", ModelFamilyKey.Nvidia)]
    public void Known_ids_resolve_to_a_family(string modelId, string family)
    {
        ModelFamilyKey.Resolve(modelId).ShouldBe(family);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("oc/nemotron-3-ultra-free")]
    [InlineData("text-embedding-3-small")]
    [InlineData("openrouter/auto")]
    public void Unknown_ids_have_no_family(string? modelId)
    {
        ModelFamilyKey.Resolve(modelId).ShouldBeNull();
    }

    [Theory]
    [InlineData("oc/nemotron-3-ultra-free", "OC")]
    [InlineData("local/default", "LO")]
    [InlineData("a", "A")]
    [InlineData(null, "?")]
    public void Monogram_uses_the_first_token(string? modelId, string monogram)
    {
        ModelFamilyKey.Monogram(modelId).ShouldBe(monogram);
    }

    [Theory]
    [InlineData("https://cdn.example/openai.svg", "https://cdn.example/openai.svg")]
    [InlineData(" https://cdn.example/openai.svg ", "https://cdn.example/openai.svg")]
    [InlineData("http://cdn.example/openai.svg", null)]
    [InlineData("javascript:alert(1)", null)]
    [InlineData("/relative.svg", null)]
    [InlineData(null, null)]
    public void Icon_url_accepts_only_https(string? value, string? expected)
    {
        ModelCatalogIconUrl.Normalize(value).ShouldBe(expected);
    }
}
