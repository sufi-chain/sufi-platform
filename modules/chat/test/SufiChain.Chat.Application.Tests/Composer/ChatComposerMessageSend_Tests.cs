using NSubstitute;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Settings;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Volo.Abp;
using Volo.Abp.SettingManagement;
using Xunit;

namespace SufiChain.Chat.Messages;

public class ChatComposerMessageSend_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
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

        var fileItemAppService = GetRequiredService<IFileItemAppService>();
        fileItemAppService.GetAsync(fileId).Returns(new FileItemDto
        {
            Id = fileId,
            StructureKey = ChatFileStructureKeys.Attachments,
            EntityType = ChatEntityTypes.Session,
            EntityId = session.Id,
            Size = 2048,
            Name = "photo.jpg",
            OriginalName = "photo.jpg",
            BlobName = "photo.jpg",
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
            var metadataJson = ChatMessageMetadata.BuildLocationJson(35.6892, 51.3890, 10, "Office");

            var message = await _messageAppService.SendAsync(new SendChatMessageInput
            {
                SessionId = session.Id,
                Body = string.Empty,
                MetadataJson = metadataJson,
                SenderKind = ChatMessageSenderKind.Visitor,
                AccessMode = AccessMode.PublicAuthenticated
            });

            message.MetadataJson.ShouldBe(metadataJson);
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
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
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
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _messageAppService.SendAsync(new SendChatMessageInput
                {
                    SessionId = session.Id,
                    Body = string.Empty,
                    MetadataJson = ChatMessageMetadata.BuildLocationJson(1, 2),
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
