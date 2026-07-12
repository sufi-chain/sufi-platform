using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Localization;
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

        var AIMenu = new ApplicationMenuItem(
            AIMenus.GroupName,
            l["Menu:SufiAI"],
            icon: "sparkles",
            order: 20
        )
        {
            IsCollapsed = false
        };

        context.Menu.AddItem(AIMenu);

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.Workspaces))
        {
            AIMenu.AddItem(new ApplicationMenuItem(
                AIMenus.Workspaces,
                l["Menu:Workspaces"],
                url: "/panel/admin/ai/workspaces",
                icon: "workspace",
                order: 1
            ).RequirePermissions(AIPermissions.Workspaces.Default));
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.Chat))
        {
            var testingMenu = new ApplicationMenuItem(
                AIMenus.TestChat,
                l["Menu:Testing"],
                url: "/panel/admin/ai/test-chat",
                icon: "chat",
                order: 1
            ).RequirePermissions(AIPermissions.TestChat.Default);


            AIMenu.AddItem(testingMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.UsageAnalytics))
        {
            var analyticsMenu = new ApplicationMenuItem(
                AIMenus.AnalyticsGroup,
                l["Menu:Analytics"],
                icon: "analytics",
                order: 4
            ).RequirePermissions(AIPermissions.AI.ViewUsage);

            analyticsMenu.AddItem(new ApplicationMenuItem(
                AIMenus.UsageAnalytics,
                l["Menu:UsageAnalytics"],
                url: "/panel/admin/ai/usage-analytics",
                icon: "chart-bar",
                order: 1
            ).RequirePermissions(AIPermissions.AI.ViewUsage));

            AIMenu.AddItem(analyticsMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.RAG))
        {
            var ragMenu = new ApplicationMenuItem(
                AIMenus.RAGGroup,
                l["Menu:RAG"],
                icon: "rag",
                order: 5
            ).RequirePermissions(AIPermissions.RAG.Default);

            ragMenu.AddItem(new ApplicationMenuItem(
                AIMenus.RAGSearch,
                l["Menu:RAGSearch"],
                url: "/panel/admin/ai/rag",
                icon: "search",
                order: 1
            ).RequirePermissions(AIPermissions.RAG.Default));

            ragMenu.AddItem(new ApplicationMenuItem(
                AIMenus.IndexingStatus,
                l["Menu:IndexingStatus"],
                url: "/panel/admin/ai/indexing-status",
                icon: "activity",
                order: 2
            ).RequirePermissions(AIPermissions.RAG.Default));

            AIMenu.AddItem(ragMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAIFeatures.MCP))
        {
            var mcpMenu = new ApplicationMenuItem(
                AIMenus.MCPGroup,
                l["Menu:MCP"],
                icon: "mcp",
                order: 6
            ).RequirePermissions(AIPermissions.MCPTools.Default);

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIMenus.MCPTools,
                l["Menu:MCPTools"],
                url: "/panel/admin/ai/mcp-tools",
                icon: "wrench",
                order: 1
            ).RequirePermissions(AIPermissions.MCPTools.Default));

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIMenus.MCPServers,
                l["Menu:MCPServers"],
                url: "/panel/admin/ai/mcp-servers",
                icon: "server",
                order: 2
            ).RequirePermissions(AIPermissions.MCPServers.Default));

            AIMenu.AddItem(mcpMenu);
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
    
    // Testing group
    public const string TestingGroup = GroupName + ".Testing";
    public const string TestChat = TestingGroup + ".TestChat";
    
    // Analytics group
    public const string AnalyticsGroup = GroupName + ".Analytics";
    public const string UsageAnalytics = AnalyticsGroup + ".UsageAnalytics";
    
    // RAG group
    public const string RAGGroup = GroupName + ".RAG";
    public const string RAGSearch = RAGGroup + ".RAGSearch";
    public const string IndexingStatus = RAGGroup + ".IndexingStatus";
    
    // MCP group
    public const string MCPGroup = GroupName + ".MCP";
    public const string MCPTools = MCPGroup + ".MCPTools";
    public const string MCPServers = MCPGroup + ".MCPServers";
}
