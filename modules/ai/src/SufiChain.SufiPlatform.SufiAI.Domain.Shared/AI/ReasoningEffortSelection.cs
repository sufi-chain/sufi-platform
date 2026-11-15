using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Picks the route default from OpenRouter <c>reasoning.supported_efforts</c>.
/// Provider <c>default_effort</c> wins, except <c>none</c>. Otherwise medium, then the middle value.
/// NVIDIA models and the free router reject High, so those routes never keep it as the default.
/// </summary>
public static class ReasoningEffortSelection
{
    public static bool OmitsElevatedReasoning(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return false;
        }

        var id = modelId.Trim();
        return id.Equals("openrouter/free", StringComparison.OrdinalIgnoreCase)
               || id.StartsWith("nvidia/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsElevated(string? effort) =>
        !string.IsNullOrWhiteSpace(effort)
        && (effort.Trim().Equals("high", StringComparison.OrdinalIgnoreCase)
            || effort.Trim().Equals("xhigh", StringComparison.OrdinalIgnoreCase));

    public static string? ChooseDefault(IReadOnlyList<string>? efforts, string? providerDefault, string? modelId = null)
    {
        var list = (efforts ?? Array.Empty<string>())
            .Where(effort => !string.IsNullOrWhiteSpace(effort))
            .Select(effort => effort.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (list.Count == 0)
        {
            return null;
        }

        string? chosen = null;
        if (!string.IsNullOrWhiteSpace(providerDefault))
        {
            var match = list.FirstOrDefault(effort =>
                effort.Equals(providerDefault.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null && !match.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                chosen = match;
            }
        }

        chosen ??= list.FirstOrDefault(effort => effort.Equals("medium", StringComparison.OrdinalIgnoreCase))
                   ?? list[list.Count / 2];
        return OmitsElevatedReasoning(modelId) ? SafeEffort(list, chosen) : chosen;
    }

    /// <summary>
    /// Keeps a non-elevated effort. High and xhigh become medium, then low, minimal, or none.
    /// When the route lists only elevated efforts, reasoning is omitted.
    /// </summary>
    public static string? SafeEffort(IReadOnlyList<string> efforts, string? chosen)
    {
        if (!IsElevated(chosen) && !string.IsNullOrWhiteSpace(chosen))
        {
            var kept = efforts.FirstOrDefault(effort =>
                effort.Equals(chosen.Trim(), StringComparison.OrdinalIgnoreCase));
            if (kept != null)
            {
                return kept;
            }
        }

        foreach (var name in new[] { "medium", "low", "minimal", "none" })
        {
            var match = efforts.FirstOrDefault(effort => effort.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
