using SufiChain.SufiAbp.AIManagement.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.AIManagement.Permissions;

public class AIManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var aiManagementGroup = context.AddGroup(AIManagementPermissions.GroupName, L("Permission:AIManagement"));

        var workspacesPermission = aiManagementGroup.AddPermission(
            AIManagementPermissions.Workspaces.Default, 
            L("Permission:Workspaces")
        );
        workspacesPermission.AddChild(AIManagementPermissions.Workspaces.Create, L("Permission:Create"));
        workspacesPermission.AddChild(AIManagementPermissions.Workspaces.Edit, L("Permission:Edit"));
        workspacesPermission.AddChild(AIManagementPermissions.Workspaces.Delete, L("Permission:Delete"));

        var ragPermission = aiManagementGroup.AddPermission(
            AIManagementPermissions.RAG.Default, 
            L("Permission:RAG")
        );
        ragPermission.AddChild(AIManagementPermissions.RAG.Manage, L("Permission:Manage"));
        ragPermission.AddChild(AIManagementPermissions.RAG.Index, L("Permission:Index"));

        aiManagementGroup.AddPermission(
            AIManagementPermissions.TestChat.Default,
            L("Permission:TestChat")
        );
        
        var mcpToolsPermission = aiManagementGroup.AddPermission(
            AIManagementPermissions.MCPTools.Default,
            L("Permission:MCPTools")
        );
        mcpToolsPermission.AddChild(AIManagementPermissions.MCPTools.Execute, L("Permission:Execute"));
        mcpToolsPermission.AddChild(AIManagementPermissions.MCPTools.Manage, L("Permission:Manage"));
        
        var mcpServersPermission = aiManagementGroup.AddPermission(
            AIManagementPermissions.MCPServers.Default,
            L("Permission:MCPServers")
        );
        mcpServersPermission.AddChild(AIManagementPermissions.MCPServers.Create, L("Permission:Create"));
        mcpServersPermission.AddChild(AIManagementPermissions.MCPServers.Edit, L("Permission:Edit"));
        mcpServersPermission.AddChild(AIManagementPermissions.MCPServers.Delete, L("Permission:Delete"));
        
        var aiPermission = aiManagementGroup.AddPermission(
            AIManagementPermissions.AI.Default,
            L("Permission:AI")
        );
        aiPermission.AddChild(AIManagementPermissions.AI.Chat, L("Permission:AI.Chat"));
        aiPermission.AddChild(AIManagementPermissions.AI.Audio, L("Permission:AI.Audio"));
        aiPermission.AddChild(AIManagementPermissions.AI.Vision, L("Permission:AI.Vision"));
        aiPermission.AddChild(AIManagementPermissions.AI.Embeddings, L("Permission:AI.Embeddings"));
        aiPermission.AddChild(AIManagementPermissions.AI.FunctionCalling, L("Permission:AI.FunctionCalling"));
        aiPermission.AddChild(AIManagementPermissions.AI.ManageConfigurations, L("Permission:AI.ManageConfigurations"));
        aiPermission.AddChild(AIManagementPermissions.AI.ViewUsage, L("Permission:AI.ViewUsage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AIManagementResource>(name);
    }
}
