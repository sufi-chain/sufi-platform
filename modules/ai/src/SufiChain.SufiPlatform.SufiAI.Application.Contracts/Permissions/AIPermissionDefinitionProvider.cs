using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.Features;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Permissions;

public class AIPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var AIGroup = context.AddGroup(AIPermissions.GroupName, L("Permission:SufiAI"));

        // sufi-chain#490: every AI permission is only meaningful while the master AI feature is on.
        // The feature state checker sits on each permission (not just the group) because ABP checks a
        // child permission on its own. With the feature off the permissions read as not granted, so the
        // AI menus/pages close and [Authorize]-guarded AI app services refuse calls.
        var webSettings = AIGroup.AddPermission("SufiAI.WebSearchSettings.Default", L("WebResearchSettings"))
            .RequireFeatures(SufiAIFeatures.Enable);
        webSettings.AddChild("SufiAI.WebSearchSettings.Update", L("Permission:Edit")).RequireFeatures(SufiAIFeatures.Enable);
        webSettings.AddChild("SufiAI.WebSearchSettings.Test", L("WebResearchTestConnection")).RequireFeatures(SufiAIFeatures.Enable);
        var workspacesPermission = AIGroup.AddPermission(
            AIPermissions.Workspaces.Default, 
            L("Permission:Workspaces")
        ).RequireFeatures(SufiAIFeatures.Enable);
        workspacesPermission.AddChild(AIPermissions.Workspaces.Create, L("Permission:Create")).RequireFeatures(SufiAIFeatures.Enable);
        workspacesPermission.AddChild(AIPermissions.Workspaces.Edit, L("Permission:Edit")).RequireFeatures(SufiAIFeatures.Enable);
        workspacesPermission.AddChild(AIPermissions.Workspaces.Delete, L("Permission:Delete")).RequireFeatures(SufiAIFeatures.Enable);

        var ragPermission = AIGroup.AddPermission(
            AIPermissions.RAG.Default, 
            L("Permission:RAG")
        ).RequireFeatures(SufiAIFeatures.Enable);
        ragPermission.AddChild(AIPermissions.RAG.Manage, L("Permission:Manage")).RequireFeatures(SufiAIFeatures.Enable);
        ragPermission.AddChild(AIPermissions.RAG.Index, L("Permission:Index")).RequireFeatures(SufiAIFeatures.Enable);

        AIGroup.AddPermission(
            AIPermissions.WorkspaceChat.Default,
            L("Permission:WorkspaceChat")
        ).RequireFeatures(SufiAIFeatures.Enable);
        
        var mcpToolsPermission = AIGroup.AddPermission(
            AIPermissions.MCPTools.Default,
            L("Permission:MCPTools")
        ).RequireFeatures(SufiAIFeatures.Enable);
        mcpToolsPermission.AddChild(AIPermissions.MCPTools.Execute, L("Permission:Execute")).RequireFeatures(SufiAIFeatures.Enable);
        mcpToolsPermission.AddChild(AIPermissions.MCPTools.Manage, L("Permission:Manage")).RequireFeatures(SufiAIFeatures.Enable);
        
        var mcpServersPermission = AIGroup.AddPermission(
            AIPermissions.MCPServers.Default,
            L("Permission:MCPServers")
        ).RequireFeatures(SufiAIFeatures.Enable);
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Create, L("Permission:Create")).RequireFeatures(SufiAIFeatures.Enable);
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Edit, L("Permission:Edit")).RequireFeatures(SufiAIFeatures.Enable);
        mcpServersPermission.AddChild(AIPermissions.MCPServers.Delete, L("Permission:Delete")).RequireFeatures(SufiAIFeatures.Enable);

        // sufi-chain#384 slice 6a: the tool-call audit trail is its own permission so it can be granted
        // to an auditor without handing over MCP server management.
        AIGroup.AddPermission(
            AIPermissions.McpAudit.Default,
            L("Permission:McpAudit")
        ).RequireFeatures(SufiAIFeatures.Enable);
        
        var aiPermission = AIGroup.AddPermission(
            AIPermissions.AI.Default,
            L("Permission:SufiAI")
        ).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.Chat, L("Permission:SufiAI.Chat")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.Audio, L("Permission:SufiAI.Audio")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.Vision, L("Permission:SufiAI.Vision")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.Embeddings, L("Permission:SufiAI.Embeddings")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.WebSearch, L("Permission:SufiAI.WebSearch")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.WebFetch, L("Permission:SufiAI.WebFetch")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.FunctionCalling, L("Permission:SufiAI.FunctionCalling")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.ManageConfigurations, L("Permission:SufiAI.ManageConfigurations")).RequireFeatures(SufiAIFeatures.Enable);
        aiPermission.AddChild(AIPermissions.AI.ViewUsage, L("Permission:SufiAI.ViewUsage")).RequireFeatures(SufiAIFeatures.Enable);

        var aiCredits = AIGroup.AddPermission(
            AIPermissions.AiCredits.Default,
            L("Permission:SufiAI.AiCredits"),
            multiTenancySide: MultiTenancySides.Host).RequireFeatures(SufiAIFeatures.Enable);
        aiCredits.AddChild(
            AIPermissions.AiCredits.ViewAll,
            L("Permission:SufiAI.AiCredits.ViewAll"),
            multiTenancySide: MultiTenancySides.Host).RequireFeatures(SufiAIFeatures.Enable);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AIResource>(name);
    }
}
