using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Maps a model id to a known author family. Unknown ids return null so the UI can fall back to a monogram.
/// </summary>
public static class ModelFamilyKey
{
    public const string OpenAI = "openai";
    public const string Anthropic = "anthropic";
    public const string Google = "google";
    public const string Meta = "meta";
    public const string Mistral = "mistral";
    public const string Nvidia = "nvidia";
    public const string Qwen = "qwen";
    public const string DeepSeek = "deepseek";
    public const string Cohere = "cohere";
    public const string Xai = "xai";

    private static readonly Dictionary<string, string> AuthorAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["openai"] = OpenAI,
        ["anthropic"] = Anthropic,
        ["google"] = Google,
        ["google-vertex"] = Google,
        ["gemini"] = Google,
        ["meta"] = Meta,
        ["meta-llama"] = Meta,
        ["mistral"] = Mistral,
        ["mistralai"] = Mistral,
        ["nvidia"] = Nvidia,
        ["qwen"] = Qwen,
        ["deepseek"] = DeepSeek,
        ["cohere"] = Cohere,
        ["xai"] = Xai,
        ["x-ai"] = Xai
    };

    public static string? Resolve(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        var id = modelId.Trim();
        var slash = id.IndexOf('/');
        if (slash > 0 && slash < id.Length - 1)
        {
            var author = id[..slash];
            var rest = id[(slash + 1)..];
            if (author.Equals("openrouter", StringComparison.OrdinalIgnoreCase) && rest.Contains('/'))
            {
                return Resolve(rest);
            }

            return MapAuthor(author);
        }

        return MapBare(id);
    }

    public static string Monogram(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return "?";
        }

        var token = modelId.Trim().Split('/', 2)[0];
        var letters = new string(Array.FindAll(token.ToCharArray(), char.IsLetterOrDigit));
        if (letters.Length == 0)
        {
            return "?";
        }

        return letters.Length == 1
            ? letters.ToUpperInvariant()
            : letters[..2].ToUpperInvariant();
    }

    private static string? MapAuthor(string author)
    {
        return AuthorAliases.TryGetValue(author, out var family) ? family : null;
    }

    private static string? MapBare(string modelId)
    {
        var id = modelId.Split(':', 2)[0];
        if (id.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("chatgpt", StringComparison.OrdinalIgnoreCase) ||
            IsOpenAIReasoning(id))
        {
            return OpenAI;
        }

        if (id.StartsWith("claude", StringComparison.OrdinalIgnoreCase))
        {
            return Anthropic;
        }

        if (id.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
        {
            return Google;
        }

        if (id.StartsWith("llama", StringComparison.OrdinalIgnoreCase))
        {
            return Meta;
        }

        if (id.StartsWith("nemotron", StringComparison.OrdinalIgnoreCase))
        {
            return Nvidia;
        }

        return null;
    }

    private static bool IsOpenAIReasoning(string id)
    {
        if (id.Length < 2 || (id[0] != 'o' && id[0] != 'O'))
        {
            return false;
        }

        if (id[1] is not ('1' or '3' or '4'))
        {
            return false;
        }

        return id.Length == 2 || id[2] is '-' or '.' or ':';
    }
}
