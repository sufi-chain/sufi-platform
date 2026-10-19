using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#340: turns a business error from an internal MCP tool into text the model can act on: what was
/// refused and what to call instead. Without it the model only saw a bare code such as
/// <c>CMS:PageDraftNotEmpty</c> and told the user it had hit a technical problem. Modules register one for
/// their own error codes; the text should be localized to the current UI culture.
/// </summary>
public interface IMcpToolErrorDescriber
{
    /// <summary>Model-facing text for <paramref name="code"/>, or null when this describer does not know the code.</summary>
    string? Describe(string code, IReadOnlyDictionary<string, object?> data);
}
