using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
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

    public ChatComposerTranscribe_Tests()
    {
        _uploadAppService = GetRequiredService<IChatComposerUploadAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _fileStorage = GetRequiredService<IFileStorageIntegrationService>();
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
    public async Task Transcribe_Should_Not_Persist_When_Audio_Service_Is_Unavailable()
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
