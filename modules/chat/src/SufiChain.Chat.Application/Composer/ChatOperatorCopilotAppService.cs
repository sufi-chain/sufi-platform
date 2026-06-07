using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp;
using AiChatMessage = SufiChain.SufiAbp.AIManagement.AI.ChatMessage;
using Volo.Abp.Settings;

namespace SufiChain.Chat.Composer;

[Authorize(ChatPermissions.Inbox.Reply)]
public class ChatOperatorCopilotAppService : ChatAppService, IChatOperatorCopilotAppService
{
    protected IChatSessionRepository SessionRepository { get; }

    protected IChatAssistantWorkspaceResolver WorkspaceResolver { get; }

    protected IChatUsageGuard UsageGuard { get; }

    protected IAIService AiService { get; }

    public ChatOperatorCopilotAppService(
        IChatSessionRepository sessionRepository,
        IChatAssistantWorkspaceResolver workspaceResolver,
        IChatUsageGuard usageGuard,
        IAIService aiService)
    {
        SessionRepository = sessionRepository;
        WorkspaceResolver = workspaceResolver;
        UsageGuard = usageGuard;
        AiService = aiService;
    }

    public virtual async Task<ChatOperatorCopilotResultDto> AssistAsync(ChatOperatorCopilotInput input)
    {
        if (!await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled))
        {
            throw new BusinessException(ChatErrorCodes.AiUnavailable);
        }

        var session = await SessionRepository.GetAsync(input.SessionId);
        var usageResult = await UsageGuard.CheckCanInvokeAiAsync(
            input.SessionId,
            ChatAiOperationKind.SuggestReply);

        if (!usageResult.IsAllowed)
        {
            throw new BusinessException(usageResult.ReasonCode ?? ChatErrorCodes.AiUnavailable);
        }

        var workspaceName = await WorkspaceResolver.ResolveWorkspaceNameAsync(new ChatAssistantWorkspaceResolveContext
        {
            SessionId = session.Id,
            SessionMetadataJson = session.MetadataJson,
            AccessMode = session.AccessMode,
            ConversationKind = session.ConversationKind
        });

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new BusinessException(ChatErrorCodes.AiUnavailable);
        }

        var reservationId = await UsageGuard.ReserveAiUsageAsync(
            input.SessionId,
            ChatAiOperationKind.SuggestReply);

        try
        {
            var systemPrompt = BuildSystemPrompt(input.Operation);
            var userMessage = BuildUserMessage(input);

            var response = await AiService.SendChatMessageAsync(new ChatCompletionRequest
            {
                WorkspaceName = workspaceName,
                SystemPrompt = systemPrompt,
                Messages =
                [
                    new AiChatMessage
                    {
                        Role = "user",
                        Content = userMessage
                    }
                ]
            });

            await UsageGuard.RecordAiUsageAsync(reservationId, new ChatAiUsageRecord
            {
                OperatorUserId = CurrentUser.Id,
                PromptTokens = response.InputTokens ?? 0,
                CompletionTokens = response.OutputTokens ?? 0,
                TotalTokens = response.TotalTokens ?? 0,
                ProviderName = response.ModelId,
                WorkspaceName = workspaceName
            });

            return new ChatOperatorCopilotResultDto
            {
                SuggestedText = response.Content.Trim(),
                WorkspaceName = workspaceName,
                TotalTokens = response.TotalTokens
            };
        }
        catch
        {
            await UsageGuard.ReleaseAiReservationAsync(reservationId);
            throw;
        }
    }

    protected virtual string BuildSystemPrompt(ChatOperatorCopilotOperation operation)
    {
        return operation switch
        {
            ChatOperatorCopilotOperation.Rewrite =>
                "You are a support operator assistant. Rewrite the draft reply to be clear, professional, and concise. Return only the rewritten message text.",
            ChatOperatorCopilotOperation.ImproveTone =>
                "You are a support operator assistant. Improve the tone of the draft reply while preserving meaning. Return only the improved message text.",
            ChatOperatorCopilotOperation.GenerateFromPrompt =>
                "You are a support operator assistant. Write a customer-facing chat reply based on the operator prompt. Return only the reply text.",
            _ => "You are a support operator assistant. Return only the suggested reply text."
        };
    }

    protected virtual string BuildUserMessage(ChatOperatorCopilotInput input)
    {
        return input.Operation switch
        {
            ChatOperatorCopilotOperation.GenerateFromPrompt =>
                string.IsNullOrWhiteSpace(input.Prompt)
                    ? throw new BusinessException(ChatErrorCodes.MessageContentRequired)
                    : input.Prompt.Trim(),
            _ => string.IsNullOrWhiteSpace(input.DraftText)
                ? throw new BusinessException(ChatErrorCodes.MessageContentRequired)
                : input.DraftText.Trim()
        };
    }
}
