using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SettingsPanelLoadingTests : BunitContext
{
    public SettingsPanelLoadingTests()
    {
        Services.AddSingleton<IStringLocalizerFactory>(new PassthroughLocalizerFactory());
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new PassthroughLocalizer());
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns((Guid?)null);
        Services.AddSingleton(tenant);
    }

    [Fact]
    public async Task Email_panel_keeps_the_placeholder_until_the_saved_port_arrives()
    {
        var gate = new TaskCompletionSource<EmailSettingsDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var email = Substitute.For<IEmailSettingsAppService>();
        email.GetAsync().Returns(_ => gate.Task);
        Services.AddSingleton(email);

        var cut = Render<EmailSettingsGroup>();
        var timeout = TimeSpan.FromSeconds(5);

        // The synchronous wait posts its check onto the renderer dispatcher and then blocks.
        // On a busy CI runner that check never runs (count 0) before the one-second timer fires.
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldContain("sb-loading-skeleton");
            cut.FindAll("input[type=number]").Count.ShouldBe(0);
        }, timeout);

        gate.SetResult(new EmailSettingsDto
        {
            SmtpHost = "smtp.example",
            SmtpPort = 587
        });

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.ShouldNotContain("sb-loading-skeleton");
            cut.Find("input[type=number]").GetAttribute("value").ShouldBe("587");
        }, timeout);

        var saved = await cut.InvokeAsync(() => cut.Instance.TrySaveAsync());
        saved.ShouldBeTrue();
        await email.Received(1).UpdateAsync(Arg.Is<UpdateEmailSettingsDto>(dto =>
            dto.SmtpHost == "smtp.example" &&
            dto.SmtpPort == 587 &&
            dto.SmtpPassword == null));
    }

    [Fact]
    public async Task Tenant_email_section_shows_the_host_lock_without_calling_the_endpoint()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.IsAvailable.Returns(true);
        tenant.Id.Returns(Guid.NewGuid());
        Services.AddSingleton(tenant);
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(SettingsFeatures.AllowChangingEmailSettings).Returns(false);
        Services.AddSingleton(features);
        var email = Substitute.For<IEmailSettingsAppService>();
        Services.AddSingleton(email);

        var cut = Render<EmailSettingsGroup>();

        await cut.WaitForAssertionAsync(() =>
        {
            cut.Find("[data-testid='email-settings-locked']").TextContent
                .ShouldContain("EmailSettingsLockedByHost");
        }, TimeSpan.FromSeconds(5));
        cut.FindAll("input").Count.ShouldBe(0);
        await email.DidNotReceive().GetAsync();
    }

    [Fact]
    public void Every_settings_group_marks_its_load_before_the_first_render()
    {
        var groups = new[]
        {
            "sufi-platform/modules/settings/src/SufiChain.SufiPlatform.Settings.Blazor/Settings/EmailSettingsGroup.razor.cs",
            "sufi-platform/modules/settings/src/SufiChain.SufiPlatform.Settings.Blazor/Settings/ExternalAuthSettingsGroup.razor.cs",
            "sufi-platform/modules/settings/src/SufiChain.SufiPlatform.Settings.Blazor/Settings/IdentitySettingsGroup.razor.cs",
            "sufi-platform/modules/settings/src/SufiChain.SufiPlatform.Settings.Blazor/Settings/TimeZoneSettingsGroup.razor.cs",
            "sufi-platform/modules/short-links/src/SufiChain.SufiPlatform.ShortLinks.Blazor/Settings/ShortLinksSettingsGroup.razor.cs",
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Settings/FileManagerArchivingSettingsGroup.razor.cs",
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Settings/FileManagerGeneralSettingsGroup.razor.cs",
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Settings/FileManagerStorageSettingsGroup.razor.cs",
            "pro-modules/branding/src/SufiChain.SufiPlatform.Branding.Blazor/Settings/TenantBrandingSettingsGroup.razor.cs"
        };

        foreach (var group in groups)
        {
            var text = File.ReadAllText(FindWorkspaceFile(group));
            text.ShouldContain("LoadingStates[LoadingKeys.Load] = true");
        }
    }

    private static string FindWorkspaceFile(string relative)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relative);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(relative);
    }

    private sealed class PassthroughLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new PassthroughLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new PassthroughLocalizer();
    }

    private sealed class PassthroughLocalizer : IStringLocalizer, IStringLocalizer<SufiBlazorResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
