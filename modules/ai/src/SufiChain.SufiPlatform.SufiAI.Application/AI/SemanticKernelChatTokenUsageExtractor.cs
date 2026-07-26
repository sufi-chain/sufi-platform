using System.Collections.Generic;
using System.Reflection;
using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Reads token usage from Semantic Kernel <see cref="ChatMessageContent"/> responses.
/// OpenAI connector stores counts under <c>Metadata["Usage"]</c> as <c>ChatTokenUsage</c>
/// (<c>InputTokenCount</c>/<c>OutputTokenCount</c>), not as flat int metadata keys.
/// </summary>
public static class SemanticKernelChatTokenUsageExtractor
{
    public static SufiAITokenUsage Extract(ChatMessageContent response)
    {
        var fromMetadata = Extract(response.Metadata);
        if (fromMetadata.HasUsage)
        {
            return fromMetadata;
        }

        if (response.InnerContent == null)
        {
            return new SufiAITokenUsage();
        }

        // ChatCompletion.Usage or equivalent nested on InnerContent
        var usageProperty = response.InnerContent.GetType()
            .GetProperty("Usage", BindingFlags.Instance | BindingFlags.Public);
        if (usageProperty?.GetValue(response.InnerContent) is { } nestedUsage)
        {
            var fromInnerUsage = FromUsageObject(nestedUsage);
            if (fromInnerUsage.HasUsage)
            {
                return fromInnerUsage;
            }
        }

        return Create(
            ReadIntProperty(response.InnerContent, "InputTokens", "PromptTokens", "InputTokenCount"),
            ReadIntProperty(response.InnerContent, "OutputTokens", "CompletionTokens", "OutputTokenCount"),
            ReadIntProperty(response.InnerContent, "TotalTokens", "TotalTokenCount"));
    }

    public static SufiAITokenUsage Extract(IReadOnlyDictionary<string, object?>? metadata)
    {
        if (metadata == null)
        {
            return new SufiAITokenUsage();
        }

        foreach (var key in new[] { "Usage", "usage", "TokenUsage", "token_usage" })
        {
            if (metadata.TryGetValue(key, out var usage) && usage != null)
            {
                var fromUsage = FromUsageObject(usage);
                if (fromUsage.HasUsage)
                {
                    return fromUsage;
                }
            }
        }

        return Create(
            ReadIntMetadata(metadata, "InputTokens", "PromptTokens", "input_tokens", "prompt_tokens"),
            ReadIntMetadata(metadata, "OutputTokens", "CompletionTokens", "output_tokens", "completion_tokens"),
            ReadIntMetadata(metadata, "TotalTokens", "total_tokens"));
    }

    private static SufiAITokenUsage FromUsageObject(object usage) =>
        Create(
            ReadIntProperty(usage, "InputTokens", "PromptTokens", "InputTokenCount"),
            ReadIntProperty(usage, "OutputTokens", "CompletionTokens", "OutputTokenCount"),
            ReadIntProperty(usage, "TotalTokens", "TotalTokenCount"));

    private static SufiAITokenUsage Create(int? inputTokens, int? outputTokens, int? totalTokens)
    {
        return new SufiAITokenUsage
        {
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = totalTokens ?? (inputTokens.HasValue || outputTokens.HasValue
                ? (inputTokens ?? 0) + (outputTokens ?? 0)
                : null)
        };
    }

    private static int? ReadIntMetadata(IReadOnlyDictionary<string, object?> metadata, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (metadata.TryGetValue(key, out var value) && TryConvertInt(value, out var result))
            {
                return result;
            }
        }

        return null;
    }

    private static int? ReadIntProperty(object source, params string[] propertyNames)
    {
        var sourceType = source.GetType();
        foreach (var propertyName in propertyNames)
        {
            var property = sourceType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && TryConvertInt(property.GetValue(source), out var result))
            {
                return result;
            }
        }

        return null;
    }

    private static bool TryConvertInt(object? value, out int result)
    {
        switch (value)
        {
            case int intValue:
                result = intValue;
                return true;
            case long longValue when longValue <= int.MaxValue:
                result = (int)longValue;
                return true;
            case null:
                result = 0;
                return false;
            default:
                return int.TryParse(value.ToString(), out result);
        }
    }
}
