using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.Features;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Menus;

/// <summary>
/// Menu contributor for AI Management module.
/// </summary>
public class AIMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<AIResource>();
        var featureChecker = context.ServiceProvider.GetRequiredService<IFeatureChecker>();

        if (!await featureChecker.IsEnabledAsync(SufiAIFeatures.Enable))
        {
            return;
        }

        var aiMenu = context.Menu.Items.FirstOrDefault(item => item.Name == AIMenus.GroupName);
        if (aiMenu == null)
        {
            aiMenu = new ApplicationMenuItem(
                AIMenus.GroupName,
                l["Menu:SufiAI"],
                icon: "sparkles",
                order: 20)
            {
                IsCollapsed = false
            };
            context.Menu.AddItem(aiMenu);
        }

        aiMenu.RequireFeatures(SufiAIFeatures.Enable);

        aiMenu.AddItem(new ApplicationMenuItem("SufiAI.WebResearch", l["WebResearchSettings"],
            url: SettingsSectionLink.For(WebResearchSettingsSection.Id), icon: "search", order: 8)
            .RequirePermissions(AIPermissions.WebSearchSettings.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.WebSearch));

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.Workspaces))
        {
            aiMenu.AddItem(new ApplicationMenuItem(
                AIMenus.Workspaces,
                l["Menu:Workspaces"],
                url: "/panel/admin/ai/workspaces",
                icon: "workspace",
                order: -1
            ).RequirePermissions(AIPermissions.Workspaces.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.Workspaces));
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.Chat))
        {
            aiMenu.AddItem(new ApplicationMenuItem(
                AIMenus.WorkspaceChat,
                l["Menu:WorkspaceChat"],
                url: "/panel/admin/ai/chat",
                icon: "chat",
                order: 0
            ).RequirePermissions(AIPermissions.WorkspaceChat.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.Chat));
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.UsageAnalytics))
        {
            aiMenu.AddItem(new ApplicationMenuItem(
                AIMenus.UsageAnalytics,
                l["Menu:UsageAnalytics"],
                url: "/panel/admin/ai/usage-analytics",
                icon: "chart-bar",
                order: 4
            ).RequirePermissions(AIPermissions.AI.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.UsageAnalytics));
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.RAG))
        {
            var ragMenu = new ApplicationMenuItem(
                AIMenus.RAGGroup,
                l["Menu:RAG"],
                icon: "rag",
                order: 5
            ).RequirePermissions(AIPermissions.RAG.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.RAG);

            ragMenu.AddItem(new ApplicationMenuItem(
                AIMenus.RAGSearch,
                l["Menu:RAGSearch"],
                url: "/panel/admin/ai/rag",
                icon: "search",
                order: 1
            ).RequirePermissions(AIPermissions.RAG.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.RAG));

            ragMenu.AddItem(new ApplicationMenuItem(
                AIMenus.IndexingStatus,
                l["Menu:IndexingStatus"],
                url: "/panel/admin/ai/indexing-status",
                icon: "activity",
                order: 2
            ).RequirePermissions(AIPermissions.RAG.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.RAG));

            aiMenu.AddItem(ragMenu);
        }

        // Host administration only. A tenant circuit must not see MCP servers or tools,
        // even when SufiAI.MCP is enabled for that edition.
        var currentTenant = context.ServiceProvider.GetService<ICurrentTenant>();
        if (currentTenant?.IsAvailable != true && await featureChecker.IsEnabledAsync(SufiAIFeatures.MCP))
        {
            var mcpMenu = new ApplicationMenuItem(
                AIMenus.MCPGroup,
                l["Menu:MCP"],
                icon: "mcp",
                order: 6
            ).RequirePermissions(AIPermissions.MCPTools.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.MCP);

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIMenus.MCPTools,
                l["Menu:MCPTools"],
                url: "/panel/admin/ai/mcp-tools",
                icon: "wrench",
                order: 1
            ).RequirePermissions(AIPermissions.MCPTools.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.MCP));

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIMenus.MCPServers,
                l["Menu:MCPServers"],
                url: "/panel/admin/ai/mcp-servers",
                icon: "server",
                order: 2
            ).RequirePermissions(AIPermissions.MCPServers.Default)
            .RequireFeatures(SufiAIFeatures.Enable, SufiAIFeatures.MCP));

            aiMenu.AddItem(mcpMenu);
        }

        return;
    }
}

/// <summary>
/// Menu name constants for AI Management module.
/// </summary>
public static class AIMenus
{
    public const string GroupName = "SufiAI";
    
    // Standalone items
    public const string Workspaces = GroupName + ".Workspaces";
    public const string ModelConfigurations = GroupName + ".ModelConfigurations";
    
    public const string WorkspaceChat = GroupName + ".WorkspaceChat";
    
    public const string UsageAnalytics = GroupName + ".UsageAnalytics";
    
    // RAG group
    public const string RAGGroup = GroupName + ".RAG";
    public const string RAGSearch = RAGGroup + ".RAGSearch";
    public const string IndexingStatus = RAGGroup + ".IndexingStatus";
    
    // MCP group
    public const string MCPGroup = GroupName + ".MCP";
    public const string MCPTools = MCPGroup + ".MCPTools";
    public const string MCPServers = MCPGroup + ".MCPServers";
}
