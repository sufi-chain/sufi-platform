using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Inbox;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Inbox;

public class InboxFeedPresentationTests
{
    [Fact]
    public void Empty_state_waits_until_the_first_load_completes()
    {
        InboxFeedPresentation.ShowSkeleton(initialLoadCompleted: false, isLoading: false, itemCount: 0).ShouldBeTrue();
        InboxFeedPresentation.ShowEmpty(initialLoadCompleted: false, isLoading: false, itemCount: 0).ShouldBeFalse();

        InboxFeedPresentation.ShowSkeleton(initialLoadCompleted: false, isLoading: true, itemCount: 0).ShouldBeTrue();
        InboxFeedPresentation.ShowEmpty(initialLoadCompleted: false, isLoading: true, itemCount: 0).ShouldBeFalse();
    }

    [Fact]
    public void Empty_state_shows_only_after_a_completed_load_returns_nothing()
    {
        InboxFeedPresentation.ShowSkeleton(initialLoadCompleted: true, isLoading: false, itemCount: 0).ShouldBeFalse();
        InboxFeedPresentation.ShowEmpty(initialLoadCompleted: true, isLoading: false, itemCount: 0).ShouldBeTrue();
    }

    [Fact]
    public void Existing_rows_stay_visible_while_a_later_load_runs()
    {
        InboxFeedPresentation.ShowSkeleton(initialLoadCompleted: true, isLoading: true, itemCount: 16).ShouldBeFalse();
        InboxFeedPresentation.ShowEmpty(initialLoadCompleted: true, isLoading: true, itemCount: 16).ShouldBeFalse();
    }
}
