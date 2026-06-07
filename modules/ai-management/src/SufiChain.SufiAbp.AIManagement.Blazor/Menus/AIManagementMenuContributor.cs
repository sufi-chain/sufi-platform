using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Localization;
using SufiChain.SufiAbp.AIManagement.Permissions;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Menus;

/// <summary>
/// Menu contributor for AI Management module.
/// </summary>
public class AIManagementMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<AIManagementResource>();
        var featureChecker = context.ServiceProvider.GetRequiredService<IFeatureChecker>();

        if (!await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable))
        {
            return;
        }

        var aiManagementMenu = new ApplicationMenuItem(
            AIManagementMenus.GroupName,
            l["Menu:AIManagement"],
            icon: "sparkles",
            order: 30
        )
        {
            IsCollapsed = false
        }.RequirePermissions(AIManagementPermissions.Workspaces.Default);

        context.Menu.AddItem(aiManagementMenu);

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces))
        {
            aiManagementMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.Workspaces,
                l["Menu:Workspaces"],
                url: "/admin/ai-management/workspaces",
                icon: "workspace",
                order: 1
            ).RequirePermissions(AIManagementPermissions.Workspaces.Default));
        }

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces))
        {
            var configurationMenu = new ApplicationMenuItem(
                AIManagementMenus.ConfigurationGroup,
                l["Menu:Configuration"],
                icon: "sliders",
                order: 2
            ).RequirePermissions(AIManagementPermissions.AI.ManageConfigurations);

            configurationMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.ModelConfigurations,
                l["Menu:ModelConfigurations"],
                url: "/admin/ai-management/model-configurations",
                icon: "model",
                order: 1
            ).RequirePermissions(AIManagementPermissions.AI.ManageConfigurations));

            aiManagementMenu.AddItem(configurationMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat))
        {
            var testingMenu = new ApplicationMenuItem(
                AIManagementMenus.TestingGroup,
                l["Menu:Testing"],
                icon: "flask",
                order: 3
            ).RequirePermissions(AIManagementPermissions.TestChat.Default);

            testingMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.TestChat,
                l["Menu:TestChat"],
                url: "/admin/ai-management/test-chat",
                icon: "chat",
                order: 1
            ).RequirePermissions(AIManagementPermissions.TestChat.Default));

            if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Audio) ||
                await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Vision) ||
                await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Embeddings))
            {
                testingMenu.AddItem(new ApplicationMenuItem(
                    AIManagementMenus.MultiModalTest,
                    l["Menu:MultiModalTest"],
                    url: "/admin/ai-management/multimodal-test",
                    icon: "multimodal",
                    order: 2
                ).RequirePermissions(AIManagementPermissions.AI.Chat));
            }

            aiManagementMenu.AddItem(testingMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.UsageAnalytics))
        {
            var analyticsMenu = new ApplicationMenuItem(
                AIManagementMenus.AnalyticsGroup,
                l["Menu:Analytics"],
                icon: "analytics",
                order: 4
            ).RequirePermissions(AIManagementPermissions.AI.ViewUsage);

            analyticsMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.UsageAnalytics,
                l["Menu:UsageAnalytics"],
                url: "/admin/ai-management/usage-analytics",
                icon: "chart-bar",
                order: 1
            ).RequirePermissions(AIManagementPermissions.AI.ViewUsage));

            aiManagementMenu.AddItem(analyticsMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.RAG))
        {
            var ragMenu = new ApplicationMenuItem(
                AIManagementMenus.RAGGroup,
                l["Menu:RAG"],
                icon: "rag",
                order: 5
            ).RequirePermissions(AIManagementPermissions.RAG.Default);

            ragMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.RAGSearch,
                l["Menu:RAGSearch"],
                url: "/admin/ai-management/rag",
                icon: "search",
                order: 1
            ).RequirePermissions(AIManagementPermissions.RAG.Default));

            ragMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.IndexingStatus,
                l["Menu:IndexingStatus"],
                url: "/admin/ai-management/indexing-status",
                icon: "activity",
                order: 2
            ).RequirePermissions(AIManagementPermissions.RAG.Default));

            aiManagementMenu.AddItem(ragMenu);
        }

        if (await featureChecker.IsEnabledAsync(SufiAbpAIFeatures.MCP))
        {
            var mcpMenu = new ApplicationMenuItem(
                AIManagementMenus.MCPGroup,
                l["Menu:MCP"],
                icon: "mcp",
                order: 6
            ).RequirePermissions(AIManagementPermissions.MCPTools.Default);

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.MCPTools,
                l["Menu:MCPTools"],
                url: "/admin/ai-management/mcp-tools",
                icon: "wrench",
                order: 1
            ).RequirePermissions(AIManagementPermissions.MCPTools.Default));

            mcpMenu.AddItem(new ApplicationMenuItem(
                AIManagementMenus.MCPServers,
                l["Menu:MCPServers"],
                url: "/admin/ai-management/mcp-servers",
                icon: "server",
                order: 2
            ).RequirePermissions(AIManagementPermissions.MCPServers.Default));

            aiManagementMenu.AddItem(mcpMenu);
        }

        return;
    }
}

/// <summary>
/// Menu name constants for AI Management module.
/// </summary>
public static class AIManagementMenus
{
    public const string GroupName = "AIManagement";
    
    // Standalone items
    public const string Workspaces = GroupName + ".Workspaces";
    
    // Configuration group
    public const string ConfigurationGroup = GroupName + ".Configuration";
    public const string ModelConfigurations = ConfigurationGroup + ".ModelConfigurations";
    
    // Testing group
    public const string TestingGroup = GroupName + ".Testing";
    public const string TestChat = TestingGroup + ".TestChat";
    public const string MultiModalTest = TestingGroup + ".MultiModalTest";
    
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
