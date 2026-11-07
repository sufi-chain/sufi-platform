using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;
using SufiChain.SufiPlatform.SufiCom.Chat.Connectors;
using SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Mapping;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: an operation id is not a pass to another session, visibility, or metadata.
/// </summary>
public class AssistantTranscriptBindingTests : ChatApplicationTestBase<SufiComChatEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task An_operation_from_another_session_is_not_returned()
    {
        var first = await CreateSessionAsync();
        var second = await CreateSessionAsync();
        var operationId = Guid.Parse("11111111-1111-1111-1111-111111111191");
        await SaveAsync(first, operationId, "private reply");

        var error = await Should.ThrowAsync<AbpAuthorizationException>(() => SaveAsync(second, operationId, "private reply"));
        error.Message.ShouldNotContain("private reply");

        (await ReloadAsync(second)).ShouldBeEmpty();
        (await ReloadAsync(first)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task A_different_visibility_or_marker_does_not_reuse_the_operation()
    {
        var sessionId = await CreateSessionAsync();
        var operationId = Guid.Parse("11111111-1111-1111-1111-111111111192");
        await SaveAsync(sessionId, operationId, "marked", extraProperties: new ExtraPropertyDictionary
        {
            ["helpdesk.kb.proposal"] = "draft"
        });

        await Should.ThrowAsync<AbpAuthorizationException>(() => SaveAsync(
            sessionId,
            operationId,
            "marked",
            isInternal: true,
            extraProperties: new ExtraPropertyDictionary { ["helpdesk.kb.proposal"] = "draft" }));
        await Should.ThrowAsync<AbpAuthorizationException>(() => SaveAsync(
            sessionId,
            operationId,
            "marked",
            extraProperties: new ExtraPropertyDictionary { ["helpdesk.kb.proposal"] = "other" }));

        var stored = await ReloadAsync(sessionId);
        stored.Count.ShouldBe(1);
        stored[0].IsInternal.ShouldBeFalse();
        stored[0].ExtraProperties["helpdesk.kb.proposal"].ShouldBe("draft");
    }

    [Fact]
    public async Task Two_saves_of_one_operation_count_usage_once()
    {
        var sessionId = await CreateSessionAsync();
        var operationId = Guid.Parse("11111111-1111-1111-1111-111111111193");
        var writer = GetRequiredService<IChatAssistantTranscriptWriter>();
        var first = writer.SaveForCurrentUserAsync(Input(sessionId, operationId, "once"));
        var second = writer.SaveForCurrentUserAsync(Input(sessionId, operationId, "once"));
        try
        {
            await Task.WhenAll(first, second);
        }
        catch (Exception)
        {
            // One of the two can lose the charge row. The stored quota stays at one.
        }

        (await ReloadAsync(sessionId)).Count.ShouldBe(1);
        (await SessionMessageCountAsync(sessionId)).ShouldBe(1);
    }

    [Fact]
    public async Task A_bot_turn_id_retries_the_same_transcript()
    {
        var sessionId = await CreateSessionAsync();
        var turnId = Guid.Parse("11111111-1111-1111-1111-111111111194");
        var integration = GetRequiredService<IChatIntegrationService>();
        var input = new PostBotMessageInput
        {
            SessionId = sessionId,
            Body = "bot reply",
            TurnId = turnId
        };

        var first = await integration.PostBotMessageAsync(input);
        var second = await integration.PostBotMessageAsync(input);

        second.ShouldBe(first);
        (await ReloadAsync(sessionId)).Count.ShouldBe(1);
        AssistantTranscriptOperation.Resolve(Guid.Empty, turnId).ShouldBe(turnId);
        AssistantTranscriptOperation.Resolve(turnId, Guid.NewGuid()).ShouldBe(turnId);
    }

    private static SendAssistantTranscriptInput Input(Guid sessionId, Guid operationId, string body)
    {
        return new SendAssistantTranscriptInput
        {
            SessionId = sessionId,
            Body = body,
            OperationId = operationId
        };
    }

    private Task<List<ChatMessageDto>> SaveAsync(
        Guid sessionId,
        Guid operationId,
        string body,
        bool isInternal = false,
        ExtraPropertyDictionary? extraProperties = null)
    {
        return GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = body,
                OperationId = operationId,
                IsInternal = isInternal,
                ExtraProperties = extraProperties ?? new ExtraPropertyDictionary()
            });
    }

    private async Task<Guid> CreateSessionAsync()
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        });
        return session.Id;
    }

    private async Task<List<ChatMessage>> ReloadAsync(Guid sessionId)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var messages = await GetRequiredService<IChatMessageRepository>().GetListBySessionAsync(
            sessionId,
            includeInternal: true,
            maxResultCount: 20);
        await uow.CompleteAsync();
        return messages;
    }

    private Task<long> SessionMessageCountAsync(Guid sessionId)
    {
        return GetRequiredService<IChatUsageCounterRepository>().GetCountAsync(
            tenantId: null,
            $"Session:{sessionId:D}:Messages",
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue);
    }
}

public class AssistantTranscriptBindingModuleTests : ChatApplicationTestBase<AssistantTranscriptBindingTestModule>
{
    [Fact]
    public async Task The_workspace_save_keeps_its_operation_id_when_delivery_fails()
    {
        UsageMarkerGapWriter.FailuresRemaining = 1;
        SuppressedAssistantLookupRepository.Remaining = 0;
        var sessionId = (await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        })).Id;
        var attempt = new AssistantTranscriptAttempt();
        var body = "recover this reply";
        var turnId = Guid.NewGuid();
        var operationId = attempt.BeginTurn(sessionId, turnId);
        var writer = GetRequiredService<IChatAssistantTranscriptWriter>();

        await Should.ThrowAsync<InvalidOperationException>(() => writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = sessionId,
            Body = body,
            OperationId = operationId
        }));
        attempt.NoteFailure();
        attempt.RetryFailedTurn(sessionId, turnId).ShouldBe(operationId);
        (await GetRequiredService<IChatUsageCounterRepository>().GetCountAsync(
            null,
            $"Session:{sessionId:D}:Messages",
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue)).ShouldBe(1);

        var stored = await writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = sessionId,
            Body = body,
            OperationId = attempt.RetryFailedTurn(sessionId, turnId)
        });

        stored.Count.ShouldBe(1);
        (await GetRequiredService<IChatUsageCounterRepository>().GetCountAsync(
            null,
            $"Session:{sessionId:D}:Messages",
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue)).ShouldBe(1);
        attempt.Complete();
        attempt.BeginTurn(sessionId, Guid.NewGuid()).ShouldNotBe(operationId);
    }

    [Fact]
    public async Task A_conflict_winner_from_another_session_is_not_returned()
    {
        SuppressedAssistantLookupRepository.Remaining = 0;
        var sessions = GetRequiredService<IChatSessionAppService>();
        var first = (await sessions.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        })).Id;
        var second = (await sessions.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        })).Id;
        var operationId = Guid.Parse("11111111-1111-1111-1111-111111111195");
        var writer = GetRequiredService<IChatAssistantTranscriptWriter>();
        await writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = first,
            Body = "owned",
            OperationId = operationId
        });

        SuppressedAssistantLookupRepository.Remaining = 3;
        await Should.ThrowAsync<AbpAuthorizationException>(() => writer.SaveForCurrentUserAsync(new SendAssistantTranscriptInput
        {
            SessionId = second,
            Body = "owned",
            OperationId = operationId
        }));

        SuppressedAssistantLookupRepository.Remaining.ShouldBe(0);
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var secondCount = await GetRequiredService<IChatMessageRepository>().GetCountBySessionAsync(second, includeInternal: true);
        var firstCount = await GetRequiredService<IChatMessageRepository>().GetCountBySessionAsync(first, includeInternal: true);
        await uow.CompleteAsync();
        secondCount.ShouldBe(0);
        firstCount.ShouldBe(1);
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptBindingTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<IChatMessageRepository, SuppressedAssistantLookupRepository>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatAssistantTranscriptWriter, UsageMarkerGapWriter>());
    }
}

[DisableConventionalRegistration]
public class SuppressedAssistantLookupRepository : EfCoreSufiComChatMessageRepository
{
    public static int Remaining;

    public SuppressedAssistantLookupRepository(IDbContextProvider<ISufiComChatDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<List<ChatMessage>> GetListByAssistantOperationAsync(
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        if (Remaining > 0)
        {
            Remaining--;
            return new List<ChatMessage>();
        }

        return await base.GetListByAssistantOperationAsync(operationId, cancellationToken);
    }
}

[DisableConventionalRegistration]
public class UsageMarkerGapWriter : ChatAssistantTranscriptWriter
{
    public static int FailuresRemaining;

    public UsageMarkerGapWriter(
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository,
        IChatParticipantRepository participantRepository,
        IPermissionChecker permissionChecker,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        ChatMessageManager messageManager,
        IChatUsageGuard usageGuard,
        ChatOutboundMessageDispatcher outboundMessageDispatcher,
        IDistributedEventBus distributedEventBus,
        ChatApplicationMapper mapper,
        IChatRealtimeNotifier realtimeNotifier,
        IChatSessionDtoEnricher sessionDtoEnricher,
        ChatAssistantReplyWatchdog assistantReplyWatchdog,
        IChatInboxAssistantAccessChecker assistantAccessChecker,
        ILogger<ChatAssistantTranscriptWriter> logger)
        : base(
            sessionRepository,
            messageRepository,
            participantRepository,
            permissionChecker,
            currentUser,
            currentTenant,
            unitOfWorkManager,
            messageManager,
            usageGuard,
            outboundMessageDispatcher,
            distributedEventBus,
            mapper,
            realtimeNotifier,
            sessionDtoEnricher,
            assistantReplyWatchdog,
            assistantAccessChecker,
            logger)
    {
    }

    protected override Task OnUsageChargeCommittedAsync(Guid operationId, int partIndex)
    {
        if (FailuresRemaining <= 0)
        {
            return Task.CompletedTask;
        }

        FailuresRemaining--;
        throw new InvalidOperationException("marker");
    }
}
