using System.Collections.ObjectModel;
using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiForms.Blazor.Pages;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
using SufiChain.SufiPlatform.SufiForms.Permissions;
using SufiChain.SufiPlatform.SufiForms.Records;
using SufiChain.SufiPlatform.UI.Alerts;
using SufiChain.SufiPlatform.UI.BlockUi;
using SufiChain.SufiPlatform.UI.Branding;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.Messages;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.UI.Notifications;
using SufiChain.SufiPlatform.UI.PageToolbars;
using SufiChain.SufiPlatform.UI.Progression;
using SufiChain.SufiPlatform.UI.Users;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormDefinitionUnpublishRefreshTests : BunitContext
{
    private static readonly Guid DefinitionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly IFormDefinitionAppService _forms = Substitute.For<IFormDefinitionAppService>();
    private readonly IUiMessageService _messages = Substitute.For<IUiMessageService>();

    public FormDefinitionUnpublishRefreshTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddSingleton<IStringLocalizerFactory, KeyLocalizerFactory>();
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new KeyLocalizer<SufiBlazorResource>());
        Services.AddSingleton<ICurrentTenant>(new HostTenant());
        Services.AddSingleton<ICurrentUserAccessor>(Substitute.For<ICurrentUserAccessor>());
        Services.AddSingleton(_messages);
        Services.AddSingleton(Substitute.For<IUiNotificationService>());
        Services.AddSingleton(Substitute.For<IAlertManager>());
        Services.AddSingleton(Substitute.For<IBlockUiService>());
        Services.AddSingleton(Substitute.For<IUiPageProgressService>());
        Services.AddSingleton(Substitute.For<IUserExceptionInformer>());
        Services.AddSingleton(Substitute.For<IPageToolbarManager>());
        Services.AddSingleton<IPageLayout>(new PageLayoutStub());
        var branding = Substitute.For<IBrandingProvider>();
        branding.AppName.Returns("Sufi");
        Services.AddSingleton(branding);
        Services.AddSingleton(_forms);
        _messages.SuccessAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Action<UiMessageOptions>?>())
            .Returns(Task.CompletedTask);
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>())
            .Returns(new PagedResultDto<FormDefinitionListItemDto>(1, [PublishedRow()]));
        this.AddAuthorization().SetAuthorized("admin");
        Services.AddSingleton<IAuthorizationService>(new GrantingAuthorizationService(
            SufiFormsPermissions.Forms.Default,
            SufiFormsPermissions.Forms.Publish));
    }

    [Fact]
    public void Unpublish_then_stale_list_still_shows_draft_and_publish()
    {
        _forms.UnpublishAsync(DefinitionId).Returns(Detail(FormPublishState.Draft));

        var cut = Render<FormDefinitionsPage>();
        cut.Find("button[aria-label='Forms.Unpublish']").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Forms.PublishState.Draft");
            cut.Find("button[aria-label='Forms.Publish']");
            cut.Markup.ShouldNotContain("Forms.PublishState.Published");
        });
    }

    [Fact]
    public void Publish_then_stale_list_still_shows_published_and_unpublish()
    {
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>())
            .Returns(new PagedResultDto<FormDefinitionListItemDto>(1, [Row(FormPublishState.Draft)]));
        _forms.PublishAsync(DefinitionId).Returns(Detail(FormPublishState.Published));

        var cut = Render<FormDefinitionsPage>();
        cut.Find("button[aria-label='Forms.Publish']").Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Forms.PublishState.Published");
            cut.Find("button[aria-label='Forms.Unpublish']");
        });
    }

    [Fact]
    public void Archive_then_stale_list_still_shows_archived()
    {
        _forms.ArchiveAsync(DefinitionId).Returns(Detail(FormPublishState.Archived));

        var cut = Render<FormDefinitionsPage>();
        cut.Find("button[aria-label='Forms.Archive']").Click();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Forms.PublishState.Archived"));
    }

    [Fact]
    public async Task Unpublish_and_archive_invalidate_the_schema_cache()
    {
        var definition = new FormDefinition(DefinitionId, null, "intake", "Intake", "Intake");
        definition.AddField(new FormField(Guid.NewGuid(), "title", FormFieldType.Text, "Title"));
        var manager = new FormDefinitionManager(Substitute.For<IFormDefinitionRepository>());
        await manager.PublishAsync(definition);
        var repository = Substitute.For<IFormDefinitionRepository>();
        repository.GetAsync(DefinitionId, true, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(definition);
        repository.UpdateAsync(definition, true, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(definition);
        var schema = Substitute.For<FormSchemaCache>(
            Substitute.For<IDistributedCache<FormDefinitionCacheItem, string>>(),
            repository);
        schema.InvalidateAsync(Arg.Any<string>()).Returns(Task.CompletedTask);
        var service = new MappingFormDefinitionAppService(
            repository,
            manager,
            schema,
            Substitute.For<IDistributedEventBus>());

        var unpublished = await service.UnpublishAsync(DefinitionId);
        unpublished.PublishState.ShouldBe(FormPublishState.Draft);
        await schema.Received(1).InvalidateAsync("intake");

        await manager.PublishAsync(definition);
        definition.PublishState.ShouldBe(FormPublishState.Published);
        var archived = await service.ArchiveAsync(DefinitionId);
        archived.PublishState.ShouldBe(FormPublishState.Archived);
        await schema.Received(2).InvalidateAsync("intake");
    }

    private static FormDefinitionListItemDto PublishedRow() => Row(FormPublishState.Published);

    private static FormDefinitionListItemDto Row(FormPublishState state) => new()
    {
        Id = DefinitionId,
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        SchemaVersion = 1,
        PublishState = state,
        IsActive = true,
        FieldCount = 1
    };

    private static FormDefinitionDto Detail(FormPublishState state) => new()
    {
        Id = DefinitionId,
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        SchemaVersion = 1,
        PublishState = state,
        IsActive = true,
        Fields = []
    };

    private sealed class MappingFormDefinitionAppService : FormDefinitionAppService
    {
        public MappingFormDefinitionAppService(
            IFormDefinitionRepository repository,
            FormDefinitionManager manager,
            FormSchemaCache schemaCache,
            IDistributedEventBus eventBus)
            : base(repository, manager, schemaCache, eventBus)
        {
        }

        protected override FormDefinitionDto MapToDto(FormDefinition entity) => new()
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            SchemaVersion = entity.SchemaVersion,
            PublishState = entity.PublishState,
            IsActive = entity.IsActive
        };
    }

    private sealed class PageLayoutStub : IPageLayout
    {
        public string? Title { get; set; } = "";

        public string? Subtitle { get; set; }

        public ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; } = [];

        public ObservableCollection<PageToolbarItem> ToolbarItems { get; } = [];

        public object? ToolbarContent { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public void Reset() => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Title)));
    }

    private sealed class GrantingAuthorizationService : IAbpAuthorizationService
    {
        private readonly HashSet<string> _granted;

        public GrantingAuthorizationService(params string[] granted) =>
            _granted = new HashSet<string>(granted, StringComparer.Ordinal);

        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity());

        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
            Task.FromResult(_granted.Contains(policyName) ? AuthorizationResult.Success() : AuthorizationResult.Failed());
    }

    private sealed class KeyLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => KeyLocalizer.Instance;

        public IStringLocalizer Create(string baseName, string location) => KeyLocalizer.Instance;
    }

    private sealed class KeyLocalizer : IStringLocalizer
    {
        public static readonly KeyLocalizer Instance = new();

        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(System.Globalization.CultureInfo.InvariantCulture, name, arguments), resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class KeyLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(System.Globalization.CultureInfo.InvariantCulture, name, arguments), resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class HostTenant : ICurrentTenant
    {
        public bool IsAvailable => false;

        public Guid? Id => null;

        public string? Name => null;

        public IDisposable Change(Guid? id, string? name = null) => new Noop();

        private sealed class Noop : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
