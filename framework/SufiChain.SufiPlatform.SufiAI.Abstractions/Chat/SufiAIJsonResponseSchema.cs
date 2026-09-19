namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>A strict JSON response contract supplied by the application owning the workflow.</summary>
public sealed class SufiAIJsonResponseSchema
{
    public string Name { get; set; } = string.Empty;

    /// <summary>JSON Schema for the final assistant response, independent of tool argument schemas.</summary>
    public string SchemaJson { get; set; } = string.Empty;
}
