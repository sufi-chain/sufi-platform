namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// The in-process generator key includes the model and dimensions, so a route
/// change cannot reuse a generator that was built for the previous model.
/// </summary>
public static class EmbedderGeneratorCacheKey
{
    public static string Build(string canonicalId, Guid configurationId, string? model, int dimensions)
    {
        return string.Join(
            ':',
            canonicalId,
            configurationId.ToString("N"),
            model?.Trim() ?? string.Empty,
            dimensions.ToString());
    }
}
