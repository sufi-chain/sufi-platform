using System.Collections.ObjectModel;
using System.ComponentModel;
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
using SufiChain.SufiPlatform.UI.Branding;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.PageToolbars;
using Volo.Abp.Authorization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

/// <summary>
/// A letter before @ is an email to Razor, so v@Definition.SchemaVersion renders as that source.
/// </summary>
public class FormSchemaVersionRenderTests : BunitContext
{
    private readonly IFormDefinitionAppService _forms = Substitute.For<IFormDefinitionAppService>();

    public FormSchemaVersionRenderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddSingleton<IStringLocalizerFactory, KeyLocalizerFactory>();
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new KeyLocalizer<SufiBlazorResource>());
        Services.AddSingleton<IPageLayout>(new PageLayoutStub());
        Services.AddSingleton(Substitute.For<IPageToolbarManager>());
        var branding = Substitute.For<IBrandingProvider>();
        branding.AppName.Returns("Sufi");
        Services.AddSingleton(branding);
        var authorization = new DenyAuthorizationService();
        Services.AddSingleton<IAuthorizationService>(authorization);
        Services.AddSingleton<IAbpAuthorizationService>(authorization);
        Services.AddSingleton(_forms);
        Renderer.SetRendererInfo(new RendererInfo("Server", isInteractive: true));
    }

    [Fact]
    public void Designer_caption_renders_the_schema_version_number()
    {
        var id = Guid.NewGuid();
        _forms.GetAsync(id).Returns(new FormDefinitionDto
        {
            Id = id,
            Key = "qa-form-key",
            Name = "QA",
            DisplayName = "QA form",
            SchemaVersion = 17,
            PublishState = FormPublishState.Draft,
            TagScope = string.Empty,
            IsActive = true
        });

        var cut = Render<FormDesignerPage>(parameters => parameters.Add(page => page.DefinitionId, id));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("v17"));
        cut.Markup.ShouldNotContain("v@Definition.SchemaVersion");
    }

    [Fact]
    public void Definitions_grid_renders_the_schema_version_number()
    {
        _forms.GetListAsync(Arg.Any<GetFormDefinitionsInput>()).Returns(
            new PagedResultDto<FormDefinitionListItemDto>(1,
            [
                new FormDefinitionListItemDto
                {
                    Id = Guid.NewGuid(),
                    Key = "qa-form-key",
                    Name = "QA",
                    DisplayName = "QA form",
                    SchemaVersion = 17,
                    PublishState = FormPublishState.Draft,
                    IsActive = true,
                    FieldCount = 0
                }
            ]));

        var cut = Render<FormDefinitionsPage>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("v17"));
        cut.Markup.ShouldNotContain("v@cellContext.Item.SchemaVersion");
    }

    private sealed class PageLayoutStub : IPageLayout
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; } = new();
        public ObservableCollection<PageToolbarItem> ToolbarItems { get; } = new();
        public object? ToolbarContent { get; set; }
        public void Reset() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
    }

    private sealed class DenyAuthorizationService : IAbpAuthorizationService
    {
        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity());
        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
            Task.FromResult(AuthorizationResult.Failed());
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
        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments), resourceNotFound: false);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class KeyLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name, resourceNotFound: false);
        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments), resourceNotFound: false);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
