namespace SufiChain.SufiAbp.AIManagement;

public static class AIManagementErrorCodes
{
    public const string WorkspaceNameAlreadyExists = "AIManagement:WorkspaceNameAlreadyExists";
    public const string WorkspaceNotFound = "AIManagement:WorkspaceNotFound";
    public const string WorkspaceNotActive = "AIManagement:WorkspaceNotActive";
    public const string InvalidProviderConfiguration = "AIManagement:InvalidProviderConfiguration";
    public const string DocumentSourceNotFound = "AIManagement:DocumentSourceNotFound";
    public const string EmbeddingGenerationFailed = "AIManagement:EmbeddingGenerationFailed";
    public const string VectorSearchFailed = "AIManagement:VectorSearchFailed";
    
    // MCP Tool Error Codes
    public const string MCPToolNotFound = "AIManagement:MCPToolNotFound";
    public const string MCPToolExecutionFailed = "AIManagement:MCPToolExecutionFailed";
    public const string MCPServerNotFound = "AIManagement:MCPServerNotFound";
    public const string MCPServerConnectionFailed = "AIManagement:MCPServerConnectionFailed";
    public const string MCPServerNotEnabled = "AIManagement:MCPServerNotEnabled";
    public const string MCPToolParameterBindingFailed = "AIManagement:MCPToolParameterBindingFailed";
    public const string MCPToolPermissionDenied = "AIManagement:MCPToolPermissionDenied";
}
