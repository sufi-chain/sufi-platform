using SufiChain.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace SufiChain.Chat.Permissions;

public class ChatPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var chatGroup = context.AddGroup(ChatPermissions.GroupName, L("Permission:Chat"));

        var sessionsPermission = chatGroup.AddPermission(ChatPermissions.Sessions.Default, L("Permission:Sessions"));
        sessionsPermission.AddChild(ChatPermissions.Sessions.Create, L("Permission:Create"));
        sessionsPermission.AddChild(ChatPermissions.Sessions.Close, L("Permission:Close"));
        sessionsPermission.AddChild(ChatPermissions.Sessions.Manage, L("Permission:Manage"));

        var messagesPermission = chatGroup.AddPermission(ChatPermissions.Messages.Default, L("Permission:Messages"));
        messagesPermission.AddChild(ChatPermissions.Messages.Send, L("Permission:Send"));
        messagesPermission.AddChild(ChatPermissions.Messages.SendInternal, L("Permission:SendInternal"));
        messagesPermission.AddChild(ChatPermissions.Messages.Delete, L("Permission:Delete"));
        messagesPermission.AddChild(ChatPermissions.Messages.ViewInternal, L("Permission:ViewInternal"));

        var inboxPermission = chatGroup.AddPermission(ChatPermissions.Inbox.Default, L("Permission:Inbox"));
        inboxPermission.AddChild(ChatPermissions.Inbox.User, L("Permission:UserInbox"));
        inboxPermission.AddChild(ChatPermissions.Inbox.Operator, L("Permission:OperatorInbox"));
        inboxPermission.AddChild(ChatPermissions.Inbox.Admin, L("Permission:AdminInbox"));
        inboxPermission.AddChild(ChatPermissions.Inbox.Reply, L("Permission:Reply"));
        inboxPermission.AddChild(ChatPermissions.Inbox.Manage, L("Permission:Manage"));

        var usagePermission = chatGroup.AddPermission(ChatPermissions.Usage.Default, L("Permission:Usage"));
        usagePermission.AddChild(ChatPermissions.Usage.View, L("Permission:Usage.View"));
        usagePermission.AddChild(ChatPermissions.Usage.ManagePolicies, L("Permission:Usage.ManagePolicies"));

        var aiUsagePermission = chatGroup.AddPermission(ChatPermissions.AiUsage.Default, L("Permission:AiUsage"));
        aiUsagePermission.AddChild(ChatPermissions.AiUsage.View, L("Permission:AiUsage.View"));
        aiUsagePermission.AddChild(ChatPermissions.AiUsage.Manage, L("Permission:Manage"));

        var settingsPermission = chatGroup.AddPermission(ChatPermissions.Settings.Default, L("Permission:Settings"));
        settingsPermission.AddChild(ChatPermissions.Settings.Manage, L("Permission:Manage"));

        var linksPermission = chatGroup.AddPermission(ChatPermissions.Links.Default, L("Permission:Links"));
        linksPermission.AddChild(ChatPermissions.Links.Manage, L("Permission:Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
