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
    public void Email_panel_keeps_the_placeholder_until_the_saved_port_arrives()
    {
        var gate = new TaskCompletionSource<EmailSettingsDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var email = Substitute.For<IEmailSettingsAppService>();
        email.GetAsync().Returns(_ => gate.Task);
        Services.AddSingleton(email);

        var cut = Render<EmailSettingsGroup>();

        cut.Markup.ShouldContain("sb-loading-skeleton");
        cut.FindAll("input[type=number]").Count.ShouldBe(0);

        gate.SetResult(new EmailSettingsDto
        {
            SmtpHost = "smtp.example",
            SmtpPort = 587
        });

        cut.WaitForAssertion(() =>
        {
            cut.Find("input[type=number]").GetAttribute("value").ShouldBe("587");
        });
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
