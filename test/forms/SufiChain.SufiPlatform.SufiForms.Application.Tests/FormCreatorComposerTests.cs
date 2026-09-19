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

using SufiChain.SufiPlatform.SufiForms.Forms;

using Volo.Abp;

using Volo.Abp.Authorization;

using Xunit;



namespace SufiChain.SufiPlatform.SufiForms;



public class FormCreatorComposerTests

{

    [Fact]

    public async Task Attachments_without_an_owned_session_never_reach_the_provider()

    {

        var service = new Creator();

        var error = await Should.ThrowAsync<BusinessException>(() => service.ConverseAsync(new()

        {

            Message = "Use this image", AttachmentFileIds = [Guid.NewGuid()]

        }));

        error.Code.ShouldBe(SufiAIMediaErrorCodes.Unauthorized);

        await service.Runtime.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

    }



    [Fact]

    public async Task Another_hooshvares_session_is_rejected()

    {

        var service = new Creator();

        service.Session.AssistantId = Guid.NewGuid();

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.ConverseAsync(new()

        {

            Message = "Describe a form", SessionId = service.Session.Id

        }));

        await service.Runtime.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

    }



    [Fact]

    public async Task Disabled_attachments_are_rejected_before_reading_media()

    {

        var service = new Creator();

        service.Settings.AllowComposerAttachment = false;

        await Should.ThrowAsync<BusinessException>(() => service.ConverseAsync(new()

        {

            Message = "Use this image", SessionId = service.Session.Id, AttachmentFileIds = [Guid.NewGuid()]

        }));

        await service.Media.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default, default);

        await service.Runtime.DidNotReceiveWithAnyArgs().SendAsync(default!, default);

    }



    [Fact]

    public async Task Images_are_scoped_to_the_session_and_preserved_in_current_and_previous_turns()

    {

        var service = new Creator();

        var fileId = Guid.NewGuid();

        service.Media.ResolveAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<SufiAIMediaResolutionContext>(), default)

            .Returns(new List<SufiAIMedia>

            {

                new() { FileId = fileId, Kind = SufiAIMediaKind.Image, MimeType = "image/png", Bytes = [1, 2, 3] }

            });

        await service.ConverseAsync(new()

        {

            Message = "Use this layout", SessionId = service.Session.Id, AttachmentFileIds = [fileId],

            History = [new() { Content = "Earlier layout", AttachmentFileIds = [fileId] }]

        });



        await service.Media.Received(2).ResolveAsync(Arg.Any<IEnumerable<Guid>>(),

            Arg.Is<SufiAIMediaResolutionContext>(context => context.SessionId == service.Session.Id &&

                context.TenantId == service.Session.TenantId && !context.AllowOperatorGallery), default);

        await service.Runtime.Received(1).SendAsync(Arg.Is<HooshvareRuntimeRequestDto>(request =>

            request.SessionId == service.Session.Id && request.MessageContentParts.Count == 2 &&

            request.MessageContentParts[0].Text == "Use this layout" &&

            request.MessageContentParts[1].DataUrl == "data:image/png;base64,AQID" &&

            request.ConversationHistory[0].ContentParts.Count == 2), default);

    }



    [Fact]

    public async Task Audio_attachments_are_transcribed_before_the_form_request()

    {

        var service = new Creator();

        service.Settings.WorkspaceName = "forms-workspace";

        service.Media.ResolveAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<SufiAIMediaResolutionContext>(), default)

            .Returns(new List<SufiAIMedia>

            {

                new() { Kind = SufiAIMediaKind.Audio, MimeType = "audio/webm;codecs=opus", Bytes = [1, 2] }

            });

        service.Audio.TranscribeAsync(Arg.Any<SufiAITranscriptionRequest>(), default)

            .Returns(new SufiAITranscriptionResponse { Text = "Include an email field" });



        await service.ConverseAsync(new()

        {

            Message = "Requirements", SessionId = service.Session.Id, AttachmentFileIds = [Guid.NewGuid()]

        });



        await service.Audio.Received(1).TranscribeAsync(Arg.Is<SufiAITranscriptionRequest>(request =>

            request.WorkspaceName == "forms-workspace" && request.AudioFormat == "webm"), default);

        await service.Runtime.Received(1).SendAsync(Arg.Is<HooshvareRuntimeRequestDto>(request =>

            request.Message == "Requirements\nInclude an email field"), default);

    }



    private sealed class Creator : FormCreatorAppService

    {

        public IHooshvareRuntimeAppService Runtime { get; }

        public IChatIntegrationService Chat { get; } = Substitute.For<IChatIntegrationService>();

        public IHooshvareCatalogAppService Catalog { get; } = Substitute.For<IHooshvareCatalogAppService>();

        public IChatComposerCapabilitiesAppService Capabilities { get; } = Substitute.For<IChatComposerCapabilitiesAppService>();

        public ISufiAIMediaResolver Media { get; } = Substitute.For<ISufiAIMediaResolver>();

        public ISufiAIAudioService Audio { get; } = Substitute.For<ISufiAIAudioService>();

        public HooshvareCatalogItemDto Settings { get; } = new() { AllowComposerAttachment = true };

        public ChatSessionDto Session { get; } = new()

        {

            Id = Guid.NewGuid(), TenantId = Guid.NewGuid(), AssistantId = Guid.Empty,

            ConversationKind = ConversationKind.Assistant, IsExternallyOrchestrated = true

        };



        public Creator() : this(Substitute.For<IHooshvareRuntimeAppService>()) { }

        private Creator(IHooshvareRuntimeAppService runtime)

            : base(Substitute.For<IPlatformHooshvareResolver>(), runtime, Substitute.For<IFormDefinitionAppService>())

        {

            Runtime = runtime;

            runtime.SendAsync(Arg.Any<HooshvareRuntimeRequestDto>(), default)

                .Returns(new HooshvareRuntimeResultDto { Message = "{\"message\":\"Ready\",\"proposal\":null}" });

            Chat.GetOwnedSessionAsync(Session.Id).Returns(Session);

            Catalog.GetByKeyAsync(SufiFormsHooshvareKeys.FormCreator).Returns(Settings);

            Capabilities.GetAsync(Session.Id).Returns(new ChatComposerCapabilitiesDto

            {

                CanAttachFiles = true, MaxFilesPerMessage = 10, AllowedFileTypes = ChatAttachmentAllowedFileTypes.All

            });

        }

        protected override IChatIntegrationService ComposerChat => Chat;

        protected override IHooshvareCatalogAppService ComposerCatalog => Catalog;

        protected override IChatComposerCapabilitiesAppService ComposerCapabilities => Capabilities;

        protected override ISufiAIMediaResolver ComposerMedia => Media;

        protected override ISufiAIAudioService ComposerAudio => Audio;

        protected override Task CheckOperationAsync(Guid? id) => Task.CompletedTask;

    }

}

