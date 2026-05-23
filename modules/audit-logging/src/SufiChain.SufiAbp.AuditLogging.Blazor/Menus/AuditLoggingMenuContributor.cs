using SufiChain.SufiAbp.AuditLogging.Localization;
using SufiChain.SufiAbp.AuditLogging.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.AuditLogging.Blazor.Menus;

public class AuditLoggingMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            return ConfigureMainMenuAsync(context);
        }

        return Task.CompletedTask;
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<SufiAbpAuditLoggingResource>();
        var administration = context.Menu.GetAdministration();

        // Audit Logs page - includes Actions and EntityChanges in detail view
        administration.AddItem(new ApplicationMenuItem(
            AuditLoggingMenuNames.GroupName,
            l["AuditLogs"],
            url: "/admin/audit-logs",
            icon: "clipboard-list",
            order: 20
        ).RequirePermissions(AuditLoggingPermissions.AuditLogs.Default));

        return Task.CompletedTask;
    }
}

public static class AuditLoggingMenuNames
{
    public const string GroupName = "AuditLogging";
}
