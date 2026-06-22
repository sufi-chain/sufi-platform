using SufiChain.SufiAbp.AI.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiAbp.AI.Permissions;

public class AIPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var AIGroup = context.AddGroup(AIPermissions.GroupName, L("Permission:AI"));

        var workspacesPermission = AIGroup.AddPermission(
            AIPermissions.Workspaces.Default, 
            L("Permission:Workspaces")
        );
        workspacesPermission.AddChild(AIPermissions.Workspaces.Create, L("Permission:Create"));
        workspacesPermission.AddChild(AIPermissions.Workspaces.Edit, L("Permission:Edit"));
        workspacesPermission.AddChild(AIPermissions.Workspaces.Delete, L("Permission:Delete"));

        var ragPermission = AIGroup.AddPermission(
            AIPermissions.RAG.Default, 
            L("Permission:RAG")
        );
        ragPermission.AddChild(AIPermissions.RAG.Manage, L("Permission:Manage"));
        ragPermission.AddChild(AIPermissions.RAG.Index, L("Permission:Index"));

        AIGroup.AddPermission(
            AIPermissions.TestChat.Default,
            L("Permission:TestChat")
        );
        
        var mcpToolsPermission = AIGroup.AddPermission(
            AIPermissions.MCPTools.Default,
            L("Permission:MCPTools")
        );
        mcpToolsPermission.AddChild(AIPermissions.MCPTools.Execute, L("Permission:Execute"));
        mcpToolsPermission.AddChild(AIPermissions.MCPTools.Manage, L("Permission:Manage"));
        
        var mcpServersPermission = AIGroup.AddPermission(
            AIPermissions.MCPServers.Default,
            L("Permission:MCPServers")
        );
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Create, L("Permission:Create"));
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Edit, L("Permission:Edit"));
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Delete, L("Permission:Delete"));
        
        var aiPermission = AIGroup.AddPermission(
            AIPermissions.AI.Default,
            L("Permission:AI")
        );
        aiPermission.AddChild(AIPermissions.AI.Chat, L("Permission:AI.Chat"));
        aiPermission.AddChild(AIPermissions.AI.Audio, L("Permission:AI.Audio"));
        aiPermission.AddChild(AIPermissions.AI.Vision, L("Permission:AI.Vision"));
        aiPermission.AddChild(AIPermissions.AI.Embeddings, L("Permission:AI.Embeddings"));
        aiPermission.AddChild(AIPermissions.AI.FunctionCalling, L("Permission:AI.FunctionCalling"));
        aiPermission.AddChild(AIPermissions.AI.ManageConfigurations, L("Permission:AI.ManageConfigurations"));
        aiPermission.AddChild(AIPermissions.AI.ViewUsage, L("Permission:AI.ViewUsage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AIResource>(name);
    }
}
