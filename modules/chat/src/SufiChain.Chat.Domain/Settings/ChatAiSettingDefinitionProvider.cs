using SufiChain.Chat.AiUsage;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Settings;

public class ChatAiSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(ChatSettingNames.Ai.Enabled, "false", isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.UsageGuard, "true", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.RequireOperatorForAnonymousHandoff, "true", isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.DefaultWorkspaceName, string.Empty, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.AssistantMappings, ChatAssistantMappings.EmptyJson, isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxRepliesPerSession, ChatSettingDefaults.AiMaxRepliesPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxTokensPerSession, ChatSettingDefaults.AiMaxTokensPerSession.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxTokensPerTenantPerDay, ChatSettingDefaults.AiMaxTokensPerTenantPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxAnonymousAiSessionsPerHour, ChatSettingDefaults.AiMaxAnonymousAiSessionsPerHour.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxSuggestionsPerOperatorPerDay, ChatSettingDefaults.AiMaxSuggestionsPerOperatorPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxSummariesPerOperatorPerDay, ChatSettingDefaults.AiMaxSummariesPerOperatorPerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxCopilotMessagesPerArticlePerDay, ChatSettingDefaults.AiMaxCopilotMessagesPerArticlePerDay.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.MaxRagChunksPerRequest, ChatSettingDefaults.AiMaxRagChunksPerRequest.ToString(), isVisibleToClients: false, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.DoNotTrain, "true", isVisibleToClients: true, isInherited: true),
            new SettingDefinition(ChatSettingNames.Ai.FallbackMessageKey, "Chat:AiUnavailable", isVisibleToClients: true, isInherited: true));
    }
}
