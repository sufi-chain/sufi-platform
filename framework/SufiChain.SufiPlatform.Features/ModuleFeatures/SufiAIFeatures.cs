namespace SufiChain.SufiPlatform.SufiAI.Features;

/// <summary>
/// Shared feature names for Sufi AI capabilities.
/// </summary>
public static class SufiAIFeatures
{
    public const string GroupName = "SufiAI";

    /// <summary>
    /// Master switch for AI Management and runtime AI services.
    /// </summary>
    public const string Enable = GroupName + ".Enable";

    /// <summary>
    /// AI workspace configuration and provider management.
    /// </summary>
    public const string Workspaces = GroupName + ".Workspaces";

    /// <summary>
    /// Text chat completion.
    /// </summary>
    public const string Chat = GroupName + ".Chat";

    /// <summary>
    /// Audio transcription and text-to-speech.
    /// </summary>
    public const string Audio = GroupName + ".Audio";

    /// <summary>
    /// Vision/image analysis.
    /// </summary>
    public const string Vision = GroupName + ".Vision";

    /// <summary>
    /// Embedding generation.
    /// </summary>
    public const string Embeddings = GroupName + ".Embeddings";

    /// <summary>
    /// Retrieval-augmented generation and vector search.
    /// </summary>
    public const string RAG = GroupName + ".RAG";

    /// <summary>
    /// Model Context Protocol tools, servers, and function calling.
    /// </summary>
    public const string MCP = GroupName + ".MCP";

    /// <summary>
    /// Usage logging, cost tracking, and analytics.
    /// </summary>
    public const string UsageAnalytics = GroupName + ".UsageAnalytics";

    /// <summary>
    /// File Manager integration for generated/processed AI files.
    /// </summary>
    public const string FileManagerIntegration = GroupName + ".FileManagerIntegration";
}
