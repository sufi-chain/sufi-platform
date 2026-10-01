using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Hooshvare.Mcp;
using SufiChain.SufiPlatform.SufiCMS.PendingActions;
using SufiChain.SufiPlatform.SufiCMS.Transfer;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class CmsPendingActionAndTransferTests
{
    [Fact]
    public void Pending_Action_Expires_And_Rejects_Second_Confirm()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var action = new CmsPendingAction(
            Guid.NewGuid(),
            CmsPendingActionKind.Redirect,
            "Add redirect",
            "{}",
            Guid.NewGuid(),
            null,
            now.AddMinutes(-1));

        action.IsExpired(now).ShouldBeTrue();
        action.Expire(now);
        action.Status.ShouldBe(CmsPendingActionStatus.Expired);
        Should.Throw<BusinessException>(() => action.Confirm("{}", now.AddSeconds(1)))
            .Code.ShouldBe(CMSErrorCodes.PendingActionNotPending);
    }

    [Fact]
    public void Style_Change_On_An_Open_Page_Stays_On_That_Page()
    {
        var pageId = Guid.NewGuid();
        CmsStyleChangeTarget.AppliesToPage(null, pageId).ShouldBeTrue();
        CmsStyleChangeTarget.AppliesToPage("page", pageId).ShouldBeTrue();
        CmsStyleChangeTarget.AppliesToPage("site", pageId).ShouldBeFalse();
        CmsStyleChangeTarget.AppliesToPage(null, null).ShouldBeFalse();
    }

    [Fact]
    public void Attached_Section_Style_Replaces_Page_And_Shortened_Selectors()
    {
        var css = CmsSectionStyleScope.ScopeToSection(
            """.cms-site, [data-cms-section="r"] { --cms-color-muted: #374151; --cms-color-background: #374151; }""",
            "s-aareed-r");

        css.ShouldStartWith("[data-cms-section=\"s-aareed-r\"]");
        css.ShouldContain("--cms-color-background: #374151;");
        css.ShouldNotContain(".cms-site");
        css.ShouldNotContain("data-cms-section=\"r\"");
    }

    [Fact]
    public void Mcp_Tool_Lists_Cannot_Confirm_Actions()
    {
        var tools = SufiCMSHooshvareKeys.Tools.SiteBuilderTools
            .Concat(SufiCMSHooshvareKeys.Tools.WriterTools)
            .Concat(SufiCMSHooshvareKeys.Tools.SiteOpsTools);

        tools.ShouldNotContain(name => name.Contains("confirm", StringComparison.OrdinalIgnoreCase));
        tools.ShouldContain(SufiCMSHooshvareKeys.Tools.ImportFromUrl);
        tools.ShouldContain(SufiCMSHooshvareKeys.Tools.ProposeStyleChange);
    }

    [Fact]
    public void Confirm_Then_Confirm_Again_Is_Rejected()
    {
        var now = new DateTime(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);
        var action = new CmsPendingAction(
            Guid.NewGuid(),
            CmsPendingActionKind.Upload,
            "Upload logo",
            "{}",
            Guid.NewGuid(),
            null,
            now.AddMinutes(10));

        action.Confirm("{\"fileId\":\"1\"}", now);
        action.Status.ShouldBe(CmsPendingActionStatus.Confirmed);
        Should.Throw<BusinessException>(() => action.Confirm("{}", now.AddSeconds(1)))
            .Code.ShouldBe(CMSErrorCodes.PendingActionNotPending);
    }

    [Fact]
    public void Starter_Packages_Are_Current_Version_Drafts()
    {
        var keys = typeof(CmsSiteTransferAppService).Assembly.GetManifestResourceNames()
            .Where(n => n.Contains(".StarterSites.", StringComparison.Ordinal) && n.EndsWith(".json", StringComparison.Ordinal))
            .ToList();

        keys.Count.ShouldBeGreaterThanOrEqualTo(3);
        foreach (var name in keys)
        {
            using var stream = typeof(CmsSiteTransferAppService).Assembly.GetManifestResourceStream(name);
            stream.ShouldNotBeNull();
            var package = System.Text.Json.JsonSerializer.Deserialize<CmsSitePackage>(
                stream!,
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
            package.ShouldNotBeNull();
            package!.Version.ShouldBe(CmsSitePackage.CurrentVersion);
            package.Pages.ShouldNotBeEmpty();
            package.Pages.ShouldContain(p => p.Slug == "home");
            package.Pages.ShouldContain(p => p.Slug == "contact" && p.Variants.Any(v => v.Html.Contains("cms-block")));
        }
    }
}
