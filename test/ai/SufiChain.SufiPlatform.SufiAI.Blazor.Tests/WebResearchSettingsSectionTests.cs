using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using SufiChain.SufiPlatform.SufiAI.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class WebResearchSettingsSectionTests
{
    [Fact]
    public async Task Web_research_section_stays_hidden_without_the_view_permission()
    {
        var groups = await LoadAsync(canView: false, canUpdate: false);
        groups.ShouldBeEmpty();
    }

    [Fact]
    public async Task Web_research_section_is_read_only_until_update_is_granted()
    {
        var groups = await LoadAsync(canView: true, canUpdate: false);
        var group = groups.ShouldHaveSingleItem();
        group.Id.ShouldBe(WebResearchSettingsSection.Id);
        group.Icon.ShouldBe("search");
        group.Order.ShouldBe(140);
        group.Policy.ShouldBe(AIPermissions.WebSearchSettings.Default);
        group.ComponentType.ShouldBe(typeof(WebResearchSettings));
        group.ReadOnly.ShouldBeTrue();
        group.DisplayName.ShouldBe("WebResearchSettings");
        group.Description.ShouldBe("WebResearchSetupHint");
    }

    [Fact]
    public async Task Web_research_section_can_be_saved_when_update_is_granted()
    {
        var groups = await LoadAsync(canView: true, canUpdate: true);
        groups.ShouldHaveSingleItem().ReadOnly.ShouldBeFalse();
    }

    [Fact]
    public void Web_research_labels_already_exist_in_four_cultures()
    {
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var texts = ReadTexts(FindRepoFile(
                "modules", "ai", "src", "SufiChain.SufiPlatform.SufiAI.Domain.Shared",
                "Localization", "AI", culture + ".json"));
            texts.ShouldContainKey("WebResearchSettings");
            texts.ShouldContainKey("WebResearchSetupHint");
            texts["WebResearchSettings"].ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void Old_web_research_route_redirects_into_the_settings_shell()
    {
        ReadRepoFile(
            "modules", "ai", "src", "SufiChain.SufiPlatform.SufiAI.Blazor",
            "Pages", "AI", "WebResearchSettings.razor").ShouldNotContain("@page");
        ReadRepoFile(
            "modules", "ai", "src", "SufiChain.SufiPlatform.SufiAI.Blazor",
            "Pages", "AI", "WebResearchSettingsRedirect.razor")
            .ShouldContain("@page \"/panel/admin/ai/web-research\"");
    }

    private static async Task<IReadOnlyList<SettingComponentGroup>> LoadAsync(bool canView, bool canUpdate)
    {
        var granted = new List<string>();
        if (canView)
        {
            granted.Add(AIPermissions.WebSearchSettings.Default);
        }

        if (canUpdate)
        {
            granted.Add(AIPermissions.WebSearchSettings.Update);
        }

        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationService>(new GrantList(granted));
        services.AddSingleton<IStringLocalizer<AIResource>, KeyLocalizer<AIResource>>();
        return await SettingsGroupCatalog.LoadAsync(
            [new WebResearchSettingsGroupContributor()],
            services.BuildServiceProvider());
    }

    private static Dictionary<string, string> ReadTexts(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!property.Name.Equals("texts", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var texts = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in property.Value.EnumerateObject())
            {
                texts[item.Name] = item.Value.GetString() ?? string.Empty;
            }

            return texts;
        }

        throw new InvalidOperationException("No texts object in " + path);
    }

    private static string ReadRepoFile(params string[] parts) => File.ReadAllText(FindRepoFile(parts));

    private static string FindRepoFile(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(new[] { dir.FullName }.Concat(parts).ToArray());
            if (File.Exists(path))
            {
                return path;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(string.Join('/', parts));
    }

    private sealed class GrantList : IAbpAuthorizationService
    {
        private readonly HashSet<string> _granted;

        public GrantList(IEnumerable<string> granted)
        {
            _granted = new HashSet<string>(granted, StringComparer.Ordinal);
        }

        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity());

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            return Task.FromResult(_granted.Contains(policyName)
                ? AuthorizationResult.Success()
                : AuthorizationResult.Failed());
        }
    }

    private sealed class KeyLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
