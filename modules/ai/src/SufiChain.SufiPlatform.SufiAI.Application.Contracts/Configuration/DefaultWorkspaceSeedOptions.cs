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

    /// <summary>
    /// Chat completion model. Override via <c>SufiAI:DefaultWorkspace:Model</c>.
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Embedding model used for RAG indexing. Override via <c>SufiAI:DefaultWorkspace:EmbeddingModel</c>.
    /// Leave empty to avoid creating an embeddings configuration.
    /// </summary>
    public string EmbeddingModel { get; set; } = "";

    /// <summary>
    /// Vision analysis model. Override via <c>SufiAI:DefaultWorkspace:VisionModel</c>.
    /// Leave empty to avoid creating a vision configuration.
    /// </summary>
    public string VisionModel { get; set; } = "";

    /// <summary>
    /// Audio transcription model. Override via <c>SufiAI:DefaultWorkspace:AudioModel</c>.
    /// Leave empty to avoid creating an audio transcription configuration.
    /// </summary>
    public string AudioModel { get; set; } = "";

    /// <summary>
    /// Text-to-speech model. Override via <c>SufiAI:DefaultWorkspace:TtsModel</c>.
    /// Leave empty to avoid creating a text-to-speech configuration.
    /// </summary>
    public string TtsModel { get; set; } = "";

    /// <summary>
    /// Image generation model. Override via <c>SufiAI:DefaultWorkspace:ImageModel</c>.
    /// Leave empty to avoid creating an image generation configuration.
    /// </summary>
    public string ImageModel { get; set; } = "";

    /// <summary>
    /// Prefer <c>appsettings.secrets.json</c> for this value.
    /// </summary>
    public string? ApiKey { get; set; }

    public string? ApiBaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Retained for configuration compatibility. Default workspace seeding keeps the prompt module-neutral.
    /// </summary>
    public string? SystemPrompt { get; set; }

    public float Temperature { get; set; } = 0.7f;

    public int MaxContextTokens { get; set; } = 200000;

    public OpenAIApiMode OpenAIApiMode { get; set; } = OpenAIApiMode.ChatCompletions;
}
