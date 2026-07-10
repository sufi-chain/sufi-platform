using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Helper for building multi-culture seed text dictionaries (fa-first).
/// </summary>
public static class MultiCultureText
{
    public static Dictionary<string, string> Create(string fa, string en, string ar, string es)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = fa,
            ["en"] = en,
            ["ar"] = ar,
            ["es"] = es
        };
    }
}
