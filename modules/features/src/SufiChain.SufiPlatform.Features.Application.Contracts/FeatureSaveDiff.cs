#nullable enable
using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Picks the features whose edited value differs from the value loaded into the editor.
/// Saving only those keeps one save from rewriting (and raising change events for) every feature.
/// </summary>
public static class FeatureSaveDiff
{
    public static List<UpdateFeatureDto> Changed(
        IEnumerable<KeyValuePair<string, string?>> edited,
        IReadOnlyDictionary<string, string?> saved)
    {
        ArgumentNullException.ThrowIfNull(edited);
        ArgumentNullException.ThrowIfNull(saved);

        var changed = new List<UpdateFeatureDto>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (name, value) in edited)
        {
            if (!seen.Add(name))
            {
                continue;
            }

            if (saved.TryGetValue(name, out var original) && string.Equals(original, value, StringComparison.Ordinal))
            {
                continue;
            }

            changed.Add(new UpdateFeatureDto { Name = name, Value = value! });
        }

        return changed;
    }
}
