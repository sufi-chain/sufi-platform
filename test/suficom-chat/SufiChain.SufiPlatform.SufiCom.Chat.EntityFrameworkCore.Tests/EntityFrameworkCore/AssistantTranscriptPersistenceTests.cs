using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: a split assistant reply reloads with the same characters, including
/// spaces and newlines at the cut, and a failed later part leaves nothing stored.
/// </summary>
public class AssistantTranscriptPersistenceTests : ChatApplicationTestBase<SufiComChatEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task A_space_at_the_16000_boundary_survives_reload()
    {
        await ReloadMatchesAsync(new string('a', ChatConsts.MaxMessageBodyLength - 1) + " " + "b");
    }

    [Fact]
    public async Task A_newline_at_the_boundary_survives_reload()
    {
        await ReloadMatchesAsync(new string('a', ChatConsts.MaxMessageBodyLength - 1) + "\n" + "b");
    }

    [Fact]
    public async Task A_whitespace_tail_is_stored_and_does_not_throw()
    {
        var body = new string('a', ChatConsts.MaxMessageBodyLength) + " \n";
        var sessionId = await CreateSessionAsync();

        var stored = await GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = body
            });

        stored.Count.ShouldBeGreaterThan(1);
        stored[stored.Count - 1].Body.ShouldBe(" \n");
        string.Concat((await ReloadAsync(sessionId)).Select(message => message.Body)).ShouldBe(body);
    }

    [Fact]
    public async Task A_whitespace_only_reply_throws_and_stores_nothing()
    {
        var sessionId = await CreateSessionAsync();

        await Should.ThrowAsync<Exception>(() =>
            GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
                new SendAssistantTranscriptInput
                {
                    SessionId = sessionId,
                    Body = " \n\t "
                }));

        (await ReloadAsync(sessionId)).ShouldBeEmpty();
    }

    [Fact]
    public async Task A_surrogate_pair_at_the_boundary_survives_reload()
    {
        var pair = "\uD83D\uDE00";
        var body = new string('a', ChatConsts.MaxMessageBodyLength - 1) + pair;
        var messages = await ReloadMatchesAsync(body);

        messages.ShouldAllBe(message =>
            message.Body.Length == 0 || !char.IsHighSurrogate(message.Body[message.Body.Length - 1]));
    }

    [Fact]
    public async Task Persian_text_at_the_boundary_survives_reload()
    {
        await ReloadMatchesAsync(new string('ا', ChatConsts.MaxMessageBodyLength - 1) + " سلام");
    }

    [Fact]
    public async Task The_first_part_keeps_extra_properties()
    {
        var body = new string('a', ChatConsts.MaxMessageBodyLength) + "b";
        var sessionId = await CreateSessionAsync();
        var marker = new ExtraPropertyDictionary { ["helpdesk.kb.proposal"] = "draft" };

        await GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = body,
                ExtraProperties = marker
            });

        var messages = await ReloadAsync(sessionId);
        messages.Count.ShouldBeGreaterThan(1);
        messages[0].ExtraProperties["helpdesk.kb.proposal"].ShouldBe("draft");
        messages.Skip(1).ShouldAllBe(message => !message.ExtraProperties.ContainsKey("helpdesk.kb.proposal"));
    }

    [Fact]
    public async Task A_normal_user_send_still_trims()
    {
        var sessionId = await CreateSessionAsync();

        await GetRequiredService<IChatMessageAppService>().SendAsync(new SendChatMessageInput
        {
            SessionId = sessionId,
            Body = "  hi  ",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        var messages = await ReloadAsync(sessionId);
        messages.Count.ShouldBe(1);
        messages[0].Body.ShouldBe("hi");
    }

    private async Task<List<ChatMessage>> ReloadMatchesAsync(string body)
    {
        var sessionId = await CreateSessionAsync();
        await GetRequiredService<IChatAssistantTranscriptWriter>().SaveForCurrentUserAsync(
            new SendAssistantTranscriptInput
            {
                SessionId = sessionId,
                Body = body
            });

        var messages = await ReloadAsync(sessionId);
        messages.Count.ShouldBeGreaterThan(1);
        messages.ShouldAllBe(message => message.Body.Length > 0 && message.Body.Length <= ChatConsts.MaxMessageBodyLength);
        string.Concat(messages.Select(message => message.Body)).ShouldBe(body);
        return messages;
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
            maxResultCount: 20);
        await uow.CompleteAsync();
        return messages;
    }
}

[DependsOn(typeof(SufiComChatEntityFrameworkCoreTestModule))]
public class AssistantTranscriptRollbackTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<FailOnSecondInsertChatMessageRepository>();
        context.Services.Replace(ServiceDescriptor.Transient<IChatMessageRepository, FailOnSecondInsertChatMessageRepository>());
    }
}

[DisableConventionalRegistration]
public class FailOnSecondInsertChatMessageRepository : EfCoreSufiComChatMessageRepository
{
    private int _inserts;

    public FailOnSecondInsertChatMessageRepository(IDbContextProvider<ISufiComChatDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<ChatMessage> InsertAsync(
        ChatMessage entity,
        bool autoSave = false,
        CancellationToken cancellationToken = default)
    {
        if (++_inserts >= 2)
        {
            throw new InvalidOperationException("later-part");
        }

        return await base.InsertAsync(entity, autoSave, cancellationToken);
    }
}

public class AssistantTranscriptRollbackTests : ChatApplicationTestBase<AssistantTranscriptRollbackTestModule>
{
    [Fact]
    public async Task A_later_part_failure_rolls_back_the_earlier_part()
    {
        var session = await GetRequiredService<IChatSessionAppService>().CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support
        });
        var body = new string('a', ChatConsts.MaxMessageBodyLength) + "b";

        await Should.ThrowAsync<InvalidOperationException>(() =>
            GetRequiredService<ChatMessageManager>().SaveAssistantTranscriptAsync(
                session.Id,
                body,
                isAuthorizedOperator: true));

        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var count = await GetRequiredService<IChatMessageRepository>().GetCountBySessionAsync(session.Id, includeInternal: true);
        await uow.CompleteAsync();
        count.ShouldBe(0);
    }
}
