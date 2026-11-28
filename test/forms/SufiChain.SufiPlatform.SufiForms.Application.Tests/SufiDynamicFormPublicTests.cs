using System.Collections.ObjectModel;
using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.SufiForms.Blazor.Public.Components;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
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

public class SufiDynamicFormPublicTests : BunitContext
{
    private readonly IPublicFormAppService _forms = Substitute.For<IPublicFormAppService>();

    public SufiDynamicFormPublicTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddSingleton<IStringLocalizerFactory, KeyLocalizerFactory>();
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new KeyLocalizer<SufiBlazorResource>());
        Services.AddSingleton<ICurrentTenant>(Substitute.For<ICurrentTenant>());
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
        this.AddAuthorization().SetAuthorized("admin");
        Services.AddSingleton<IAuthorizationService>(new GrantingAuthorizationService());
    }

    [Fact]
    public void SufiDynamicForm_Anonymous_PublishedForm_RendersFields()
    {
        _forms.GetPublishedByKeyAsync("intake").Returns(Definition());

        var cut = Render<SufiDynamicForm>(parameters => parameters.Add(form => form.FormKey, "intake"));

        cut.Markup.ShouldContain("Message");
        cut.Markup.ShouldNotContain("Forms.FormNotFound");
    }

    [Fact]
    public void SufiDynamicForm_Anonymous_Draft_ShowsNotFound()
    {
        _forms.GetPublishedByKeyAsync("intake").Returns((FormDefinitionDto?)null);

        var cut = Render<SufiDynamicForm>(parameters => parameters.Add(form => form.FormKey, "intake"));

        cut.Markup.ShouldContain("Forms.FormNotFound");
    }

    private static FormDefinitionDto Definition() => new()
    {
        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        Key = "intake",
        Name = "Intake",
        DisplayName = "Intake",
        PublishState = FormPublishState.Published,
        IsActive = true,
        TagScope = "sufiforms",
        Fields =
        [
            new FormFieldDto
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "message",
                FieldType = FormFieldType.Text,
                DisplayName = "Message"
            }
        ]
    };

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
        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity());
        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Success());
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName) =>
            Task.FromResult(AuthorizationResult.Success());
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
}
