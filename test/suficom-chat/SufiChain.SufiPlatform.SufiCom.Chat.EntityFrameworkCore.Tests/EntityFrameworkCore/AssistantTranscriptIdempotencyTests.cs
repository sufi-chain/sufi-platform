using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: an assistant turn is identified by its operation id.
/// The same text can be stored again for another turn, and a retry finds the original
/// even when it is no longer in the newest page.
/// </summary>
public class AssistantTranscriptIdempotencyTests : ChatApplicationTestBase<SufiComChatEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task The_same_text_in_two_turns_is_stored_twice()
    {
        var sessionId = await CreateSessionAsync();
        var body = "same assistant text";

        var first = await SaveAsync(sessionId, body, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));
        var second = await SaveAsync(sessionId, body, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"));

        first.Single().Id.ShouldNotBe(second.Single().Id);
        var stored = await ReloadAsync(sessionId);
        stored.Count.ShouldBe(2);
        stored.ShouldAllBe(message => message.Body == body);
    }

    [Fact]
    public async Task The_same_text_with_different_visibility_is_not_reused()
    {
        var sessionId = await CreateSessionAsync();
        var body = "same assistant text";
        var marker = new ExtraPropertyDictionary { ["helpdesk.kb.proposal"] = "draft" };

        await SaveAsync(sessionId, body, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), isInternal: false);
        await SaveAsync(
            sessionId,
            body,
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"),
            isInternal: true,
            extraProperties: marker);

        var stored = await ReloadAsync(sessionId);
        stored.Count.ShouldBe(2);
        stored.Single(message => !message.IsInternal).ExtraProperties.ContainsKey("helpdesk.kb.proposal").ShouldBeFalse();
        stored.Single(message => message.IsInternal).ExtraProperties["helpdesk.kb.proposal"].ShouldBe("draft");
    }

    [Fact]
    public async Task A_retry_finds_a_transcript_older_than_the_newest_page()
    {
        var sessionId = await CreateSessionAsync();
        var operationId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
        var body = new string('a', ChatConsts.MaxMessageBodyLength) + "tail";
        var first = await SaveAsync(sessionId, body, operationId);

        using (var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true))
        {
            var repository = GetRequiredService<IChatMessageRepository>();
            for (var index = 0; index < 201; index++)
            {
                await repository.InsertAsync(
                    new ChatMessage(
                        Guid.NewGuid(),
                        tenantId: null,
                        sessionId,
                        "pad-" + index,
                        ChatMessageSenderKind.Visitor),
                    autoSave: true);
            }

            await uow.CompleteAsync();
        }

        var second = await SaveAsync(sessionId, body, operationId);
        second.Select(message => message.Id).ShouldBe(first.Select(message => message.Id));

        var operation = await GetRequiredService<IChatMessageRepository>().GetListByAssistantOperationAsync(operationId);
        operation.Count.ShouldBe(first.Count);
        string.Concat(operation.Select(message => message.Body)).ShouldBe(body);
        (await ReloadAsync(sessionId)).Count(message => message.AssistantOperationId == operationId).ShouldBe(first.Count);
    }

    [Fact]
    public async Task Ordinary_messages_without_an_operation_can_share_a_session()
    {
        var sessionId = await CreateSessionAsync();
        var messages = GetRequiredService<IChatMessageAppService>();
        await messages.SendAsync(VisitorInput(sessionId, "one"));
        await messages.SendAsync(VisitorInput(sessionId, "two"));

        (await ReloadAsync(sessionId)).Count.ShouldBe(2);
    }

    private async Task<List<ChatMessageDto>> SaveAsync(
        Guid sessionId,
        string body,
        Guid operationId,
        bool isInternal = false,
        ExtraPropertyDictionary? extraProperties = null)
    {
        return await GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = body,
                OperationId = operationId,
                IsInternal = isInternal,
                ExtraProperties = extraProperties ?? new ExtraPropertyDictionary()
            });
    }

    private static SendChatMessageInput VisitorInput(Guid sessionId, string body)
    {
        return new SendChatMessageInput
        {
            SessionId = sessionId,
            Body = body,
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        };
    }

    private async Task<Guid> CreateSessionAsync()
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });
        return session.Id;
    }

    private async Task<List<ChatMessage>> ReloadAsync(Guid sessionId)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var messages = await GetRequiredService<IChatMessageRepository>().GetListBySessionAsync(
            sessionId,
            includeInternal: true,
            maxResultCount: 300);
        await uow.CompleteAsync();
        return messages;
    }
}

/// <summary>
/// sufi-chain#551: two saves that both miss the pre-insert lookup still leave one transcript.
/// The unique operation index rejects the second insert.
/// </summary>
public class AssistantTranscriptConflictTests : ChatApplicationTestBase<AssistantTranscriptConflictTestModule>
{
    [Fact]
    public async Task A_second_insert_for_the_same_operation_returns_the_first_transcript()
    {
        StaleAssistantOperationLookupRepository.Reset();
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        });
        var operationId = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1");
        var manager = GetRequiredService<ChatMessageManager>();

        var first = await manager.SaveAssistantTranscriptAsync(
            session.Id,
            "one turn",
            operationId,
            isAuthorizedOperator: true);
        var second = await manager.SaveAssistantTranscriptAsync(
            session.Id,
            "one turn",
            operationId,
            isAuthorizedOperator: true);

        second.Messages.Select(message => message.Id).ShouldBe(first.Messages.Select(message => message.Id));
        second.AlreadyStored.ShouldBeTrue();
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var count = await GetRequiredService<IChatMessageRepository>().GetCountBySessionAsync(session.Id, includeInternal: true);
        await uow.CompleteAsync();
        count.ShouldBe(1);
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptConflictTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<IChatMessageRepository, StaleAssistantOperationLookupRepository>());
    }
}

[DisableConventionalRegistration]
public class StaleAssistantOperationLookupRepository : EfCoreSufiComChatMessageRepository
{
    private static int _lookups;

    public StaleAssistantOperationLookupRepository(IDbContextProvider<ISufiComChatDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public static void Reset()
    {
        _lookups = 0;
    }

    public override async Task<List<ChatMessage>> GetListByAssistantOperationAsync(
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Increment(ref _lookups) <= 4)
        {
            return new List<ChatMessage>();
        }

        return await base.GetListByAssistantOperationAsync(operationId, cancellationToken);
    }
}
