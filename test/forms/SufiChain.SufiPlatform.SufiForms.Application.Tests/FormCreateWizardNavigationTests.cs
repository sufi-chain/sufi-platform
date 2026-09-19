using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiForms.Blazor.Components;
using SufiChain.SufiPlatform.SufiForms.Copilots;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreateWizardNavigationTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public async Task Rich_tools_follow_copilot_flags_and_platform_capabilities(bool audio, bool attachments, bool platformEnabled)
    {
        var catalog = Substitute.For<ICopilotCatalogAppService>();
        catalog.GetByKeyAsync(SufiFormsCopilotKeys.FormCreator).Returns(new CopilotCatalogItemDto
        {
            AllowComposerAudio = audio, AllowComposerAttachment = attachments, AllowComposerEmoji = false
        });
        var creator = Substitute.For<IFormCreatorAppService>();
        var sessionId = Guid.NewGuid();
        creator.CreateComposerSessionAsync().Returns(sessionId);
        var capabilities = Substitute.For<IChatComposerCapabilitiesAppService>();
        capabilities.GetAsync(sessionId).Returns(new ChatComposerCapabilitiesDto
        {
            CanUseRichComposer = true, CanRecordVoice = platformEnabled, CanAttachFiles = platformEnabled,
            AllowedFileTypes = SufiChain.SufiPlatform.SufiCom.Chat.ChatAttachmentAllowedFileTypes.All
        });
        var wizard = new TestWizard(creator, catalog, capabilities);

        await wizard.OpenAsync(true);

        wizard.SessionId.ShouldBe(sessionId);
        wizard.Capabilities!.CanRecordVoice.ShouldBe(audio && platformEnabled);
        wizard.Capabilities.CanAttachFiles.ShouldBe(attachments && platformEnabled);
        await creator.Received(1).CreateComposerSessionAsync();
    }

    [Theory]
    [InlineData("AICopilots:CopilotNotFound")]
    [InlineData("AICopilots:CopilotDisabled")]
    public async Task Unavailable_copilot_finishes_loading_without_blocking_manual_creation(string code)
    {
        var catalog = Substitute.For<ICopilotCatalogAppService>();
        catalog.GetByKeyAsync(SufiFormsCopilotKeys.FormCreator)
            .Returns(Task.FromException<CopilotCatalogItemDto>(new Volo.Abp.BusinessException(code)));
        var wizard = new TestWizard(Substitute.For<IFormCreatorAppService>(), catalog);

        await wizard.OpenAsync(true);

        wizard.Settings.ShouldBeNull();
        wizard.IsLoadingCatalog.ShouldBeFalse();
        wizard.Prompts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Missing_copilot_permission_allows_manual_creation()
    {
        var catalog = Substitute.For<ICopilotCatalogAppService>();
        catalog.GetByKeyAsync(SufiFormsCopilotKeys.FormCreator)
            .Returns(Task.FromException<CopilotCatalogItemDto>(new Volo.Abp.Authorization.AbpAuthorizationException()));
        var wizard = new TestWizard(Substitute.For<IFormCreatorAppService>(), catalog);

        await wizard.OpenAsync(true);

        wizard.Settings.ShouldBeNull();
        wizard.IsLoadingCatalog.ShouldBeFalse();
    }

    [Fact]
    public async Task Opening_loads_current_copilot_options_and_reopening_resets_model_selection()
    {
        var catalog = Substitute.For<ICopilotCatalogAppService>();
        var settings = new CopilotCatalogItemDto
        {
            Id = Guid.NewGuid(), WorkspaceId = Guid.NewGuid(), AllowUserModelSelection = true,
            AllowComposerEmoji = true, ShortcutPromptsJson = "[\"Registration form\"]"
        };
        catalog.GetByKeyAsync(SufiFormsCopilotKeys.FormCreator).Returns(settings);
        var wizard = new TestWizard(Substitute.For<IFormCreatorAppService>(), catalog);
        await wizard.OpenAsync(true);
        wizard.Settings.ShouldBeSameAs(settings);
        wizard.Prompts.ShouldContain("Registration form");
        wizard.EmojiAvailable.ShouldBeTrue();
        var model = Guid.NewGuid();
        wizard.SelectModel(model);
        wizard.SelectedModel.ShouldBe(model);
        wizard.SetBusy(true);
        wizard.SelectModel(Guid.NewGuid());
        wizard.SelectedModel.ShouldBe(model);
        wizard.SetBusy(false);
        await wizard.OpenAsync(false);
        settings.AllowUserModelSelection = false;
        await wizard.OpenAsync(true);
        wizard.SelectedModel.ShouldBeNull();
        wizard.Settings!.AllowUserModelSelection.ShouldBeFalse();
        await catalog.Received(2).GetByKeyAsync(SufiFormsCopilotKeys.FormCreator);
    }

    [Fact]
    public async Task Next_opens_existing_proposal_without_another_provider_request()
    {
        var service = Substitute.For<IFormCreatorAppService>();
        var wizard = new TestWizard(service);
        await wizard.NextAsync();
        wizard.CurrentStep.ShouldBe(1);
        wizard.Document.ShouldBe("The approved conversation proposal");
        await service.DidNotReceiveWithAnyArgs().ConverseAsync(default!);
        await service.DidNotReceiveWithAnyArgs().BuildAsync(default!);
    }

    [Fact]
    public async Task Step_headers_cannot_bypass_confirmation_or_leave_an_active_request()
    {
        var wizard = new TestWizard(Substitute.For<IFormCreatorAppService>());
        await wizard.HeaderAsync(2);
        wizard.CurrentStep.ShouldBe(0);
        await wizard.NextAsync();
        wizard.SetBusy(true);
        await wizard.HeaderAsync(0);
        wizard.CurrentStep.ShouldBe(1);
        wizard.SetBusy(false);
        await wizard.HeaderAsync(0);
        wizard.CurrentStep.ShouldBe(0);
    }

    private sealed class TestWizard : FormCreateWizard
    {
        public TestWizard(IFormCreatorAppService service, ICopilotCatalogAppService? catalog = null,
            IChatComposerCapabilitiesAppService? capabilities = null)
        {
            Creator = service;
            CopilotCatalog = catalog!;
            ComposerCapabilities = capabilities!;
            Proposal = "The approved conversation proposal";
            ProposalReady = true;
        }
        public int CurrentStep => Step;
        public Guid? SessionId => ComposerSessionId;
        public ChatComposerCapabilitiesDto? Capabilities => Messenger.ComposerCapabilities;
        public string Document => Proposal;
        public Task NextAsync() => ReviewAsync();
        public Task HeaderAsync(int step) => OnStepChangedAsync(step);
        public void SetBusy(bool busy) => Busy = busy;
        public CopilotCatalogItemDto? Settings => Copilot;
        public bool IsLoadingCatalog => LoadingCatalog;
        public IReadOnlyList<string> Prompts => ShortcutPrompts;
        public Guid? SelectedModel => ModelConfigurationId;
        public bool EmojiAvailable => Messenger.ComposerCapabilities?.CanUseRichComposer == true;
        public void SelectModel(Guid? id) => OnModelChanged(id);
        public Task OpenAsync(bool open) { Open = open; return OnParametersSetAsync(); }
    }
}
