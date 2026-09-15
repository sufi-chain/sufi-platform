using NSubstitute;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.Settings;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatComposerMessageSend_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatMessageAppService _messageAppService;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly ISettingManager _settingManager;

    public ChatComposerMessageSend_Tests()
    {
        _messageAppService = GetRequiredService<IChatMessageAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
    }

    [Fact]
    public async Task Should_Send_Message_With_Attachment()
    {
        await EnableAttachmentSettingsAsync();
        await ChatTestSettingHelper.SetAuthenticatedUsagePolicyAsync(_settingManager);
        var session = await CreateAuthenticatedSessionAsync();
        var fileId = Guid.NewGuid();

        var fileStorageIntegrationService = GetRequiredService<IFileStorageIntegrationService>();
        fileStorageIntegrationService.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            StructureKey = ChatFileStructureKeys.Attachments,
            EntityType = ChatEntityTypes.Session,
            EntityId = session.Id,
            SizeInBytes = 2048,
            FileName = "photo.jpg",
            MimeType = "image/jpeg"
        });

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var message = await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = "See attached",
                AttachmentFileIds = { fileId },
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAuthenticated
            });

            message.AttachmentFileIds.ShouldContain(fileId);
        }
    }

    [Fact]
    public async Task Should_Send_Location_Message_With_Empty_Body()
    {
        await EnableAttachmentSettingsAsync();
        await ChatTestSettingHelper.SetAuthenticatedUsagePolicyAsync(_settingManager);
        var session = await CreateAuthenticatedSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var metadata = ChatMessageMetadata.CreateLocation(35.6892, 51.3890, 10, "Office");

            var message = await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = string.Empty,
                Metadata = metadata,
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAuthenticated
            });

            var persistedMetadata = ChatMessageMetadata.FromExtraProperties(message.ExtraProperties);
            persistedMetadata.ShouldNotBeNull();
            persistedMetadata.ContentKind.ShouldBe(ChatMessageContentKind.Location);
            persistedMetadata.Location.ShouldNotBeNull();
            persistedMetadata.Location.Latitude.ShouldBe(35.6892);
            persistedMetadata.Location.Longitude.ShouldBe(51.3890);
            persistedMetadata.Location.AccuracyMeters.ShouldBe(10);
            persistedMetadata.Location.Label.ShouldBe("Office");
            message.Body.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task Should_Reject_Attachments_When_Disabled()
    {
        await _settingManager.SetGlobalAsync(ChatSettingNames.General.EnableFileAttachments, false.ToString());
        await ChatTestSettingHelper.SetAuthenticatedUsagePolicyAsync(_settingManager);

        var session = await CreateAuthenticatedSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _messageAppService.SendAsync(new SendChatMessageInput
                {
                    SessionId = session.Id,
                    Body = "See file",
                    AttachmentFileIds = { Guid.NewGuid() },
                    SenderKind = ChatMessageSenderKind.Visitor,
                    AccessMode = AccessMode.PublicAuthenticated
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.AttachmentsDisabled);
        }
    }

    [Fact]
    public async Task Should_Reject_Location_When_Disabled()
    {
        await EnableAttachmentSettingsAsync();
        await ChatTestSettingHelper.SetAuthenticatedUsagePolicyAsync(_settingManager);
        await _settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableLocationSharing, false.ToString());

        var session = await CreateAuthenticatedSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _messageAppService.SendAsync(new SendChatMessageInput
                {
                    SessionId = session.Id,
                    Body = string.Empty,
                    Metadata = ChatMessageMetadata.CreateLocation(1, 2),
                    SenderKind = ChatMessageSenderKind.Visitor,
                    AccessMode = AccessMode.PublicAuthenticated
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.LocationSharingDisabled);
        }
    }

    private async Task EnableAttachmentSettingsAsync()
    {
        await _settingManager.SetGlobalAsync(ChatSettingNames.General.EnableFileAttachments, true.ToString());
        await _settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableLocationSharing, true.ToString());
        await _settingManager.SetGlobalAsync(ChatSettingNames.Attachments.EnableVoiceMessages, true.ToString());
    }

    private async Task<ChatSessionDto> CreateAuthenticatedSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Direct,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    },
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserBId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }
    }
}
