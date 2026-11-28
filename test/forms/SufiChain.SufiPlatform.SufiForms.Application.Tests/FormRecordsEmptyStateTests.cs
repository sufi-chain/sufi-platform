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

public class FormRecordsEmptyStateTests : BunitContext
{
    private readonly IFormDefinitionAppService _forms = Substitute.For<IFormDefinitionAppService>();

    public FormRecordsEmptyStateTests()
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
        Services.AddSingleton(Substitute.For<IFormRecordAppService>());
        Services.AddSingleton(Substitute.For<IFormDataJobAppService>());
        Services.AddSingleton(Substitute.For<IFormRecordHooshvareAppService>());
        Services.AddSingleton(Substitute.For<IChatComposerCapabilitiesAppService>());
        Services.AddSingleton(Substitute.For<IHooshvareCatalogAppService>());
        Services.AddSingleton(Substitute.For<IFileItemAppService>());
        Services.AddSingleton(Substitute.For<IFileStructureAppService>());
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        Services.AddSingleton(Substitute.For<IFileUploadAccessTokenProvider>());
        Services.AddSingleton<FileManagerJsInterop, DisposableFileManagerJsInterop>();
        this.AddAuthorization().SetAuthorized("admin");
        Services.AddSingleton<IAuthorizationService>(new GrantingAuthorizationService(
            SufiFormsPermissions.Records.Default,
            SufiFormsPermissions.Forms.Default));
    }

    [Fact]
    public async Task Empty_state_stays_hidden_until_the_forms_list_finishes()
    {
        var gate = new TaskCompletionSource<PagedResultDto<FormDefinitionListItemDto>>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>()).Returns(_ => gate.Task);

        var cut = Render<DynamicFormRecordsPage>();
        var timeout = TimeSpan.FromSeconds(5);

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldNotContain("Forms.NoFormsTitle");
            cut.Markup.ShouldContain("sb-skeleton");
        }, timeout);

        gate.SetResult(new PagedResultDto<FormDefinitionListItemDto>(0, []));

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldContain("Forms.NoFormsTitle");
            cut.Markup.ShouldNotContain("sb-skeleton");
        }, timeout);
    }

    [Fact]
    public async Task Loaded_forms_show_the_picker_instead_of_the_empty_state()
    {
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>())
            .Returns(new PagedResultDto<FormDefinitionListItemDto>(1, [ListItem()]));

        var cut = Render<DynamicFormRecordsPage>();

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldNotContain("Forms.NoFormsTitle");
            cut.Markup.ShouldContain("intake");
        }, TimeSpan.FromSeconds(5));
    }

    private static FormDefinitionListItemDto ListItem() => new()
    {
        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        PublishState = FormPublishState.Published,
        IsActive = true
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
