using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.UI.Authorization;
using SufiChain.SufiPlatform.UI.Features;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Services.Navigation;
using SufiChain.SufiPlatform.UI.Users;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

/// <summary>
/// sufi-chain#490: the menu builder must drop items whose required feature is off (edition or tenant
/// override), not only items the user lacks permission for. This is what removes KnowledgeBase, Ticketing,
/// LiveChat and AI Credits from the sidebar after F5 while the feature stays disabled, and restores them
/// once it is turned back on without a re-login.
/// </summary>
public class DefaultMenuManagerFeatureFilteringTests
{
    private const string KnowledgeBaseFeature = "HelpDesk.KnowledgeBase";
    private const string TicketingFeature = "HelpDesk.Ticketing";

    [Fact]
    public async Task Item_with_a_disabled_feature_is_removed()
    {
        var manager = Build(featureEnabled: false, menu => menu
            .AddItem(new ApplicationMenuItem("kb", "KnowledgeBase", "/panel/kb")
                .RequireFeatures(KnowledgeBaseFeature))
            .AddItem(new ApplicationMenuItem("tickets", "Ticketing", "/panel/tickets")));

        var result = await manager.GetMainMenuAsync();

        result.Items.ShouldNotContain(item => item.Name == "kb");
        result.Items.ShouldContain(item => item.Name == "tickets");
    }

    [Fact]
    public async Task Item_with_an_enabled_feature_stays_visible()
    {
        var manager = Build(featureEnabled: true, menu => menu
            .AddItem(new ApplicationMenuItem("kb", "KnowledgeBase", "/panel/kb")
                .RequireFeatures(KnowledgeBaseFeature)));

        var result = await manager.GetMainMenuAsync();

        result.Items.ShouldContain(item => item.Name == "kb");
    }

    [Fact]
    public async Task Disabled_feature_hides_its_subtree_and_the_empty_parent()
    {
        var manager = Build(featureEnabled: false, menu =>
        {
            var parent = new ApplicationMenuItem("helpdesk", "HelpDesk");
            parent.AddItem(new ApplicationMenuItem("kb", "KnowledgeBase", "/panel/kb")
                .RequireFeatures(KnowledgeBaseFeature));
            parent.AddItem(new ApplicationMenuItem("tickets", "Ticketing", "/panel/tickets")
                .RequireFeatures(TicketingFeature));
            menu.AddItem(parent);
            menu.AddItem(new ApplicationMenuItem("dashboard", "Dashboard", "/panel"));
        });

        var result = await manager.GetMainMenuAsync();

        result.Items.ShouldNotContain(item => item.Name == "helpdesk");
        result.Items.ShouldContain(item => item.Name == "dashboard");
    }

    [Fact]
    public async Task Only_the_gated_leaf_is_pruned_when_the_parent_has_other_children()
    {
        var manager = Build(featureEnabled: false, menu =>
        {
            var parent = new ApplicationMenuItem("helpdesk", "HelpDesk");
            parent.AddItem(new ApplicationMenuItem("kb", "KnowledgeBase", "/panel/kb")
                .RequireFeatures(KnowledgeBaseFeature));
            parent.AddItem(new ApplicationMenuItem("tickets", "Ticketing", "/panel/tickets"));
            menu.AddItem(parent);
        });

        var result = await manager.GetMainMenuAsync();

        var helpDesk = result.Items.Single(item => item.Name == "helpdesk");
        helpDesk.Items.ShouldNotContain(item => item.Name == "kb");
        helpDesk.Items.ShouldContain(item => item.Name == "tickets");
    }

    [Fact]
    public async Task Feature_checker_receives_every_required_feature_name()
    {
        var featureChecker = Substitute.For<ISufiFeatureChecker>();
        featureChecker.IsEnabledAsync(Arg.Any<IEnumerable<string>>())
            .Returns(callInfo =>
            {
                var names = callInfo.Arg<IEnumerable<string>>();
                return Task.FromResult(names.Distinct().ToDictionary(name => name, _ => true));
            });

        var manager = Build(
            featureChecker,
            menu => menu
                .AddItem(new ApplicationMenuItem("kb", "KnowledgeBase", "/panel/kb")
                    .RequireFeatures(KnowledgeBaseFeature))
                .AddItem(new ApplicationMenuItem("tickets", "Ticketing", "/panel/tickets")
                    .RequireFeatures(TicketingFeature)));

        await manager.GetMainMenuAsync();

        await featureChecker.Received(1).IsEnabledAsync(
            Arg.Is<IEnumerable<string>>(names =>
                names.Contains(KnowledgeBaseFeature) && names.Contains(TicketingFeature)));
    }

    private static DefaultMenuManager Build(bool featureEnabled, Action<ApplicationMenu> configure)
    {
        var featureChecker = Substitute.For<ISufiFeatureChecker>();
        featureChecker.IsEnabledAsync(Arg.Any<string>())
            .Returns(Task.FromResult(featureEnabled));
        featureChecker.IsEnabledAsync(Arg.Any<IEnumerable<string>>())
            .Returns(callInfo =>
            {
                var names = callInfo.Arg<IEnumerable<string>>();
                return Task.FromResult(names.Distinct().ToDictionary(name => name, _ => featureEnabled));
            });

        return Build(featureChecker, configure);
    }

    private static DefaultMenuManager Build(ISufiFeatureChecker featureChecker, Action<ApplicationMenu> configure)
    {
        var options = new SufiNavigationOptions();
        options.MenuContributors.Add(new TestMenuContributor(configure));

        var permissionChecker = Substitute.For<ISufiPermissionChecker>();
        permissionChecker.IsGrantedAsync(Arg.Any<string>())
            .Returns(Task.FromResult(true));
        permissionChecker.IsGrantedAsync(Arg.Any<IEnumerable<string>>())
            .Returns(callInfo =>
            {
                var names = callInfo.Arg<IEnumerable<string>>();
                return Task.FromResult(names.Distinct().ToDictionary(name => name, _ => true));
            });

        var currentUser = Substitute.For<ICurrentUserAccessor>();
        currentUser.IsAuthenticated.Returns(true);

        return new DefaultMenuManager(
            new ServiceCollection().BuildServiceProvider(),
            Options.Create(options),
            permissionChecker,
            featureChecker,
            currentUser);
    }

    private sealed class TestMenuContributor : IMenuContributor
    {
        private readonly Action<ApplicationMenu> _configure;

        public TestMenuContributor(Action<ApplicationMenu> configure)
        {
            _configure = configure;
        }

        public Task ConfigureMenuAsync(MenuConfigurationContext context)
        {
            _configure(context.Menu);
            return Task.CompletedTask;
        }
    }
}
