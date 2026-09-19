using System.Text.Json;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiForms.Hooshvare;
using Volo.Abp;
using Volo.Abp.Authorization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormRecordHooshvareTests
{
    [Theory]
    [InlineData(SufiFormsHooshvareKeys.RecordAnalyst, true, null, true)]
    [InlineData(SufiFormsHooshvareKeys.RecordAssistant, true, null, true)]
    [InlineData(SufiFormsHooshvareKeys.RecordAssistant, false, null, false)]
    [InlineData(SufiFormsHooshvareKeys.RecordAnalyst, true, false, false)]
    [InlineData(SufiFormsHooshvareKeys.RecordAnalyst, false, true, true)]
    public async Task Saving_uses_the_catalog_default_or_explicit_bookmark_choice(
        string key, bool configured, bool? selected, bool expected)
    {
        var service = new Records();
        service.Settings.PersistChatSession = configured;
        var input = service.Input();
        input.HooshvareKey = key;
        input.PersistChatSession = selected;
        var reply = await service.SendAsync(input);

        await service.RuntimeMock.Received(1).SendAsync(Arg.Is<HooshvareRuntimeRequestDto>(request =>
            request.PersistChatSession == expected && request.SessionId == service.Session.Id), default);
        reply.SessionId.ShouldBe(service.Session.Id);
        reply.PersistedChatSession.ShouldBe(expected);
    }

    [Fact]
    public async Task Another_hooshvare_or_an_unowned_session_cannot_be_used()
    {
        var service = new Records();
        service.Session.AssistantId = Guid.NewGuid();
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.SendAsync(service.Input()));
        service.Chat.GetOwnedSessionAsync(service.Session.Id)
            .Returns(_ => Task.FromException<ChatSessionDto>(new AbpAuthorizationException()));
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.SendAsync(service.Input()));
        await service.RuntimeMock.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task Unrelated_hooshvares_are_rejected_before_catalog_or_runtime_access()
    {
        var service = new Records();
        var input = service.Input();
        input.HooshvareKey = SufiFormsHooshvareKeys.FormCreator;
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.SendAsync(input));
        await service.Catalog.DidNotReceiveWithAnyArgs().GetByKeyAsync(default!);
        await service.RuntimeMock.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task Attachments_require_an_owned_session_and_enabled_capabilities()
    {
        var service = new Records();
        var input = service.Input();
        input.AttachmentFileIds = [Guid.NewGuid()];
        input.SessionId = null;
        await Should.ThrowAsync<BusinessException>(() => service.SendAsync(input));
        input.SessionId = service.Session.Id;
        service.Settings.AllowComposerAttachment = false;
        await Should.ThrowAsync<BusinessException>(() => service.SendAsync(input));
        service.Settings.AllowComposerAttachment = true;
        service.Allowed.CanAttachFiles = false;
        await Should.ThrowAsync<BusinessException>(() => service.SendAsync(input));
        await service.Media.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default, default);
        await service.RuntimeMock.DidNotReceiveWithAnyArgs().SendAsync(default!, default);
    }

    [Fact]
    public async Task Images_survive_history_and_are_forwarded_for_saved_message_attachments()
    {
        var service = new Records();
        var fileId = Guid.NewGuid();
        var input = service.Input();
        input.AttachmentFileIds = [fileId];
        input.History = [new() { Content = "Earlier image", AttachmentFileIds = [fileId] }];
        service.Media.ResolveAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<SufiAIMediaResolutionContext>(), default)
            .Returns(new List<SufiAIMedia>
            {
                new() { FileId = fileId, Kind = SufiAIMediaKind.Image, MimeType = "image/png", Bytes = [1, 2, 3] }
            });
        await service.SendAsync(input);

        await service.Media.Received(2).ResolveAsync(Arg.Any<IEnumerable<Guid>>(),
            Arg.Is<SufiAIMediaResolutionContext>(context => context.SessionId == service.Session.Id &&
                context.TenantId == service.Session.TenantId && !context.AllowOperatorGallery), default);
        await service.RuntimeMock.Received(1).SendAsync(Arg.Is<HooshvareRuntimeRequestDto>(request =>
            request.AttachmentFileIds.Contains(fileId) && request.MessageContentParts.Count == 2 &&
            request.MessageContentParts[1].DataUrl == "data:image/png;base64,AQID" &&
            request.ConversationHistory[0].ContentParts.Count == 2), default);
    }

    [Fact]
    public async Task Audio_attachments_are_transcribed_in_the_configured_workspace()
    {
        var service = new Records();
        var input = service.Input();
        input.AttachmentFileIds = [Guid.NewGuid()];
        service.Media.ResolveAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<SufiAIMediaResolutionContext>(), default)
            .Returns(new List<SufiAIMedia>
            {
                new() { Kind = SufiAIMediaKind.Audio, MimeType = "audio/webm;codecs=opus", Bytes = [1, 2] }
            });
        service.Audio.TranscribeAsync(Arg.Any<SufiAITranscriptionRequest>(), default)
            .Returns(new SufiAITranscriptionResponse { Text = "Find pending records" });
        await service.SendAsync(input);
        await service.Audio.Received(1).TranscribeAsync(Arg.Is<SufiAITranscriptionRequest>(request =>
            request.WorkspaceName == service.Settings.WorkspaceName && request.AudioFormat == "webm"), default);
        await service.RuntimeMock.Received(1).SendAsync(Arg.Is<HooshvareRuntimeRequestDto>(request =>
            request.Message.EndsWith("\nFind pending records")), default);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Assistant_context_keeps_confirmation_record_schema_and_selected_model(bool confirmed)
    {
        var service = new Records();
        var input = service.Input();
        input.HooshvareKey = SufiFormsHooshvareKeys.RecordAssistant;
        input.Confirmed = confirmed;
        input.RecordId = "record-123";
        input.SchemaVersion = "4";
        input.CurrentValues = "{\"status\":\"pending\"}";
        input.ModelConfigurationId = Guid.NewGuid();
        HooshvareRuntimeRequestDto? sent = null;
        service.RuntimeMock.SendAsync(Arg.Do<HooshvareRuntimeRequestDto>(value => sent = value), default)
            .Returns(new HooshvareRuntimeResultDto { SessionId = service.Session.Id });
        await service.SendAsync(input);
        sent.ShouldNotBeNull();
        sent.ModelConfigurationId.ShouldBe(input.ModelConfigurationId);
        using var metadata = JsonDocument.Parse(sent.MetadataJson!);
        var context = metadata.RootElement.GetProperty("hooshvareContext");
        context.GetProperty("formKey").GetString().ShouldBe(input.FormKey);
        context.GetProperty("confirmation").GetString().ShouldBe(confirmed ? "confirmed" : "preview");
        context.GetProperty("recordId").GetString().ShouldBe(input.RecordId);
        context.GetProperty("schemaVersion").GetString().ShouldBe(input.SchemaVersion);
        context.GetProperty("currentValues").GetString().ShouldBe(input.CurrentValues);
    }

    [Fact]
    public void Attachment_ids_cannot_be_injected_through_the_public_runtime_request()
    {
        var fileId = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new HooshvareRuntimeRequestDto { AttachmentFileIds = [fileId] });
        json.ShouldNotContain(nameof(HooshvareRuntimeRequestDto.AttachmentFileIds));
        JsonSerializer.Deserialize<HooshvareRuntimeRequestDto>(
            "{\"AttachmentFileIds\":[\"" + fileId + "\"]}")!.AttachmentFileIds.ShouldBeEmpty();
    }

    private sealed class Records : FormRecordHooshvareAppService
    {
        public IHooshvareRuntimeAppService RuntimeMock { get; } = Substitute.For<IHooshvareRuntimeAppService>();
        public IChatIntegrationService Chat { get; } = Substitute.For<IChatIntegrationService>();
        public IHooshvareCatalogAppService Catalog { get; } = Substitute.For<IHooshvareCatalogAppService>();
        public IChatComposerCapabilitiesAppService Capabilities { get; } = Substitute.For<IChatComposerCapabilitiesAppService>();
        public ISufiAIMediaResolver Media { get; } = Substitute.For<ISufiAIMediaResolver>();
        public ISufiAIAudioService Audio { get; } = Substitute.For<ISufiAIAudioService>();
        public HooshvareCatalogItemDto Settings { get; } = new()
        {
            Id = Guid.NewGuid(), AllowComposerAttachment = true, WorkspaceName = "forms-workspace"
        };
        public ChatComposerCapabilitiesDto Allowed { get; } = new()
        {
            CanAttachFiles = true, MaxFilesPerMessage = 10, AllowedFileTypes = ChatAttachmentAllowedFileTypes.All
        };
        public ChatSessionDto Session { get; } = new()
        {
            Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), ConversationKind = ConversationKind.Assistant,
            IsExternallyOrchestrated = true
        };

        public Records()
        {
            Session.AssistantId = Settings.Id;
            RuntimeMock.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), default)
                .Returns(new HooshvareRuntimeResultDto { Message = "Ready", SessionId = Session.Id });
            Chat.GetOwnedSessionAsync(Session.Id).Returns(Session);
            Catalog.GetByKeyAsync(Arg.Any<string>()).Returns(Settings);
            Capabilities.GetAsync(Session.Id).Returns(Allowed);
        }

        public FormRecordHooshvareInput Input() => new()
        {
            Message = "Find records", HooshvareKey = SufiFormsHooshvareKeys.RecordAnalyst,
            FormKey = "orders", SessionId = Session.Id
        };

        protected override IHooshvareRuntimeAppService Runtime => RuntimeMock;
        protected override IChatIntegrationService ComposerChat => Chat;
        protected override IHooshvareCatalogAppService ComposerCatalog => Catalog;
        protected override IChatComposerCapabilitiesAppService ComposerCapabilities => Capabilities;
        protected override ISufiAIMediaResolver ComposerMedia => Media;
        protected override ISufiAIAudioService ComposerAudio => Audio;
    }
}
