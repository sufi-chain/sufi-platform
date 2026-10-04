using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class PublicMenuAnonymousAccessTests
{
    [Theory]
    [InlineData("Public")]
    [InlineData("public")]
    [InlineData("SufiCMS")]
    [InlineData("suficms")]
    public async Task Built_in_public_site_contexts_stay_readable(string contextType)
    {
        var access = new PublicMenuAnonymousAccess([]);

        (await access.AllowsAsync(contextType, null)).ShouldBeTrue();
        (await access.AllowsAsync(contextType, Guid.NewGuid())).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Main")]
    [InlineData("SufiHelpDesk.KnowledgeBase.Project")]
    public async Task Unknown_contexts_are_denied_until_a_policy_allows_them(string? contextType)
    {
        var access = new PublicMenuAnonymousAccess([]);

        (await access.AllowsAsync(contextType, Guid.NewGuid())).ShouldBeFalse();
    }

    [Fact]
    public async Task A_denying_policy_hides_a_context_even_when_another_policy_allows_it()
    {
        var projectId = Guid.NewGuid();
        var access = new PublicMenuAnonymousAccess(
        [
            new StubVisibility("SufiHelpDesk.KnowledgeBase.Project", true),
            new StubVisibility("SufiHelpDesk.KnowledgeBase.Project", false)
        ]);

        (await access.AllowsAsync("SufiHelpDesk.KnowledgeBase.Project", projectId)).ShouldBeFalse();
        (await access.AllowsAsync("Public", null)).ShouldBeTrue();
    }

    [Fact]
    public async Task An_allowing_policy_opens_only_its_context()
    {
        var projectId = Guid.NewGuid();
        var access = new PublicMenuAnonymousAccess(
        [
            new StubVisibility("SufiHelpDesk.KnowledgeBase.Project", true, projectId)
        ]);

        (await access.AllowsAsync("SufiHelpDesk.KnowledgeBase.Project", projectId)).ShouldBeTrue();
        (await access.AllowsAsync("SufiHelpDesk.KnowledgeBase.Project", Guid.NewGuid())).ShouldBeFalse();
        (await access.AllowsAsync("Main", null)).ShouldBeFalse();
    }

    [Fact]
    public async Task Anonymous_tree_and_slug_reads_skip_private_contexts_and_keep_public_site_menus()
    {
        var menus = new RecordingPublicMenus(
        [
            new StubVisibility("SufiHelpDesk.KnowledgeBase.Project", false)
        ]);

        var privateTree = await menus.GetTreeAsync("SufiHelpDesk.KnowledgeBase.Project", Guid.NewGuid(), "Categories");
        var privateItem = await menus.FindItemBySlugAsync("SufiHelpDesk.KnowledgeBase.Project", Guid.NewGuid(), "Categories", "billing");
        privateTree.ShouldBeEmpty();
        privateItem.ShouldBeNull();
        menus.TreeLoads.ShouldBe(0);
        menus.ItemLoads.ShouldBe(0);

        var landing = await menus.GetTreeAsync("Public", null, "Default");
        var cms = await menus.GetTreeAsync("SufiCMS", null, "Header");
        var landingItem = await menus.FindItemBySlugAsync("Public", null, "Default", "home");
        landing.Single().DisplayName.ShouldBe("Public");
        cms.Single().DisplayName.ShouldBe("SufiCMS");
        landingItem.ShouldNotBeNull();
        landingItem.DisplayName.ShouldBe("home");
        menus.TreeLoads.ShouldBe(2);
        menus.ItemLoads.ShouldBe(1);
    }

    [Fact]
    public async Task Tree_by_id_returns_null_for_a_private_context_and_the_tree_for_a_public_one()
    {
        var privateMenu = new Menu(Guid.NewGuid(), "SufiHelpDesk.KnowledgeBase.Project", Guid.NewGuid(), "Categories", "Categories");
        var publicMenu = new Menu(Guid.NewGuid(), "Public", null, "Default", "Default");
        var missingId = Guid.NewGuid();
        var repository = Substitute.For<IMenuRepository>();
        repository.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var id = call.ArgAt<Guid>(0);
                if (id == privateMenu.Id)
                {
                    return privateMenu;
                }

                if (id == publicMenu.Id)
                {
                    return publicMenu;
                }

                return null;
            });

        var menus = new RecordingPublicMenus(
            [new StubVisibility("SufiHelpDesk.KnowledgeBase.Project", false)],
            repository);

        (await menus.GetTreeByIdAsync(missingId)).ShouldBeNull();
        (await menus.GetTreeByIdAsync(privateMenu.Id)).ShouldBeNull();
        menus.TreeLoads.ShouldBe(0);

        var tree = await menus.GetTreeByIdAsync(publicMenu.Id);
        tree.ShouldNotBeNull();
        tree.Single().DisplayName.ShouldBe("Public");
        menus.TreeLoads.ShouldBe(1);
    }

    private sealed class StubVisibility : IPublicMenuContextVisibility
    {
        private readonly string _contextType;
        private readonly bool _visible;
        private readonly Guid? _contextId;

        public StubVisibility(string contextType, bool visible, Guid? contextId = null)
        {
            _contextType = contextType;
            _visible = visible;
            _contextId = contextId;
        }

        public Task<bool?> IsPubliclyVisibleAsync(string contextType, Guid? contextId)
        {
            if (!string.Equals(contextType, _contextType, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<bool?>(null);
            }

            if (_contextId.HasValue && _contextId != contextId)
            {
                return Task.FromResult<bool?>(false);
            }

            return Task.FromResult<bool?>(_visible);
        }
    }

    private sealed class RecordingPublicMenus : PublicMenuAppService
    {
        public int TreeLoads { get; private set; }
        public int ItemLoads { get; private set; }

        public RecordingPublicMenus(IEnumerable<IPublicMenuContextVisibility> policies, IMenuRepository? menus = null)
            : base(menus ?? Substitute.For<IMenuRepository>(), null!, null!, null!, null!, policies)
        {
        }

        protected override Task<List<MenuItemTreeDto>> LoadTreeAsync(string contextType, Guid? contextId, string menuName)
        {
            TreeLoads++;
            return Task.FromResult(new List<MenuItemTreeDto> { new() { DisplayName = contextType } });
        }

        protected override Task<MenuItemDto?> LoadItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug)
        {
            ItemLoads++;
            return Task.FromResult<MenuItemDto?>(new MenuItemDto { DisplayName = slug });
        }

    }
}
