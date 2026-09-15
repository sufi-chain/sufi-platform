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
    public const string ApiKeyRequired = "AI:ApiKeyRequired";
    public const string NoModelConfigured = "AI:NoModelConfigured";
    public const string ProviderNotSupported = "AI:ProviderNotSupported";
    public const string CapabilityNotSupported = "AI:CapabilityNotSupported";
    public const string McpWorkspaceNotReady = "AI:McpWorkspaceNotReady";
    public const string McpRequiresChatCompletions = "AI:McpRequiresChatCompletions";
    public const string McpProviderNotSupported = "AI:McpProviderNotSupported";
    public const string WorkspaceGuardrailExceeded = "AI:WorkspaceGuardrailExceeded";
    public const string InheritedWorkspaceReadOnly = "AI:InheritedWorkspaceReadOnly";
    public const string InheritedWorkspaceMustBelongToTenant = "AI:InheritedWorkspaceMustBelongToTenant";
    public const string WorkspaceAssignmentHostOnly = "AI:WorkspaceAssignmentHostOnly";
    public const string TenantRequired = "AI:TenantRequired";
    public const string TenantNotFound = "AI:TenantNotFound";
    public const string CannotAssignInheritedWorkspace = "AI:CannotAssignInheritedWorkspace";
    public const string CannotAssignTenantWorkspace = "AI:CannotAssignTenantWorkspace";
    public const string WorkspaceAlreadyAssignedToTenant = "AI:WorkspaceAlreadyAssignedToTenant";
    public const string WorkspaceNotInherited = "AI:WorkspaceNotInherited";
    public const string AssignmentNotFound = "AI:AssignmentNotFound";
    public const string InvalidMaxContextTokens = "AI:InvalidMaxContextTokens";
    public const string ModelRouteNotFound = "AI:ModelRouteNotFound";
    public const string ModelRouteNotSelectable = "AI:ModelRouteNotSelectable";
    public const string ModelRouteCapabilityMismatch = "AI:ModelRouteCapabilityMismatch";
    public const string ModelRouteRequiresChatCompletions = "AI:ModelRouteRequiresChatCompletions";
    public const string ModelRouteOutsideWorkspace = "AI:ModelRouteOutsideWorkspace";
    public const string ModelRouteNotAllowedForCopilot = "AI:ModelRouteNotAllowedForCopilot";
    public const string ModelRouteCopilotNotFound = "AI:ModelRouteCopilotNotFound";
}
