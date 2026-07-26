namespace SufiChain.SufiPlatform.SufiAI;

public static class AIErrorCodes
{
    public const string WorkspaceNameAlreadyExists = "AI:WorkspaceNameAlreadyExists";
    public const string WorkspaceNotFound = "AI:WorkspaceNotFound";
    public const string WorkspaceNotActive = "AI:WorkspaceNotActive";
    public const string InvalidProviderConfiguration = "AI:InvalidProviderConfiguration";
    public const string EmbeddingsModelNotConfigured = "AI:EmbeddingsModelNotConfigured";
    public const string EmbeddingsCredentialsMissing = "AI:EmbeddingsCredentialsMissing";
    public const string VectorStoreConfigurationMissing = "AI:VectorStoreConfigurationMissing";
    public const string VectorStoreProviderNotSupported = "AI:VectorStoreProviderNotSupported";
    public const string VectorStoreConfigurationInvalid = "AI:VectorStoreConfigurationInvalid";
    public const string RagUnavailable = "AI:RagUnavailable";
    public const string DocumentSourceNotFound = "AI:DocumentSourceNotFound";
    public const string DocumentIndexingFailed = "AI:DocumentIndexingFailed";
    public const string EmbeddingGenerationFailed = "AI:EmbeddingGenerationFailed";
    public const string VectorStoreWriteFailed = "AI:VectorStoreWriteFailed";
    public const string VectorSearchFailed = "AI:VectorSearchFailed";
    
    // MCP Tool Error Codes
    public const string MCPToolNotFound = "AI:MCPToolNotFound";
    public const string MCPToolExecutionFailed = "AI:MCPToolExecutionFailed";
    public const string MCPServerNotFound = "AI:MCPServerNotFound";
    public const string MCPServerConnectionFailed = "AI:MCPServerConnectionFailed";
    public const string MCPServerNotEnabled = "AI:MCPServerNotEnabled";
    public const string MCPToolParameterBindingFailed = "AI:MCPToolParameterBindingFailed";
    public const string MCPToolPermissionDenied = "AI:MCPToolPermissionDenied";
    public const string MCPDuplicateToolName = "AI:MCPDuplicateToolName";
    public const string MCPHttpTransportNotImplemented = "AI:MCPHttpTransportNotImplemented";

    public const string ProviderRequestFailed = "AI:ProviderRequestFailed";
}
