using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.Chat.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Usage;

public class ChatUsagePolicyResolver : IChatUsagePolicyResolver, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public ChatUsagePolicyResolver(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<ChatUsagePolicy> ResolveAsync(
        AccessMode accessMode,
        CancellationToken cancellationToken = default)
    {
        return accessMode switch
        {
            AccessMode.PublicAnonymous => await ResolveAnonymousAsync(),
            AccessMode.Internal => await ResolveInternalAsync(),
            _ => await ResolveAuthenticatedAsync()
        };
    }

    public virtual async Task<ChatAiUsagePolicy> ResolveAiAsync(CancellationToken cancellationToken = default)
    {
        return new ChatAiUsagePolicy
        {
            Enabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled),
            UsageGuardEnabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.UsageGuard),
            RequireOperatorForAnonymousHandoff = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.RequireOperatorForAnonymousHandoff),
            MaxRepliesPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxRepliesPerSession),
            MaxTokensPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxTokensPerSession),
            MaxTokensPerTenantPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxTokensPerTenantPerDay),
            MaxAnonymousAiSessionsPerHour = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxAnonymousAiSessionsPerHour),
            MaxSuggestionsPerOperatorPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxSuggestionsPerOperatorPerDay),
            MaxSummariesPerOperatorPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxSummariesPerOperatorPerDay),
            MaxCopilotMessagesPerArticlePerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxCopilotMessagesPerArticlePerDay),
            MaxRagChunksPerRequest = await SettingProvider.GetAsync<int>(ChatSettingNames.Ai.MaxRagChunksPerRequest)
        };
    }

    protected virtual async Task<ChatUsagePolicy> ResolveAnonymousAsync()
    {
        return new ChatUsagePolicy
        {
            AccessMode = AccessMode.PublicAnonymous,
            MaxSessionsPerUserPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerUserPerDay),
            MaxMessagesPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerSession),
            MaxAttachmentsPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentsPerSession),
            MaxAttachmentBytesPerSession = await SettingProvider.GetAsync<long>(ChatSettingNames.Usage.PublicAnonymous.MaxAttachmentBytesPerSession),
            EnableAnonymousIpGuard = await SettingProvider.IsTrueAsync(ChatSettingNames.Usage.PublicAnonymous.EnableIpGuard),
            MaxSessionsPerIpPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxSessionsPerIpPerDay),
            MaxMessagesPerIpPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesPerIpPerDay),
            MaxAiSessionsPerIpPerHour = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxAiSessionsPerIpPerHour),
            MaxMessagesBeforeSignupRequired = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired),
            MaxAiQuestionsBeforeSignupRequired = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired),
            LimitExceededAction = await GetLimitExceededActionAsync(ChatSettingNames.Usage.PublicAnonymous.LimitExceededAction)
        };
    }

    protected virtual async Task<ChatUsagePolicy> ResolveAuthenticatedAsync()
    {
        return new ChatUsagePolicy
        {
            AccessMode = AccessMode.PublicAuthenticated,
            MaxSessionsPerUserPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAuthenticated.MaxSessionsPerUserPerDay),
            MaxMessagesPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAuthenticated.MaxMessagesPerSession),
            MaxAttachmentsPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentsPerSession),
            MaxAttachmentBytesPerSession = await SettingProvider.GetAsync<long>(ChatSettingNames.Usage.PublicAuthenticated.MaxAttachmentBytesPerSession),
            LimitExceededAction = await GetLimitExceededActionAsync(ChatSettingNames.Usage.PublicAuthenticated.LimitExceededAction)
        };
    }

    protected virtual async Task<ChatUsagePolicy> ResolveInternalAsync()
    {
        return new ChatUsagePolicy
        {
            AccessMode = AccessMode.Internal,
            MaxSessionsPerUserPerDay = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.Internal.MaxSessionsPerUserPerDay),
            MaxMessagesPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.Internal.MaxMessagesPerSession),
            MaxAttachmentsPerSession = await SettingProvider.GetAsync<int>(ChatSettingNames.Usage.Internal.MaxAttachmentsPerSession),
            MaxAttachmentBytesPerSession = await SettingProvider.GetAsync<long>(ChatSettingNames.Usage.Internal.MaxAttachmentBytesPerSession),
            LimitExceededAction = await GetLimitExceededActionAsync(ChatSettingNames.Usage.Internal.LimitExceededAction)
        };
    }

    protected virtual async Task<LimitExceededAction> GetLimitExceededActionAsync(string settingName)
    {
        var value = await SettingProvider.GetOrNullAsync(settingName);
        return Enum.TryParse<LimitExceededAction>(value, ignoreCase: true, out var action)
            ? action
            : LimitExceededAction.BlockSend;
    }
}
