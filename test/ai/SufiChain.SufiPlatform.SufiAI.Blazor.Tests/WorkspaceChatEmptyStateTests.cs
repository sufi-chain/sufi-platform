using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class WorkspaceChatEmptyStateTests
{
    [Fact]
    public void A_pending_lookup_does_not_ask_the_user_to_choose()
    {
        WorkspaceChatEmptyState.ShowLoading(loading: true, activeCount: 0, hasWorkspace: false).ShouldBeTrue();
        WorkspaceChatEmptyState.ShowChooseWorkspace(loading: true, activeCount: 0, hasWorkspace: false).ShouldBeFalse();
    }

    [Fact]
    public void One_active_workspace_stays_on_the_loading_state_until_it_is_selected()
    {
        WorkspaceChatEmptyState.ShowLoading(loading: false, activeCount: 1, hasWorkspace: false).ShouldBeTrue();
        WorkspaceChatEmptyState.ShowChooseWorkspace(loading: false, activeCount: 1, hasWorkspace: false).ShouldBeFalse();
        WorkspaceChatEmptyState.ShowLoading(loading: false, activeCount: 1, hasWorkspace: true).ShouldBeFalse();
    }

    [Fact]
    public void Zero_or_several_workspaces_ask_the_user_to_choose_after_loading()
    {
        WorkspaceChatEmptyState.ShowChooseWorkspace(loading: false, activeCount: 0, hasWorkspace: false).ShouldBeTrue();
        WorkspaceChatEmptyState.ShowChooseWorkspace(loading: false, activeCount: 3, hasWorkspace: false).ShouldBeTrue();
        WorkspaceChatEmptyState.ShowLoading(loading: false, activeCount: 3, hasWorkspace: false).ShouldBeFalse();
    }
}
