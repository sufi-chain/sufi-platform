using Shouldly;
using SufiChain.SufiPlatform.BackgroundJobs;
using Xunit;

namespace SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore.Tests;

public class BackgroundJobActionVisibilityTests
{
    [Fact]
    public void BackgroundJobs_ViewOnlyUser_SeesViewButNoRetryAbandonDelete()
    {
        BackgroundJobActionVisibility.ShowRetry(canRetry: false, isAbandoned: true).ShouldBeFalse();
        BackgroundJobActionVisibility.ShowAbandon(canRetry: false, isAbandoned: false).ShouldBeFalse();
        BackgroundJobActionVisibility.ShowDelete(canDelete: false).ShouldBeFalse();
    }

    [Fact]
    public void BackgroundJobs_ManageUser_SeesRetryAbandonDelete()
    {
        BackgroundJobActionVisibility.ShowRetry(canRetry: true, isAbandoned: true).ShouldBeTrue();
        BackgroundJobActionVisibility.ShowAbandon(canRetry: true, isAbandoned: false).ShouldBeTrue();
        BackgroundJobActionVisibility.ShowRetry(canRetry: true, isAbandoned: false).ShouldBeFalse();
        BackgroundJobActionVisibility.ShowDelete(canDelete: true).ShouldBeTrue();
    }
}
