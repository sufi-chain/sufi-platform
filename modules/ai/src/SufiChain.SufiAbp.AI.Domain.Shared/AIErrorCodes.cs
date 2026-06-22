namespace SufiChain.SufiAbp.AI;

public static class AIErrorCodes
{
    public const string WorkspaceNameAlreadyExists = "AI:WorkspaceNameAlreadyExists";
    public const string WorkspaceNotFound = "AI:WorkspaceNotFound";
    public const string WorkspaceNotActive = "AI:WorkspaceNotActive";
    public const string InvalidProviderConfiguration = "AI:InvalidProviderConfiguration";
    public const string DocumentSourceNotFound = "AI:DocumentSourceNotFound";
    public const string EmbeddingGenerationFailed = "AI:EmbeddingGenerationFailed";
    public const string VectorSearchFailed = "AI:VectorSearchFailed";
    
    // MCP Tool Error Codes
    public const string MCPToolNotFound = "AI:MCPToolNotFound";
    public const string MCPToolExecutionFailed = "AI:MCPToolExecutionFailed";
    public const string MCPServerNotFound = "AI:MCPServerNotFound";
    public const string MCPServerConnectionFailed = "AI:MCPServerConnectionFailed";
    public const string MCPServerNotEnabled = "AI:MCPServerNotEnabled";
    public const string MCPToolParameterBindingFailed = "AI:MCPToolParameterBindingFailed";
    public const string MCPToolPermissionDenied = "AI:MCPToolPermissionDenied";
}
