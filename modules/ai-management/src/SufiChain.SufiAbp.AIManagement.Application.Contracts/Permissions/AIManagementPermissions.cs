namespace SufiChain.SufiAbp.AIManagement.Permissions;

public static class AIManagementPermissions
{
    public const string GroupName = "AIManagement";

    public static class Workspaces
    {
        public const string Default = GroupName + ".Workspaces";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class RAG
    {
        public const string Default = GroupName + ".RAG";
        public const string Manage = Default + ".Manage";
        public const string Index = Default + ".Index";
    }

    public static class TestChat
    {
        public const string Default = GroupName + ".TestChat";
    }
    
    public static class MCPTools
    {
        public const string Default = GroupName + ".MCPTools";
        public const string Execute = Default + ".Execute";
        public const string Manage = Default + ".Manage";
    }
    
    public static class MCPServers
    {
        public const string Default = GroupName + ".MCPServers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    
    public static class AI
    {
        public const string Default = GroupName + ".AI";
        public const string Chat = Default + ".Chat";
        public const string Audio = Default + ".Audio";
        public const string Vision = Default + ".Vision";
        public const string Embeddings = Default + ".Embeddings";
        public const string FunctionCalling = Default + ".FunctionCalling";
        public const string ManageConfigurations = Default + ".ManageConfigurations";
        public const string ViewUsage = Default + ".ViewUsage";
    }
}
