using System.Reflection;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

/// <summary>
/// Portal assistant history must open on the newest page and keep load-earlier paging.
/// The application host uses the EF Core repository. The MongoDB test project reuses this fixture.
/// </summary>
public abstract class ChatMessageHistoryPaging_Tests<TStartupModule> : ChatApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatMessageAppService _messageAppService;
    private readonly IChatSessionAppService _sessionAppService;

    protected ChatMessageHistoryPaging_Tests()
    {
        _messageRepository = GetRequiredService<IChatMessageRepository>();
        _messageAppService = GetRequiredService<IChatMessageAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
    }

    [Fact]
    public async Task Repository_returns_the_latest_page_ascending_and_pages_older_messages()
    {
        var sessionId = await SeedTranscriptAsync();

        var latest = await _messageRepository.GetListBySessionAsync(
            sessionId,
            includeInternal: false,
            skipCount: 0,
            maxResultCount: 20);
        var older = await _messageRepository.GetListBySessionAsync(
            sessionId,
            includeInternal: false,
            skipCount: 20,
            maxResultCount: 20);
        var noneLeft = await _messageRepository.GetListBySessionAsync(
            sessionId,
            includeInternal: false,
            skipCount: 25,
            maxResultCount: 20);
        var withInternal = await _messageRepository.GetListBySessionAsync(
            sessionId,
            includeInternal: true,
            skipCount: 0,
            maxResultCount: 20);

        latest.Select(message => message.Body).ShouldBe(Bodies(5, 24));
        latest.ShouldBeInAscendingCreationOrder();
        older.Select(message => message.Body).ShouldBe(Bodies(0, 4));
        older.ShouldBeInAscendingCreationOrder();
        older.Last().CreationTime.ShouldBeLessThan(latest.First().CreationTime);
        noneLeft.ShouldBeEmpty();
        withInternal.Select(message => message.Body).Last().ShouldBe("internal-note");
        withInternal.Select(message => message.Body).ShouldNotContain("m05");
        (await _messageRepository.GetCountBySessionAsync(sessionId, includeInternal: false)).ShouldBe(25);
    }

    [Fact]
    public async Task AppService_returns_the_latest_page_ascending_and_pages_older_messages()
    {
        var sessionId = await SeedTranscriptAsync();

        var latest = await _messageAppService.GetListAsync(new GetChatMessageListInput
        {
            SessionId = sessionId,
            IncludeInternal = false,
            SkipCount = 0,
            MaxResultCount = 20
        });
        var older = await _messageAppService.GetListAsync(new GetChatMessageListInput
        {
            SessionId = sessionId,
            IncludeInternal = false,
            SkipCount = 20,
            MaxResultCount = 20
        });

        latest.TotalCount.ShouldBe(25);
        latest.Items.Select(message => message.Body).ShouldBe(Bodies(5, 24));
        latest.Items.ShouldBeInAscendingCreationOrder();
        older.TotalCount.ShouldBe(25);
        older.Items.Select(message => message.Body).ShouldBe(Bodies(0, 4));
        older.Items.ShouldBeInAscendingCreationOrder();
        older.Items.Last().CreationTime.ShouldBeLessThan(latest.Items.First().CreationTime);
    }

    private async Task<Guid> SeedTranscriptAsync()
    {
        ChatSessionDto session;
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Assistant,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }

        var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await WithUnitOfWorkAsync(async () =>
        {
            for (var index = 0; index < 25; index++)
            {
                await InsertAsync(session.Id, $"m{index:00}", isInternal: false, baseTime.AddMinutes(index));
            }

            await InsertAsync(session.Id, "internal-note", isInternal: true, baseTime.AddMinutes(40));
            await InsertAsync(Guid.NewGuid(), "other-session", isInternal: false, baseTime.AddMinutes(50));
        });

        return session.Id;
    }

    private async Task InsertAsync(Guid sessionId, string body, bool isInternal, DateTime creationTime)
    {
        var message = new ChatMessage(
            Guid.NewGuid(),
            tenantId: null,
            sessionId,
            body,
            ChatMessageSenderKind.Visitor,
            senderUserId: ChatTestData.UserAId,
            isInternal: isInternal);
        SetCreationTime(message, creationTime);
        await _messageRepository.InsertAsync(message, autoSave: true);
    }

    private async Task WithUnitOfWorkAsync(Func<Task> action)
    {
        using var unitOfWork = GetRequiredService<IUnitOfWorkManager>()
            .Begin(requiresNew: true, isTransactional: false);
        await action();
        await unitOfWork.CompleteAsync();
    }

    private static void SetCreationTime(ChatMessage message, DateTime creationTime)
    {
        var property = typeof(ChatMessage).GetProperty(nameof(ChatMessage.CreationTime))
            ?? throw new InvalidOperationException("ChatMessage.CreationTime was not found.");
        property.SetValue(message, creationTime);
    }

    private static string[] Bodies(int fromInclusive, int toInclusive)
    {
        return Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1)
            .Select(index => $"m{index:00}")
            .ToArray();
    }
}

public class EfCoreChatMessageHistoryPaging_Tests
    : ChatMessageHistoryPaging_Tests<SufiComChatApplicationTestModule>
{
}

internal static class ChatMessageHistoryOrderAssert
{
    public static void ShouldBeInAscendingCreationOrder(this IReadOnlyList<ChatMessage> messages)
    {
        for (var index = 1; index < messages.Count; index++)
        {
            messages[index].CreationTime.ShouldBeGreaterThan(messages[index - 1].CreationTime);
        }
    }

    public static void ShouldBeInAscendingCreationOrder(this IReadOnlyList<ChatMessageDto> messages)
    {
        for (var index = 1; index < messages.Count; index++)
        {
            messages[index].CreationTime.ShouldBeGreaterThan(messages[index - 1].CreationTime);
        }
    }
}
