using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Picks the route default from OpenRouter <c>reasoning.supported_efforts</c>.
/// Provider <c>default_effort</c> wins, except <c>none</c>. Otherwise medium, then the middle value.
/// </summary>
public static class ReasoningEffortSelection
{
    public static string? ChooseDefault(IReadOnlyList<string>? efforts, string? providerDefault)
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

        if (!string.IsNullOrWhiteSpace(providerDefault))
        {
            var match = list.FirstOrDefault(effort =>
                effort.Equals(providerDefault.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match != null && !match.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return match;
            }
        }

        var medium = list.FirstOrDefault(effort => effort.Equals("medium", StringComparison.OrdinalIgnoreCase));
        return medium ?? list[list.Count / 2];
    }
}
