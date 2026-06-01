using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.Features;
using SufiChain.Chat.Localization;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.Chat.Blazor.Menus;

/// <summary>
/// Adds Chat admin navigation items.
/// </summary>
public class ChatMenuContributor : IMenuContributor
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
        var featureChecker = context.ServiceProvider.GetRequiredService<IFeatureChecker>();
        if (!await featureChecker.IsEnabledAsync(ChatFeatures.Enable))
        {
            return;
        }

        var l = context.GetLocalizer<ChatResource>();
        var administration = context.Menu.GetAdministration();

        var chatMenu = new ApplicationMenuItem(
            ChatMenus.GroupName,
            l["Menu:Chat"],
            icon: "comments",
            order: 35
        ).RequirePermissions(ChatPermissions.Inbox.Default);

        administration.AddItem(chatMenu);

        chatMenu.AddItem(new ApplicationMenuItem(
            ChatMenus.OperatorInbox,
            l["Menu:ChatInbox"],
            url: "/admin/chat/inbox",
            icon: "inbox",
            order: 1
        ).RequirePermissions(ChatPermissions.Inbox.Operator));

        chatMenu.AddItem(new ApplicationMenuItem(
            ChatMenus.Sessions,
            l["Menu:ChatSessions"],
            url: "/admin/chat/sessions",
            icon: "list",
            order: 2
        ).RequirePermissions(ChatPermissions.Sessions.Default));

        chatMenu.AddItem(new ApplicationMenuItem(
            ChatMenus.Usage,
            l["Menu:ChatUsage"],
            url: "/admin/chat/usage",
            icon: "chart-line",
            order: 3
        ).RequirePermissions(ChatPermissions.Usage.View));

        chatMenu.AddItem(new ApplicationMenuItem(
            ChatMenus.AiUsage,
            l["Menu:ChatAiUsage"],
            url: "/admin/chat/ai",
            icon: "robot",
            order: 4
        ).RequirePermissions(ChatPermissions.AiUsage.View));

        chatMenu.AddItem(new ApplicationMenuItem(
            ChatMenus.Settings,
            l["Menu:ChatSettings"],
            url: "/admin/chat/settings",
            icon: "cog",
            order: 5
        ).RequirePermissions(ChatPermissions.Settings.Manage));
    }
}
