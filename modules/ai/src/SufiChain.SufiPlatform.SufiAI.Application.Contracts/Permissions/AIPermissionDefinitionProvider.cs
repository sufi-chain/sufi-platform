using SufiChain.SufiPlatform.SufiAI.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.SufiAI.Permissions;

public class AIPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var AIGroup = context.AddGroup(AIPermissions.GroupName, L("Permission:SufiAI"));

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
            L("Permission:SufiAI")
        );
        aiPermission.AddChild(AIPermissions.AI.Chat, L("Permission:SufiAI.Chat"));
        aiPermission.AddChild(AIPermissions.AI.Audio, L("Permission:SufiAI.Audio"));
        aiPermission.AddChild(AIPermissions.AI.Vision, L("Permission:SufiAI.Vision"));
        aiPermission.AddChild(AIPermissions.AI.Embeddings, L("Permission:SufiAI.Embeddings"));
        aiPermission.AddChild(AIPermissions.AI.FunctionCalling, L("Permission:SufiAI.FunctionCalling"));
        aiPermission.AddChild(AIPermissions.AI.ManageConfigurations, L("Permission:SufiAI.ManageConfigurations"));
        aiPermission.AddChild(AIPermissions.AI.ViewUsage, L("Permission:SufiAI.ViewUsage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AIResource>(name);
    }
}
