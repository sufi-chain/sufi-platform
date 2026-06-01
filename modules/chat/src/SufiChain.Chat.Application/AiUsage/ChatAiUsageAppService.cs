using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;

namespace SufiChain.Chat.AiUsage;

[Authorize(ChatPermissions.AiUsage.View)]
public class ChatAiUsageAppService : ChatAppService, IChatAiUsageAppService
{
    protected IChatAiUsageReservationRepository ReservationRepository { get; }
    protected IChatUsagePolicyResolver PolicyResolver { get; }
    protected ChatApplicationMapper Mapper { get; }

    public ChatAiUsageAppService(
        IChatAiUsageReservationRepository reservationRepository,
        IChatUsagePolicyResolver policyResolver,
        ChatApplicationMapper mapper)
    {
        ReservationRepository = reservationRepository;
        PolicyResolver = policyResolver;
        Mapper = mapper;
    }

    public virtual async Task<ChatAiUsageDashboardDto> GetDashboardAsync(GetChatAiUsageDashboardInput input)
    {
        var queryable = await ReservationRepository.GetQueryableAsync();
        var filtered = ApplyReservationFilters(queryable, input.SessionId, null, null, input.StartTime, input.EndTime).ToList();
        var policy = await PolicyResolver.ResolveAiAsync();

        return new ChatAiUsageDashboardDto
        {
            AiEnabled = policy.Enabled,
            UsageGuardEnabled = policy.UsageGuardEnabled,
            ReservedCount = filtered.Count,
            RecordedCount = filtered.Count(item => item.RecordedAt.HasValue),
            DeniedCount = filtered.Count(item => !string.IsNullOrWhiteSpace(item.DenyReason)),
            TotalPromptTokens = filtered.Sum(item => item.PromptTokens ?? 0),
            TotalCompletionTokens = filtered.Sum(item => item.CompletionTokens ?? 0),
            TotalTokens = filtered.Sum(item => item.TotalTokens ?? 0)
        };
    }

    public virtual async Task<PagedResultDto<ChatAiUsageReservationDto>> GetReservationsAsync(
        GetChatAiUsageReservationsInput input)
    {
        var queryable = await ReservationRepository.GetQueryableAsync();
        var filtered = ApplyReservationFilters(
                queryable,
                input.SessionId,
                input.UserId,
                input.OperationKind,
                input.StartTime,
                input.EndTime)
            .OrderByDescending(item => item.ReservedAt)
            .ToList();

        return new PagedResultDto<ChatAiUsageReservationDto>(
            filtered.Count,
            filtered.Skip(input.SkipCount).Take(input.MaxResultCount).Select(Mapper.ToDto).ToList());
    }

    public virtual async Task<PagedResultDto<ChatAiUsageRecordDto>> GetUsageRecordsAsync(
        GetChatAiUsageRecordsInput input)
    {
        var queryable = await ReservationRepository.GetQueryableAsync();
        var filtered = ApplyReservationFilters(
                queryable,
                input.SessionId,
                input.UserId,
                input.OperationKind,
                input.StartTime,
                input.EndTime)
            .Where(item => item.RecordedAt.HasValue)
            .OrderByDescending(item => item.RecordedAt)
            .ToList();

        return new PagedResultDto<ChatAiUsageRecordDto>(
            filtered.Count,
            filtered.Skip(input.SkipCount).Take(input.MaxResultCount).Select(Mapper.ToRecordDto).ToList());
    }

    public virtual async Task<ChatAiUsagePolicyDto> GetEffectivePolicyAsync(Guid? sessionId = null)
    {
        var policy = await PolicyResolver.ResolveAiAsync();
        return new ChatAiUsagePolicyDto
        {
            Enabled = policy.Enabled,
            UsageGuardEnabled = policy.UsageGuardEnabled,
            RequireOperatorForAnonymousHandoff = policy.RequireOperatorForAnonymousHandoff,
            MaxRepliesPerSession = policy.MaxRepliesPerSession,
            MaxTokensPerSession = policy.MaxTokensPerSession,
            MaxTokensPerTenantPerDay = policy.MaxTokensPerTenantPerDay,
            MaxAnonymousAiSessionsPerHour = policy.MaxAnonymousAiSessionsPerHour,
            MaxSuggestionsPerOperatorPerDay = policy.MaxSuggestionsPerOperatorPerDay,
            MaxSummariesPerOperatorPerDay = policy.MaxSummariesPerOperatorPerDay,
            MaxCopilotMessagesPerArticlePerDay = policy.MaxCopilotMessagesPerArticlePerDay,
            MaxRagChunksPerRequest = policy.MaxRagChunksPerRequest
        };
    }

    protected virtual IQueryable<ChatAiUsageReservation> ApplyReservationFilters(
        IQueryable<ChatAiUsageReservation> queryable,
        Guid? sessionId,
        Guid? userId,
        ChatAiOperationKind? operationKind,
        DateTime? startTime,
        DateTime? endTime)
    {
        if (sessionId.HasValue)
        {
            queryable = queryable.Where(item => item.SessionId == sessionId.Value);
        }

        if (userId.HasValue)
        {
            queryable = queryable.Where(item => item.UserId == userId.Value);
        }

        if (operationKind.HasValue)
        {
            queryable = queryable.Where(item => item.OperationKind == operationKind.Value);
        }

        if (startTime.HasValue)
        {
            queryable = queryable.Where(item => item.ReservedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            queryable = queryable.Where(item => item.ReservedAt <= endTime.Value);
        }

        return queryable;
    }
}
