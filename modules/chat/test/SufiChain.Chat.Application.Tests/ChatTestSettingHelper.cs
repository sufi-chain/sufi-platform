using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.Settings;

namespace SufiChain.Chat;

public static class ChatTestSettingHelper
{
    public static Task SetAnonymousUsagePolicyAsync(
        ISettingManager settingManager,
        int maxSessionsPerUserPerDay = 100,
        int maxMessagesPerSession = 1000,
        bool enableIpGuard = false,
        int maxSessionsPerIpPerDay = 100,
        int maxMessagesPerIpPerDay = 1000,
        int maxMessagesBeforeSignupRequired = 0,
        int maxAiQuestionsBeforeSignupRequired = 0)
    {
        return SetAnonymousUsagePolicyAsync(
            settingManager,
            null,
            maxSessionsPerUserPerDay,
            maxMessagesPerSession,
            enableIpGuard,
            maxSessionsPerIpPerDay,
            maxMessagesPerIpPerDay,
            maxMessagesBeforeSignupRequired,
            maxAiQuestionsBeforeSignupRequired);
    }

    public static async Task SetAnonymousUsagePolicyAsync(
        ISettingManager settingManager,
        Guid? tenantId,
        int maxSessionsPerUserPerDay,
        int maxMessagesPerSession,
        bool enableIpGuard,
        int maxSessionsPerIpPerDay,
        int maxMessagesPerIpPerDay,
        int maxMessagesBeforeSignupRequired,
        int maxAiQuestionsBeforeSignupRequired)
    {
        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerUserPerDay,
            maxSessionsPerUserPerDay.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerSession,
            maxMessagesPerSession.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.EnableIpGuard,
            enableIpGuard.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerIpPerDay,
            maxSessionsPerIpPerDay.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerIpPerDay,
            maxMessagesPerIpPerDay.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired,
            maxMessagesBeforeSignupRequired.ToString());

        await settingManager.SetForTenantOrGlobalAsync(
            tenantId,
            ChatSettingNames.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired,
            maxAiQuestionsBeforeSignupRequired.ToString());
    }

    public static async Task SetAuthenticatedUsagePolicyAsync(
        ISettingManager settingManager,
        int maxMessagesPerSession = 1000)
    {
        await settingManager.SetGlobalAsync(
            ChatSettingNames.Usage.PublicAuthenticated.MaxMessagesPerSession,
            maxMessagesPerSession.ToString());
    }

    public static async Task SetAiPolicyAsync(
        ISettingManager settingManager,
        bool enabled = true,
        int maxRepliesPerSession = 100)
    {
        await settingManager.SetGlobalAsync(ChatSettingNames.Ai.Enabled, enabled.ToString());
        await settingManager.SetGlobalAsync(ChatSettingNames.Ai.UsageGuard, true.ToString());
        await settingManager.SetGlobalAsync(
            ChatSettingNames.Ai.MaxRepliesPerSession,
            maxRepliesPerSession.ToString());
    }

    public static Task SetDefaultWorkspaceAsync(ISettingManager settingManager, string workspaceName)
    {
        return settingManager.SetGlobalAsync(ChatSettingNames.Ai.DefaultWorkspaceName, workspaceName);
    }
}
