using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Authorization.Permissions;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class PublicMenuSignedInTests
{
    [Fact]
    public async Task SignedIn_user_with_permission_sees_gated_public_item()
    {
        var names = await new SignedInMenus(granted: true).GetTreeAsync("Public", null, "Default");
        names.Select(item => item.DisplayName).ShouldBe(["Home", "Users"]);
    }

    [Fact]
    public async Task SignedIn_user_without_permission_does_not()
    {
        var names = await new SignedInMenus(granted: false).GetTreeAsync("Public", null, "Default");
        names.Select(item => item.DisplayName).ShouldBe(["Home"]);
    }

    [Fact]
    public async Task Anonymous_never_sees_gated_item()
    {
        var menus = new SignedInMenus(granted: true) { SignedIn = false };
        var names = await menus.GetTreeAsync("Public", null, "Default");
        names.Select(item => item.DisplayName).ShouldBe(["Home"]);
        (await menus.FindItemBySlugAsync("Public", null, "Default", "users")).ShouldBeNull();
    }

    [Fact]
    public async Task Public_tree_cache_does_not_leak_gated_items_across_users()
    {
        var key = MenuTreeCacheKeys.CreatePublicTreeCacheKey("Public", null, "Default");
        key.ShouldNotContain("user");

        var withPermission = await new SignedInMenus(granted: true).GetTreeAsync("Public", null, "Default");
        var withoutPermission = await new SignedInMenus(granted: false).GetTreeAsync("Public", null, "Default");
        withPermission.Select(item => item.DisplayName).ShouldBe(["Home", "Users"]);
        withoutPermission.Select(item => item.DisplayName).ShouldBe(["Home"]);

        var gated = new MenuItem(Guid.NewGuid(), Guid.NewGuid(), "users", "Users", "users");
        gated.SetPermissionName("SufiIdentity.Users");
        PublicMenuAppService.IncludeInSharedPublicCache(gated).ShouldBeTrue();
    }

    private sealed class SignedInMenus : PublicMenuAppService
    {
        public bool SignedIn { get; set; } = true;

        public SignedInMenus(bool granted)
            : base(
                Substitute.For<IMenuRepository>(),
                null!,
                null!,
                null!,
                null!,
                permissionChecker: Checker(granted))
        {
        }

        protected override bool IsCallerAuthenticated() => SignedIn;

        protected override Task<List<MenuItemTreeDto>> LoadTreeAsync(string contextType, Guid? contextId, string menuName)
        {
            return Task.FromResult(new List<MenuItemTreeDto>
            {
                new() { DisplayName = "Home" },
                new() { DisplayName = "Users", PermissionName = "SufiIdentity.Users" }
            });
        }

        protected override Task<MenuItemDto?> LoadItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug)
        {
            return Task.FromResult<MenuItemDto?>(new MenuItemDto
            {
                DisplayName = "Users",
                PermissionName = "SufiIdentity.Users",
                Slug = slug
            });
        }

        private static IPermissionChecker Checker(bool granted)
        {
            var checker = Substitute.For<IPermissionChecker>();
            checker.IsGrantedAsync(Arg.Any<string>()).Returns(granted);
            return checker;
        }
    }
}
