namespace SufiChain.SufiPlatform.SufiAI.Configuration;

/// <summary>
/// Configuration for seeding the default AI workspace during initial data migration.
/// </summary>
public class DefaultWorkspaceSeedOptions
{
    /// <summary>
    /// Workspace name. Defaults to <see cref="AIWorkspaceNames.Default"/>.
    /// </summary>
    public string Name { get; set; } = AIWorkspaceNames.Default;

    public AIProviderType Provider { get; set; } = AIProviderType.OpenAI;

    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Embedding model used for RAG indexing. Defaults to text-embedding-3-small.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    public string? ApiKey { get; set; }

    public string? ApiBaseUrl { get; set; } = "https://api.openai.com/v1";

    public string? SystemPrompt { get; set; }

    public float Temperature { get; set; } = 0.7f;

    public int MaxContextTokens { get; set; } = 200000;

    public OpenAIApiMode OpenAIApiMode { get; set; } = OpenAIApiMode.ChatCompletions;
}
