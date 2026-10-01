using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Features;
using SufiChain.SufiPlatform.SufiCMS.Pages;
using SufiChain.SufiPlatform.SufiCMS.PendingActions;
using SufiChain.SufiPlatform.SufiCMS.Permissions;
using SufiChain.SufiPlatform.SufiCMS.Site;
using Volo.Abp;
using Volo.Abp.Features;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class CmsPageLifecycleTests
{
    [Fact]
    public void Edit_Lock_Blocks_Other_Users_Until_Expiry()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var page = new Page(Guid.NewGuid(), PageKind.Page, "about", "en");

        page.AcquireLock(owner, now);
        page.IsLockedByOther(other, now).ShouldBeTrue();
        page.IsLockedByOther(owner, now).ShouldBeFalse();
        page.IsLockedByOther(other, now.AddMinutes(CMSConsts.EditLockMinutes + 1)).ShouldBeFalse();

        page.ReleaseLock(other);
        page.IsLockedByOther(other, now).ShouldBeTrue();
        page.ReleaseLock(owner);
        page.IsLockedByOther(other, now).ShouldBeFalse();
    }

    [Fact]
    public void Review_And_Publish_Clear_Review_State()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var page = new Page(Guid.NewGuid(), PageKind.Page, "home", "en");
        var variant = page.AddVariant(Guid.NewGuid(), "en", "Home");
        variant.SetDraft("<section><h1>Home</h1></section>", null, "console.log(1)", now);
        variant.RequestReview("please look");
        variant.ReviewStatus.ShouldBe(PageReviewStatus.InReview);
        variant.Approve("ok");
        variant.ReviewStatus.ShouldBe(PageReviewStatus.Approved);
        variant.DraftScriptDiffersFromPublished().ShouldBeTrue();

        page.Publish(now, _ => "hash");
        page.Status.ShouldBe(PageStatus.Published);
        page.IsLive(now).ShouldBeTrue();
        variant.ReviewStatus.ShouldBe(PageReviewStatus.None);
        variant.IsPublished.ShouldBeTrue();
        variant.DraftScriptDiffersFromPublished().ShouldBeFalse();
    }

    [Fact]
    public void Schedule_Sets_Status_And_Rejects_Expiry_Before_Publish()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var page = new Page(Guid.NewGuid(), PageKind.Post, "news", "en");
        Should.Throw<ArgumentException>(() => page.Schedule(now.AddHours(2), now.AddHours(1)));
        page.Schedule(now.AddHours(1), now.AddDays(1));
        page.Status.ShouldBe(PageStatus.Scheduled);
        page.ClearSchedule();
        page.Status.ShouldBe(PageStatus.Draft);
    }

    [Fact]
    public void Share_Token_Is_Valid_Until_Revoked()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var token = new CmsPreviewShareToken(Guid.NewGuid(), Guid.NewGuid(), "fa", "hash", now.AddHours(1));
        token.IsValid(now).ShouldBeTrue();
        token.Revoke(now);
        token.IsValid(now).ShouldBeFalse();
    }

    [Fact]
    public void Culture_Routes_And_Blog_Slugs_Are_Prefixed()
    {
        CmsUrls.Page(PageKind.Page, "about", "fa", "en").ShouldBe("/fa/p/about");
        CmsUrls.Page(PageKind.Page, CMSConsts.HomeSlug, "en", "en").ShouldBe("/");
        CmsUrls.Page(PageKind.Post, "hello", "ar", "en").ShouldBe("/ar/blog/hello");
        CmsUrls.Preview(Guid.Parse("11111111-1111-1111-1111-111111111111"), "fa", true)
            .ShouldContain("edit=1");
        CmsUrls.PreviewSharePath.ShouldBe("/cms/share/");
    }

    [Fact]
    public void Script_And_Transfer_Permissions_Are_Named()
    {
        CMSPermissions.Pages.EditScripts.ShouldBe("SufiCMS.Pages.EditScripts");
        CMSPermissions.Pages.Review.ShouldBe("SufiCMS.Pages.Review");
        CMSPermissions.SiteTransfer.Default.ShouldBe("SufiCMS.SiteTransfer");
        CMSPermissions.GetAll().ShouldContain(CMSPermissions.Pages.EditScripts);
    }

    [Fact]
    public async Task Image_Quota_Zero_Disables_Generation()
    {
        var features = Substitute.For<IFeatureChecker>();
        features.GetOrNullAsync(CMSFeatures.MaxAiImageGenerationsPerMonth).Returns("0");
        var pending = Substitute.For<ICmsPendingActionRepository>();
        pending.GetConfirmedCountSinceAsync(Arg.Any<CmsPendingActionKind>(), Arg.Any<DateTime>())
            .Returns(0);
        var revisions = Substitute.For<IPageRevisionRepository>();
        var pages = Substitute.For<IPageRepository>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc));

        var quota = new CmsQuotaService(features, pending, revisions, pages, clock);
        var error = await Should.ThrowAsync<BusinessException>(() => quota.EnsureImageGenerationAllowedAsync());
        error.Code.ShouldBe(CMSErrorCodes.AiQuotaExceeded);
    }
}
