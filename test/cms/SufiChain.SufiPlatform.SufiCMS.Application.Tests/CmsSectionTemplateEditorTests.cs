using SufiChain.SufiPlatform.SufiCMS.Delivery;
using SufiChain.SufiPlatform.SufiCMS.Pages;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS;

public class CmsSectionTemplateEditorTests
{
    private static readonly Guid SampleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Section_Template_And_Content_Links_Match_Editor_Page_Routes()
    {
        var templates = ReadEditorPageTemplates();
        templates.ShouldContain(CmsAdminRoutes.ContentEditorTemplate);
        templates.ShouldContain(CmsAdminRoutes.PagesEditorTemplate);
        templates.ShouldContain(CmsAdminRoutes.SectionTemplateEditorTemplate);

        CmsAdminRoutes.Matches(CmsAdminRoutes.ContentEditorTemplate, CmsAdminRoutes.PageEditor(SampleId)).ShouldBeTrue();
        CmsAdminRoutes.Matches(CmsAdminRoutes.PagesEditorTemplate, CmsAdminRoutes.PagesEditorRoute + "/" + SampleId.ToString("D")).ShouldBeTrue();
        CmsAdminRoutes.Matches(CmsAdminRoutes.SectionTemplateEditorTemplate, CmsAdminRoutes.SectionTemplateEditor(SampleId)).ShouldBeTrue();

        CmsAdminRoutes.Matches(CmsAdminRoutes.ContentEditorTemplate, CmsAdminRoutes.ContentEditorRoute + "/section-hero").ShouldBeFalse();
        CmsAdminRoutes.Matches(CmsAdminRoutes.SectionTemplateEditorTemplate, CmsAdminRoutes.SectionTemplatesRoute + "/section-hero").ShouldBeFalse();
        CmsAdminRoutes.Matches(CmsAdminRoutes.SectionTemplateEditorTemplate, CmsAdminRoutes.PageEditor(SampleId)).ShouldBeFalse();
    }

    [Fact]
    public void Editor_Lookup_Opens_A_Draft_Section_Template_And_A_Host_Template()
    {
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var otherTenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var draft = SectionTemplate("hero", tenantId: null);
        draft.Status.ShouldBe(PageStatus.Draft);
        draft.Variants.Single().IsPublished.ShouldBeFalse();

        CmsEditorPageLookup.IsHostSectionTemplateForTenant(
            draft.IsSectionTemplate, draft.TenantId, draft.Variants.Count, tenantId).ShouldBeTrue();
        CmsEditorPageLookup.IsHostSectionTemplateForTenant(
            draft.IsSectionTemplate, draft.TenantId, draft.Variants.Count, currentTenantId: null).ShouldBeFalse();

        var otherTenant = SectionTemplate("other", otherTenantId);
        CmsEditorPageLookup.IsHostSectionTemplateForTenant(
            otherTenant.IsSectionTemplate, otherTenant.TenantId, otherTenant.Variants.Count, tenantId).ShouldBeFalse();

        var ownDraft = SectionTemplate("own", tenantId);
        CmsDeliveryBuilder.SelectVariant(ownDraft, "fa", publishedOnly: true).ShouldBeNull();
        CmsDeliveryBuilder.SelectVariant(ownDraft, "fa", publishedOnly: false).ShouldNotBeNull();
        CmsEditorPageLookup.SelectEditorCulture(ownDraft.Variants.Select(v => v.Culture).ToList(), ownDraft.DefaultCulture, "en", null)
            .ShouldBe("fa");

        CmsEditorPageLookup.CanCurrentTenantWrite(pageTenantId: null, currentTenantId: null).ShouldBeTrue();
        CmsEditorPageLookup.CanCurrentTenantWrite(tenantId, tenantId).ShouldBeTrue();
        CmsEditorPageLookup.CanCurrentTenantWrite(pageTenantId: null, tenantId).ShouldBeFalse();
        CmsEditorPageLookup.CanCurrentTenantWrite(otherTenantId, tenantId).ShouldBeFalse();
    }

    [Fact]
    public async Task Tenant_Read_Opens_A_Host_Template_And_Writes_Are_Refused()
    {
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var otherTenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var host = SectionTemplate("hero", tenantId: null);
        var other = SectionTemplate("other", otherTenantId);

        var writes = new TenantScopedPageAppService(tenantId, filtered: host, unfiltered: host);
        await ShouldRefuse(writes.UpdateAsync(host.Id, new UpdatePageDto()));
        await ShouldRefuse(writes.DeleteAsync(host.Id));
        await ShouldRefuse(writes.PublishAsync(host.Id, new PublishPageDto()));
        await ShouldRefuse(writes.AcquireLockAsync(host.Id));
        writes.IgnoredTenantLookupCount.ShouldBe(0);

        var reads = new TenantScopedPageAppService(tenantId, filtered: null, unfiltered: host);
        var dto = await reads.GetAsync(host.Id);
        dto.Id.ShouldBe(host.Id);
        dto.TenantId.ShouldBeNull();
        dto.IsSectionTemplate.ShouldBeTrue();
        dto.Status.ShouldBe(PageStatus.Draft);
        reads.IgnoredTenantLookupCount.ShouldBe(1);

        var hidden = new TenantScopedPageAppService(tenantId, filtered: null, unfiltered: other);
        await ShouldRefuse(hidden.GetAsync(other.Id));
    }

    private static async Task ShouldRefuse(Task operation)
    {
        var error = await Should.ThrowAsync<BusinessException>(operation);
        error.Code.ShouldBe(CMSErrorCodes.PageNotFound);
    }

    private static Page SectionTemplate(string slug, Guid? tenantId)
    {
        var page = new Page(Guid.NewGuid(), PageKind.Partial, slug, "fa", tenantId);
        page.SetSectionTemplate(true);
        var variant = page.AddVariant(Guid.NewGuid(), "fa", slug);
        variant.SetDraft("<section>" + slug + "</section>", null, null, new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc));
        return page;
    }

    private static List<string> ReadEditorPageTemplates()
    {
        var path = FindWorkspaceFile("pro-modules/cms/src/SufiChain.SufiPlatform.SufiCMS.Blazor/Pages/CmsPageEditorPage.razor");
        return File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("@page ", StringComparison.Ordinal))
            .Select(line => line.Substring("@page ".Length).Trim().Trim('"'))
            .ToList();
    }

    private static string FindWorkspaceFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }

    private sealed class TenantScopedPageAppService : PageAppService
    {
        private readonly Guid? _tenantId;
        private readonly Page? _filtered;
        private readonly Page? _unfiltered;

        public int IgnoredTenantLookupCount { get; private set; }

        public TenantScopedPageAppService(Guid? tenantId, Page? filtered, Page? unfiltered)
            : base(null!, null!, null!, null!, null!, null!, null!)
        {
            _tenantId = tenantId;
            _filtered = filtered;
            _unfiltered = unfiltered;
        }

        protected override Guid? CurrentTenantId => _tenantId;

        protected override Task<Page?> FindPageAsync(Guid id)
        {
            return Task.FromResult(_filtered != null && _filtered.Id == id ? _filtered : null);
        }

        protected override Task<Page?> FindPageIgnoringTenantAsync(Guid id)
        {
            IgnoredTenantLookupCount++;
            return Task.FromResult(_unfiltered != null && _unfiltered.Id == id ? _unfiltered : null);
        }
    }
}
