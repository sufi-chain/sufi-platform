using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Blazor.Menus;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.UI.Features;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Services.Navigation;
using SufiChain.SufiPlatform.UI.Users;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class McpAdminMenuTests
{
    [Fact]
    public async Task Tenant_admin_does_not_see_mcp_servers_or_tools()
    {
        var menu = await BuildMenuAsync(tenantId: Guid.NewGuid());

        Find(menu, AIMenus.WorkspaceChat).ShouldNotBeNull();
        Find(menu, AIMenus.MCPGroup).ShouldBeNull();
        Find(menu, AIMenus.MCPServers).ShouldBeNull();
        Find(menu, AIMenus.MCPTools).ShouldBeNull();
    }

    [Fact]
    public async Task Host_admin_sees_mcp_servers_and_tools()
    {
        var menu = await BuildMenuAsync(tenantId: null);

        Find(menu, AIMenus.WorkspaceChat).ShouldNotBeNull();
        Find(menu, AIMenus.MCPServers).ShouldNotBeNull();
        Find(menu, AIMenus.MCPTools).ShouldNotBeNull();
    }

    private static async Task<ApplicationMenu> BuildMenuAsync(Guid? tenantId)
    {
        var options = new SufiNavigationOptions();
        options.MenuContributors.Add(new AIMenuContributor());
        var features = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            [SufiAIFeatures.Enable] = true,
            [SufiAIFeatures.Chat] = true,
            [SufiAIFeatures.MCP] = true
        };

        var services = new ServiceCollection();
        services.AddSingleton<IStringLocalizerFactory, EchoLocalizerFactory>();
        services.AddSingleton<IFeatureChecker>(new MapFeatureChecker(features));
        services.AddSingleton<ICurrentTenant>(new FixedTenant(tenantId));

        var manager = new DefaultMenuManager(
            services.BuildServiceProvider(),
            Options.Create(options),
            new GrantAllPermissionChecker(),
            new MapSufiFeatureChecker(features),
            new AuthenticatedUser(tenantId));

        return await manager.GetMainMenuAsync();
    }

    private static ApplicationMenuItem? Find(ApplicationMenu menu, string name)
    {
        return Find(menu.Items, name);
    }

    private static ApplicationMenuItem? Find(IEnumerable<ApplicationMenuItem> items, string name)
    {
        foreach (var item in items)
        {
            if (item.Name == name)
            {
                return item;
            }

            var child = Find(item.Items, name);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private sealed class FixedTenant : ICurrentTenant
    {
        public FixedTenant(Guid? id) => Id = id;

        public bool IsAvailable => Id.HasValue;

        public Guid? Id { get; }

        public string? Name => null;

        public IDisposable Change(Guid? id, string? name = null) => throw new NotSupportedException();
    }

    private sealed class MapFeatureChecker : IFeatureChecker
    {
        private readonly IReadOnlyDictionary<string, bool> _features;

        public MapFeatureChecker(IReadOnlyDictionary<string, bool> features) => _features = features;

        public Task<string> GetOrNullAsync(string name) =>
            Task.FromResult(_features.TryGetValue(name, out var enabled) && enabled ? "true" : "false");

        public Task<bool> IsEnabledAsync(string name) =>
            Task.FromResult(_features.TryGetValue(name, out var enabled) && enabled);
    }

    private sealed class MapSufiFeatureChecker : ISufiFeatureChecker
    {
        private readonly IReadOnlyDictionary<string, bool> _features;

        public MapSufiFeatureChecker(IReadOnlyDictionary<string, bool> features) => _features = features;

        public Task<bool> IsEnabledAsync(string featureName) =>
            Task.FromResult(_features.TryGetValue(featureName, out var enabled) && enabled);

        public Task<Dictionary<string, bool>> IsEnabledAsync(IEnumerable<string> featureNames) =>
            Task.FromResult(featureNames.Distinct(StringComparer.Ordinal)
                .ToDictionary(name => name, name => _features.TryGetValue(name, out var enabled) && enabled));
    }

    private sealed class GrantAllPermissionChecker : SufiChain.SufiPlatform.UI.Authorization.ISufiPermissionChecker
    {
        public Task<bool> IsGrantedAsync(string permissionName) => Task.FromResult(true);

        public Task<Dictionary<string, bool>> IsGrantedAsync(IEnumerable<string> permissionNames) =>
            Task.FromResult(permissionNames.Distinct(StringComparer.Ordinal).ToDictionary(name => name, _ => true));
    }

    private sealed class AuthenticatedUser : ICurrentUserAccessor
    {
        public AuthenticatedUser(Guid? tenantId) => TenantId = tenantId;

        public bool IsAuthenticated => true;

        public Guid? Id => Guid.NewGuid();

        public string? UserName => "admin";

        public string? Name => "Admin";

        public string? SurName => null;

        public string? PhoneNumber => null;

        public bool PhoneNumberVerified => false;

        public string? Email => "admin@sufichain.ir";

        public bool EmailVerified => true;

        public Guid? TenantId { get; }

        public string[] Roles => ["admin"];

        public System.Security.Claims.Claim? FindClaim(string claimType) => null;

        public System.Security.Claims.Claim[] FindClaims(string claimType) => [];

        public System.Security.Claims.Claim[] GetAllClaims() => [];

        public bool IsInRole(string roleName) => roleName == "admin";
    }

    private sealed class EchoLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new EchoLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new EchoLocalizer();
    }

    private sealed class EchoLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
