using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Some models emit tool calls as plain-text markup instead of structured tool calls.
/// Parse reliable invoke markup into tool calls; otherwise strip the markup so the turn can end.
/// </summary>
public static partial class TextToolCallMarkup
{
    public const string NoticeLocalizationKey = "AI:TextToolCallMarkupStripped";

    [GeneratedRegex(
        @"<(?:dots_function_call|function_call|tool_call|minimax:tool_call)\b[^>]*>[\s\S]*?</(?:dots_function_call|function_call|tool_call|minimax:tool_call)>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BlockMarkupRegex();

    [GeneratedRegex(
        @"<invoke\b[^>]*\bname\s*=\s*[""'](?<name>[^""']+)[""'][^>]*>(?<body>[\s\S]*?)</invoke>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InvokeRegex();

    [GeneratedRegex(
        @"<(?:parameter|arg|argument)\b[^>]*\bname\s*=\s*[""'](?<name>[^""']+)[""'][^>]*>(?<value>[\s\S]*?)</(?:parameter|arg|argument)>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ParameterRegex();

    public sealed record ParsedCall(string Name, IReadOnlyDictionary<string, object?> Arguments);

    public sealed record Result(
        string CleanText,
        IReadOnlyList<ParsedCall> Calls,
        bool HadMarkup,
        bool RequiresNotice);

    public static bool ContainsMarkup(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return BlockMarkupRegex().IsMatch(text) || InvokeRegex().IsMatch(text);
    }

    public static Result Process(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new Result(string.Empty, Array.Empty<ParsedCall>(), false, false);
        }

        if (!ContainsMarkup(text))
        {
            return new Result(text, Array.Empty<ParsedCall>(), false, false);
        }

        var calls = new List<ParsedCall>();
        foreach (Match invoke in InvokeRegex().Matches(text))
        {
            var name = NormalizeToolName(invoke.Groups["name"].Value);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var args = ParseArguments(invoke.Groups["body"].Value);
            calls.Add(new ParsedCall(name, args));
        }

        var clean = BlockMarkupRegex().Replace(text, string.Empty);
        clean = InvokeRegex().Replace(clean, string.Empty);
        clean = Regex.Replace(clean, @"\n{3,}", "\n\n").Trim();

        var parseable = calls.Count > 0;
        return new Result(
            clean,
            calls,
            HadMarkup: true,
            RequiresNotice: !parseable);
    }

    public static string NormalizeToolName(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();
        if (value.StartsWith("mcp__", StringComparison.OrdinalIgnoreCase))
        {
            // mcp__iranmcp__get_latest_news -> prefer external-style when possible
            var parts = value.Split("__", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 3)
            {
                return parts[^1];
            }
        }

        return value;
    }

    private static IReadOnlyDictionary<string, object?> ParseArguments(string body)
    {
        var args = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (Match parameter in ParameterRegex().Matches(body))
        {
            args[parameter.Groups["name"].Value.Trim()] = UnwrapValue(parameter.Groups["value"].Value.Trim());
        }

        if (args.Count > 0)
        {
            return args;
        }

        var trimmed = body.Trim();
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in document.RootElement.EnumerateObject())
                    {
                        args[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                    }
                }
            }
            catch (JsonException)
            {
                // leave empty; caller treats as unparseable when name-only without args is still usable
            }
        }

        return args;
    }

    private static object? UnwrapValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            return value[1..^1];
        }

        if (bool.TryParse(value, out var boolean))
        {
            return boolean;
        }

        if (long.TryParse(value, out var number))
        {
            return number;
        }

        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var real))
        {
            return real;
        }

        return value;
    }
}
