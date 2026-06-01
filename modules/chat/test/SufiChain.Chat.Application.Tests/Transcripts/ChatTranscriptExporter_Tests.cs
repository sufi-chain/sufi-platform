using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Transcripts;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.Transcripts;

public class ChatTranscriptExporter_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatTranscriptExporter _transcriptExporter;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatMessageAppService _messageAppService;

    public ChatTranscriptExporter_Tests()
    {
        _transcriptExporter = GetRequiredService<IChatTranscriptExporter>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _messageAppService = GetRequiredService<IChatMessageAppService>();
    }

    [Fact]
    public async Task Should_Export_Transcript_In_Chronological_Order()
    {
        var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
        {
            Title = "Transcript session",
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

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "First",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        await Task.Delay(10);

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "Second",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        var transcript = await _transcriptExporter.ExportAsync(session.Id);
        transcript.Messages.Count.ShouldBe(2);
        transcript.Messages[0].Body.ShouldBe("First");
        transcript.Messages[1].Body.ShouldBe("Second");
        transcript.Messages[0].CreationTime.ShouldBeLessThanOrEqualTo(transcript.Messages[1].CreationTime);

        var plainText = await _transcriptExporter.ExportAsPlainTextAsync(session.Id);
        plainText.IndexOf("First", StringComparison.Ordinal).ShouldBeLessThan(plainText.IndexOf("Second", StringComparison.Ordinal));
    }
}
