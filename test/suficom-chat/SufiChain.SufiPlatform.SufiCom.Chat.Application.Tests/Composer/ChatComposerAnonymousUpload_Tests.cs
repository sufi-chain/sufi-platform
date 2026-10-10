using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Features;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Composer;

public class ChatComposerAnonymousUpload_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatComposerUploadAppService _uploadAppService;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly ISettingManager _settingManager;
    private readonly IFileStorageTrustedService _trustedStorage;

    public ChatComposerAnonymousUpload_Tests()
    {
        _uploadAppService = GetRequiredService<IChatComposerUploadAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _trustedStorage = GetRequiredService<IFileStorageTrustedService>();
        _trustedStorage.UploadAsync(Arg.Any<FileUploadRequest>())
            .Returns(new FileReferenceDto
            {
                Id = Guid.NewGuid(),
                FileName = "note.txt",
                MimeType = "text/plain",
                SizeInBytes = 4
            });
    }

    [Fact]
    public async Task Anonymous_support_visitor_can_upload_through_the_trusted_file_service()
    {
        await _settingManager.SetGlobalAsync(ChatSettingNames.General.EnableFileAttachments, true.ToString());
        var session = await CreateSessionAsync(AccessMode.PublicAnonymous, ConversationKind.Support);

        FileReferenceDto uploaded;
        using (CurrentUser.Change(null))
        {
            uploaded = await _uploadAppService.UploadAsync(new ChatComposerUploadInput
            {
                SessionId = session.Id,
                FileName = "note.txt",
                MimeType = "text/plain",
                Content = [1, 2, 3, 4]
            });
        }

        uploaded.FileName.ShouldBe("note.txt");
        await _trustedStorage.Received(1).UploadAsync(Arg.Is<FileUploadRequest>(file =>
            file.FileName == "note.txt" &&
            file.EntityId == session.Id &&
            file.StructureKey == ChatFileStructureKeys.Attachments));
    }

    [Fact]
    public async Task Anonymous_upload_is_still_denied_for_a_session_that_is_not_public_support()
    {
        // The chat test host grants every permission, so this case is exercised on a
        // service whose authorization service denies send and inbox grants.
        var sessionId = Guid.NewGuid();
        var storage = Substitute.For<IFileStorageTrustedService>();
        var service = ChatComposerUploadPolicyHarness.Create(
            storage,
            new ChatSession(
                sessionId,
                tenantId: null,
                title: null,
                AccessMode.PublicAuthenticated,
                ConversationKind.Direct,
                ChannelOrigin.Widget));

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.UploadAsync(
            new ChatComposerUploadInput
            {
                SessionId = sessionId,
                FileName = "note.txt",
                MimeType = "text/plain",
                Content = [1, 2, 3, 4]
            }));

        await storage.DidNotReceive().UploadAsync(Arg.Any<FileUploadRequest>());
    }

    private async Task<ChatSessionDto> CreateSessionAsync(AccessMode accessMode, ConversationKind kind)
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = accessMode,
                ConversationKind = kind,
                ChannelOrigin = ChannelOrigin.Widget,
                AnonymousVisitorId = accessMode == AccessMode.PublicAnonymous
                    ? ChatTestData.AnonymousVisitorId
                    : null,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = accessMode == AccessMode.PublicAnonymous ? null : ChatTestData.UserAId,
                        AnonymousVisitorId = accessMode == AccessMode.PublicAnonymous
                            ? ChatTestData.AnonymousVisitorId
                            : null,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }
    }
}

/// <summary>
/// Authorization policy without the test host's always-allow checker.
/// A caller with no grants may upload only for a public anonymous support session.
/// </summary>
public class ChatComposerUploadPolicy_Tests
{
    [Fact]
    public async Task Anonymous_public_support_upload_reaches_trusted_storage_without_grants()
    {
        var sessionId = Guid.NewGuid();
        var storage = Substitute.For<IFileStorageTrustedService>();
        storage.UploadAsync(Arg.Any<FileUploadRequest>())
            .Returns(new FileReferenceDto
            {
                Id = Guid.NewGuid(),
                FileName = "note.txt",
                MimeType = "text/plain",
                SizeInBytes = 4
            });
        var service = ChatComposerUploadPolicyHarness.Create(
            storage,
            new ChatSession(
                sessionId,
                tenantId: null,
                title: null,
                AccessMode.PublicAnonymous,
                ConversationKind.Support,
                ChannelOrigin.Widget));

        var uploaded = await service.UploadAsync(new ChatComposerUploadInput
        {
            SessionId = sessionId,
            FileName = "note.txt",
            MimeType = "text/plain",
            Content = [1, 2, 3, 4]
        });

        uploaded.FileName.ShouldBe("note.txt");
        await storage.Received(1).UploadAsync(Arg.Is<FileUploadRequest>(file =>
            file.EntityId == sessionId &&
            file.StructureKey == ChatFileStructureKeys.Attachments));
    }
}

internal static class ChatComposerUploadPolicyHarness
{
    public static ChatComposerUploadAppService Create(IFileStorageTrustedService storage, ChatSession session)
    {
        var sessions = Substitute.For<IChatSessionRepository>();
        sessions.FindAsync(session.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(session);
        sessions.GetAsync(session.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(session);

        var participants = Substitute.For<IChatParticipantRepository>();
        participants.IsParticipantAsync(
                Arg.Any<Guid>(),
                Arg.Any<Guid?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(ChatFeatures.Attachments).Returns(true);

        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns("true");

        var user = Substitute.For<ICurrentUser>();
        user.Id.Returns((Guid?)null);
        user.IsAuthenticated.Returns(false);

        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.CurrentPrincipal.Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        authorization
            .AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object?>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Failed());

        var tenant = Substitute.For<ICurrentTenant>();
        var guids = Substitute.For<IGuidGenerator>();
        guids.Create().Returns(Guid.NewGuid());
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 10, 10, 14, 30, 0, DateTimeKind.Utc));
        var bus = Substitute.For<IDistributedEventBus>();
        bus.PublishAsync(Arg.Any<ChatAttachmentUploadedEto>(), Arg.Any<bool>(), Arg.Any<bool>())
            .Returns(Task.CompletedTask);

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ISettingProvider>().Returns(settings);
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
        lazy.LazyGetRequiredService<IAuthorizationService>().Returns(authorization);
        lazy.LazyGetRequiredService<ICurrentTenant>().Returns(tenant);
        lazy.LazyGetRequiredService<IGuidGenerator>().Returns(guids);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);

        return new ChatComposerUploadAppService(
            storage,
            sessions,
            participants,
            features,
            bus,
            Substitute.For<ISufiAIAudioService>())
        {
            LazyServiceProvider = lazy
        };
    }
}
