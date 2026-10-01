namespace SufiChain.SufiPlatform.SufiAI.Hooshvare;

/// <summary>
/// Stable key for the workspace readiness hooshvare. Keep in sync with <c>PlatformHooshvareKeys</c>.
/// </summary>
public static class WorkspaceReadinessHooshvareKeys
{
    public const string Key = "SufiAI:WorkspaceReadiness";
    public const string LocalizationResourceName = "AI";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string ListWorkspaces = "ai.list_workspaces";
        public const string GetWorkspaceReadiness = "ai.get_workspace_readiness";
        public const string ListModelCapabilities = "ai.list_model_capabilities";
        public const string GetRagAvailability = "ai.get_rag_availability";
        public const string GetMcpCatalogSummary = "ai.get_mcp_catalog_summary";
    }

    public static class Context
    {
        public const string WorkspaceId = "workspaceId";
    }

    public static class Shortcuts
    {
        public const string SummarizeReadiness = "SummarizeReadiness";
        public const string CheckRag = "CheckRag";
    }
}
