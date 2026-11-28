using System.Collections.ObjectModel;
using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.FileManager.Blazor.Services;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiForms.Blazor.Pages;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
using SufiChain.SufiPlatform.SufiForms.Hooshvare;
using SufiChain.SufiPlatform.SufiForms.Jobs;
using SufiChain.SufiPlatform.SufiForms.Permissions;
using SufiChain.SufiPlatform.SufiForms.Records;
using SufiChain.SufiPlatform.SufiForms.Records.Dtos;
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
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

/// <summary>FRM-06. A form with no records explains itself instead of the generic grid empty text.</summary>
public class FormRecordsZeroStateTests : BunitContext
{
    private static readonly Guid DefinitionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly IFormDefinitionAppService _forms = Substitute.For<IFormDefinitionAppService>();
    private readonly IFormRecordAppService _records = Substitute.For<IFormRecordAppService>();

    public FormRecordsZeroStateTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddSingleton<IStringLocalizerFactory, KeyLocalizerFactory>();
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new KeyLocalizer<SufiBlazorResource>());
        Services.AddSingleton<ICurrentTenant>(new HostTenant());
        Services.AddSingleton<ICurrentUserAccessor>(Substitute.For<ICurrentUserAccessor>());
        Services.AddSingleton(Substitute.For<IUiMessageService>());
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
        Services.AddSingleton(_records);
        Services.AddSingleton(Substitute.For<IFormDataJobAppService>());
        Services.AddSingleton(Substitute.For<IFormRecordHooshvareAppService>());
        Services.AddSingleton(Substitute.For<IChatComposerCapabilitiesAppService>());
        Services.AddSingleton(Substitute.For<IHooshvareCatalogAppService>());
        Services.AddSingleton(Substitute.For<IFileItemAppService>());
        Services.AddSingleton(Substitute.For<IFileStructureAppService>());
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        Services.AddSingleton(Substitute.For<IFileUploadAccessTokenProvider>());
        Services.AddSingleton<FileManagerJsInterop, DisposableFileManagerJsInterop>();
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>())
            .Returns(new PagedResultDto<FormDefinitionListItemDto>(1, [ListItem()]));
        this.AddAuthorization().SetAuthorized("admin");
        Services.AddSingleton<IAuthorizationService>(new GrantingAuthorizationService(
            SufiFormsPermissions.Records.Default,
            SufiFormsPermissions.Records.Create,
            SufiFormsPermissions.Forms.Default));
    }

    [Fact]
    public async Task RecordsPage_ZeroRecords_FormWithFields_ShowsAddCta()
    {
        _forms.GetByKeyAsync("intake").Returns(Detail(withField: true));
        _records.QueryAsync("intake", Arg.Any<FormQueryInput>())
            .Returns(new PagedResultDto<FormRecordDto>(0, []));

        var cut = Render<DynamicFormRecordsPage>(parameters => parameters.Add(page => page.FormKey, "intake"));

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldContain("Forms.NoRecordsTitle");
            cut.Markup.ShouldContain("Forms.CreateRecord");
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordsPage_ZeroRecords_NoFields_ShowsDesignerHint()
    {
        _forms.GetByKeyAsync("intake").Returns(Detail(withField: false));
        _records.QueryAsync("intake", Arg.Any<FormQueryInput>())
            .Returns(new PagedResultDto<FormRecordDto>(0, []));

        var cut = Render<DynamicFormRecordsPage>(parameters => parameters.Add(page => page.FormKey, "intake"));

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldContain("Forms.NoFieldsOnFormTitle");
            cut.Markup.ShouldContain($"/panel/admin/forms/designer/{DefinitionId}");
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordsPage_WithRecords_ShowsGrid()
    {
        _forms.GetByKeyAsync("intake").Returns(Detail(withField: true));
        _records.QueryAsync("intake", Arg.Any<FormQueryInput>())
            .Returns(new PagedResultDto<FormRecordDto>(1,
            [
                new FormRecordDto
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Values = new Dictionary<string, object?> { ["topic"] = "alpha-row" }
                }
            ]));

        var cut = Render<DynamicFormRecordsPage>(parameters => parameters.Add(page => page.FormKey, "intake"));

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldContain("alpha-row");
            cut.Markup.ShouldNotContain("Forms.NoRecordsTitle");
            cut.Markup.ShouldNotContain("Forms.NoFieldsOnFormTitle");
        }, TimeSpan.FromSeconds(5));
    }

    private static FormDefinitionListItemDto ListItem() => new()
    {
        Id = DefinitionId,
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        PublishState = FormPublishState.Published,
        IsActive = true
    };

    private static FormDefinitionDto Detail(bool withField) => new()
    {
        Id = DefinitionId,
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        PublishState = FormPublishState.Published,
        IsActive = true,
        Fields = withField
            ?
            [
                new FormFieldDto
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Name = "topic",
                    DisplayName = "Topic",
                    FieldType = FormFieldType.Text
                }
            ]
            : []
    };

    private sealed class DisposableFileManagerJsInterop : FileManagerJsInterop, IDisposable
    {
        public DisposableFileManagerJsInterop(IJSRuntime jsRuntime, IConfiguration configuration)
            : base(jsRuntime, configuration)
        {
        }

        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
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
        public GrantingAuthorizationService(params string[] granted) => _granted = new HashSet<string>(granted, StringComparer.Ordinal);
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
        public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: false);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class KeyLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);
        public LocalizedString this[string name, params object[] arguments] => new(name, name, resourceNotFound: false);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class HostTenant : ICurrentTenant
    {
        public bool IsAvailable => false;
        public Guid? Id => null;
        public string? Name => null;
        public IDisposable Change(Guid? id, string? name = null) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }
}
