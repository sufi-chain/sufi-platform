using Shouldly;
using SufiChain.SufiPlatform.SufiCMS;
using SufiChain.SufiPlatform.SufiCMS.Pages;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS.Pages;

public class CmsPageVariantWorkflowTests : AbpIntegratedTest<SufiCMSTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    [Fact]
    public void Publish_Should_Copy_Draft_And_Clear_Review()
    {
        GetRequiredService<Volo.Abp.Guids.IGuidGenerator>().Create().ShouldNotBe(Guid.Empty);
        var variant = new PageVariant(Guid.NewGuid(), Guid.NewGuid(), "en", "Home");
        variant.Publish("abc123", new DateTime(2026, 9, 29, 0, 0, 0, DateTimeKind.Utc));

        variant.IsPublished.ShouldBeTrue();
        variant.PublishedTitle.ShouldBe("Home");
        variant.ReviewStatus.ShouldBe(PageReviewStatus.None);
    }

    [Fact]
    public void RequestChanges_Should_Reject_An_Empty_Note()
    {
        var variant = new PageVariant(Guid.NewGuid(), Guid.NewGuid(), "en", "Home");
        Should.Throw<ArgumentException>(() => variant.RequestChanges(" "));
        variant.ReviewStatus.ShouldNotBe(PageReviewStatus.ChangesRequested);
    }
}
