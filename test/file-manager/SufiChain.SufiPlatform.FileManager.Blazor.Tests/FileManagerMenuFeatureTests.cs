using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.Blazor.Menus;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.UI.Authorization;
using SufiChain.SufiPlatform.UI.Features;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Services.Navigation;
using SufiChain.SufiPlatform.UI.Users;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class FileManagerMenuFeatureTests
{
    private static readonly string[] ChildNames =
    {
        FileManagerMenus.FileStats,
        FileManagerMenus.AssetManager,
        FileManagerMenus.FileStructures,
        FileManagerMenus.ArchivedFiles,
        FileManagerMenus.FolderAccess
    };

    [Fact]
    public async Task File_manager_menu_is_absent_when_its_features_are_off()
    {
        var menu = await BuildMenuAsync(enabled: false);

        Find(menu, FileManagerMenus.GroupName).ShouldBeNull();
        foreach (var child in ChildNames)
        {
            Find(menu, child).ShouldBeNull();
        }
    }

    [Fact]
    public async Task File_manager_menu_is_absent_when_every_boolean_feature_is_off()
    {
        var menu = await BuildMenuAsync(new Dictionary<string, bool>
        {
            [SufiFileManagerFeatures.Enable] = false,
            [SufiFileManagerFeatures.FileItems] = false,
            [SufiFileManagerFeatures.FileStructures] = false,
            [SufiFileManagerFeatures.StorageSettings] = false,
            [SufiFileManagerFeatures.Archiving] = false
        });

        Find(menu, FileManagerMenus.GroupName).ShouldBeNull();
        foreach (var child in ChildNames)
        {
            Find(menu, child).ShouldBeNull();
        }
    }

    [Fact]
    public async Task File_manager_menu_and_children_are_present_when_features_are_on()
    {
        var menu = await BuildMenuAsync(enabled: true);

        Find(menu, FileManagerMenus.GroupName).ShouldNotBeNull();
        foreach (var child in ChildNames)
        {
            Find(menu, child).ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task Child_items_follow_their_own_feature()
    {
        var menu = await BuildMenuAsync(new Dictionary<string, bool>
        {
            [SufiFileManagerFeatures.Enable] = true,
            [SufiFileManagerFeatures.FileItems] = true,
            [SufiFileManagerFeatures.FileStructures] = false,
            [SufiFileManagerFeatures.Archiving] = false
        });

        Find(menu, FileManagerMenus.FileStats).ShouldNotBeNull();
        Find(menu, FileManagerMenus.AssetManager).ShouldNotBeNull();
        Find(menu, FileManagerMenus.FolderAccess).ShouldNotBeNull();
        Find(menu, FileManagerMenus.FileStructures).ShouldBeNull();
        Find(menu, FileManagerMenus.ArchivedFiles).ShouldBeNull();
    }

    private static Task<ApplicationMenu> BuildMenuAsync(bool enabled)
    {
        return BuildMenuAsync(new Dictionary<string, bool>
        {
            [SufiFileManagerFeatures.Enable] = enabled,
            [SufiFileManagerFeatures.FileItems] = enabled,
            [SufiFileManagerFeatures.FileStructures] = enabled,
            [SufiFileManagerFeatures.StorageSettings] = enabled,
            [SufiFileManagerFeatures.Archiving] = enabled
        });
    }

    private static async Task<ApplicationMenu> BuildMenuAsync(Dictionary<string, bool> features)
    {
        var options = new SufiNavigationOptions();
        options.MenuContributors.Add(new FileManagerMenuContributor());

        var services = new ServiceCollection();
        services.AddSingleton<IStringLocalizerFactory, EchoLocalizerFactory>();

        var permissions = new GrantAllPermissionChecker();
        var manager = new DefaultMenuManager(
            services.BuildServiceProvider(),
            Options.Create(options),
            permissions,
            new MapFeatureChecker(features),
            new AuthenticatedUser());

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

    private sealed class MapFeatureChecker : ISufiFeatureChecker
    {
        private readonly IReadOnlyDictionary<string, bool> _features;

        public MapFeatureChecker(IReadOnlyDictionary<string, bool> features)
        {
            _features = features;
        }

        public Task<bool> IsEnabledAsync(string featureName)
        {
            return Task.FromResult(_features.TryGetValue(featureName, out var enabled) && enabled);
        }

        public Task<Dictionary<string, bool>> IsEnabledAsync(IEnumerable<string> featureNames)
        {
            var result = featureNames
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(name => name, name => _features.TryGetValue(name, out var enabled) && enabled);
            return Task.FromResult(result);
        }
    }

    private sealed class GrantAllPermissionChecker : ISufiPermissionChecker
    {
        public Task<bool> IsGrantedAsync(string permissionName)
        {
            return Task.FromResult(true);
        }

        public Task<Dictionary<string, bool>> IsGrantedAsync(IEnumerable<string> permissionNames)
        {
            return Task.FromResult(permissionNames.Distinct(StringComparer.Ordinal).ToDictionary(name => name, _ => true));
        }
    }

    private sealed class AuthenticatedUser : ICurrentUserAccessor
    {
        public bool IsAuthenticated => true;

        public Guid? Id => Guid.NewGuid();

        public string? UserName => "admin";

        public string? Name => "Admin";

        public string? SurName => "User";

        public string? PhoneNumber => null;

        public bool PhoneNumberVerified => false;

        public string? Email => "admin@sufichain.ir";

        public bool EmailVerified => true;

        public Guid? TenantId => null;

        public string[] Roles => [];

        public Claim? FindClaim(string claimType) => null;

        public Claim[] FindClaims(string claimType) => [];

        public Claim[] GetAllClaims() => [];

        public bool IsInRole(string roleName) => false;
    }

    private sealed class EchoLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource)
        {
            return new EchoLocalizer();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return new EchoLocalizer();
        }
    }

    private sealed class EchoLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return [];
        }
    }
}
