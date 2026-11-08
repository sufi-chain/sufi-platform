using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Composer;

public class ChatComposerTranscribe_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatComposerUploadAppService _uploadAppService;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly ISettingManager _settingManager;
    private readonly IFileStorageIntegrationService _fileStorage;
    private readonly ISufiAIAudioService _audio;

    public ChatComposerTranscribe_Tests()
    {
        _uploadAppService = GetRequiredService<IChatComposerUploadAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _fileStorage = GetRequiredService<IFileStorageIntegrationService>();
        _audio = GetRequiredService<ISufiAIAudioService>();
    }

    [Fact]
    public async Task Transcribe_Should_Reject_Direct_Session()
    {
        await EnableVoiceAsync();
        var session = await CreateSessionAsync(ConversationKind.Direct);

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _uploadAppService.TranscribeAsync(new ChatComposerTranscribeInput
                {
                    SessionId = session.Id,
                    FileName = "voice.webm",
                    MimeType = "audio/webm",
                    Content = new byte[] { 1, 2, 3 }
                }));

            exception.Code.ShouldBe(ChatErrorCodes.VoiceTranscriptionRequiresAssistantSession);
        }

        await _fileStorage.DidNotReceiveWithAnyArgs().UploadAsync(default!);
    }

    [Fact]
    public async Task Transcribe_Should_Reject_Empty_Audio_Without_Uploading()
    {
        await EnableVoiceAsync();
        var session = await CreateSessionAsync(ConversationKind.Assistant);

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _uploadAppService.TranscribeAsync(new ChatComposerTranscribeInput
                {
                    SessionId = session.Id,
                    FileName = "voice.webm",
                    MimeType = "audio/webm",
                    Content = Array.Empty<byte>()
                }));

            exception.Code.ShouldBe(ChatErrorCodes.TranscriptionFailed);
        }

        await _fileStorage.DidNotReceiveWithAnyArgs().UploadAsync(default!);
    }

    [Fact]
    public async Task Transcribe_Should_Not_Call_The_Provider_When_Ai_Is_Off()
    {
        await EnableVoiceAsync();
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);
        _audio.ClearReceivedCalls();
        var session = await CreateSessionAsync(ConversationKind.Assistant);

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _uploadAppService.TranscribeAsync(new ChatComposerTranscribeInput
                {
                    SessionId = session.Id,
                    FileName = "voice.webm",
                    MimeType = "audio/webm",
                    Content = [1, 2, 3, 4]
                }));

            exception.Code.ShouldBe(ChatErrorCodes.AiUnavailable);
        }

        await _audio.DidNotReceiveWithAnyArgs().TranscribeAsync(default!, default);
        await _audio.DidNotReceiveWithAnyArgs().IsAvailableAsync(default);
    }

    [Fact]
    public async Task Transcribe_Should_Return_The_Mocked_Text_When_Ai_Is_On()
    {
        await EnableVoiceAsync();
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
        _audio.ClearReceivedCalls();
        _audio.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _audio.TranscribeAsync(Arg.Any<SufiAITranscriptionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAITranscriptionResponse { Text = "spoken hello" });
        var session = await CreateSessionAsync(ConversationKind.Assistant);

        try
        {
            using (CurrentUser.Change(ChatTestData.UserAId))
            {
                var result = await _uploadAppService.TranscribeAsync(new ChatComposerTranscribeInput
                {
                    SessionId = session.Id,
                    FileName = "voice.webm",
                    MimeType = "audio/webm",
                    Content = [1, 2, 3, 4]
                });

                result.Text.ShouldBe("spoken hello");
            }

            await _audio.Received(1).TranscribeAsync(
                Arg.Any<SufiAITranscriptionRequest>(),
                Arg.Any<CancellationToken>());
        }
        finally
        {
            _audio.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);
            _audio.TranscribeAsync(Arg.Any<SufiAITranscriptionRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<SufiAITranscriptionResponse>(null!));
        }
    }

    [Fact]
    public async Task Transcribe_Should_Not_Persist_When_Audio_Service_Is_Unavailable()
    {
        await EnableVoiceAsync();
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);
        _audio.ClearReceivedCalls();
        _audio.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);
        var session = await CreateSessionAsync(ConversationKind.Assistant);

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _uploadAppService.TranscribeAsync(new ChatComposerTranscribeInput
                {
                    SessionId = session.Id,
                    FileName = "voice.webm",
                    MimeType = "audio/webm",
                    Content = new byte[] { 1, 2, 3, 4 }
                }));

            exception.Code.ShouldBe(ChatErrorCodes.TranscriptionUnavailable);
        }

        await _fileStorage.DidNotReceiveWithAnyArgs().UploadAsync(default!);
    }

    private async Task EnableVoiceAsync()
    {
        await _settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableVoiceMessages, true.ToString());
    }

    private async Task<ChatSessionDto> CreateSessionAsync(ConversationKind kind)
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = kind,
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
    }
}
