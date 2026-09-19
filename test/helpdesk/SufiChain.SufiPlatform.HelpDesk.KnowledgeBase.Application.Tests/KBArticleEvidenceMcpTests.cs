using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Data;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Guidance;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleEvidenceMcpTests
{
    [Fact]
    public async Task Should_Return_Exact_Version_Hash_And_Bounded_Passages()
    {
        var fixture = new Fixture("Reset password with code E-42.");
        var input = fixture.Input();
        input.MaxCharacters = 8;
        var result = await fixture.Service.GetArticleEvidenceAsync(input);

        result.Status.ShouldBe("ok");
        result.Content.ShouldBe("Reset pa");
        result.IsTruncated.ShouldBeTrue();
        result.NextOffset.ShouldBe(8);
        result.Citation!.VersionId.ShouldBe(fixture.Version.Id);
        result.Citation.ContentSha256.ShouldBe(Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(fixture.Version.Content))).ToLowerInvariant());
        result.Citation.EvidenceToolName.ShouldBe(HelpDeskKbGuidanceMcpToolSeedTexts.GetArticleEvidence);

        input.Offset = result.NextOffset.Value;
        input.MaxCharacters = 12000;
        var rest = await fixture.Service.GetArticleEvidenceAsync(input);
        (result.Content + rest.Content).ShouldBe(fixture.Version.Content);
        rest.NextOffset.ShouldBeNull();
        rest.IsTruncated.ShouldBeTrue(); // It is a trailing passage, not the whole source.
    }

    [Fact]
    public async Task Should_Not_Split_Unicode_Surrogate_Pairs()
    {
        var fixture = new Fixture("A😀B");
        var input = fixture.Input();
        input.MaxCharacters = 2;
        var first = await fixture.Service.GetArticleEvidenceAsync(input);
        first.Content.ShouldBe("A");
        first.NextOffset.ShouldBe(1);
        input.Offset = 1;
        var second = await fixture.Service.GetArticleEvidenceAsync(input);
        second.Content.ShouldBe("😀");
        second.NextOffset.ShouldBe(3);
        input.Offset = 2;
        var exception = await Should.ThrowAsync<BusinessException>(() => fixture.Service.GetArticleEvidenceAsync(input));
        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.InvalidEvidenceOffset);
    }

    [Theory]
    [InlineData("en", "Reset your password.")]
    [InlineData("fa", "رمز عبور را بازنشانی کنید.")]
    [InlineData("ar", "أعد تعيين كلمة المرور.")]
    [InlineData("es", "Restablece tu contraseña.")]
    public async Task Should_Preserve_Source_Language_And_Text(string language, string content)
    {
        var fixture = new Fixture(content, language);
        var result = await fixture.Service.GetArticleEvidenceAsync(fixture.Input());
        result.Content.ShouldBe(content);
        result.Citation!.LanguageCode.ShouldBe(language);
        result.IsTruncated.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task Should_Reject_Direct_Tool_Invocation_Without_Access(bool authenticated, bool permitted, bool enabled)
    {
        var fixture = new Fixture();
        fixture.User.IsAuthenticated.Returns(authenticated);
        fixture.Permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Default).Returns(permitted);
        fixture.Features.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBase).Returns(enabled);

        await Should.ThrowAsync<AbpAuthorizationException>(() => fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
        await fixture.Projects.DidNotReceive().FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(ArticleVisibility.Internal)]
    [InlineData(ArticleVisibility.Private)]
    public async Task Should_Withhold_Nonpublic_Content_Without_Reading_Versions(ArticleVisibility visibility)
    {
        var fixture = new Fixture();
        fixture.Article.SetVisibility(visibility);
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
        await fixture.Versions.DidNotReceive().FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Recheck_Unpublication_And_Project_Deactivation()
    {
        var fixture = new Fixture();
        (await fixture.Service.GetArticleEvidenceAsync(fixture.Input())).Status.ShouldBe("ok");
        fixture.Article.Unpublish(new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc));
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));

        fixture = new Fixture();
        fixture.Project.Deactivate();
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
    }

    [Fact]
    public async Task Should_Withhold_Old_Drafts_And_Changed_Translations()
    {
        var fixture = new Fixture();
        fixture.Translation.Update("Changed title", "This current content supersedes the citation.");
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
    }

    [Fact]
    public async Task Should_Not_Trust_A_Version_From_Another_Article()
    {
        var fixture = new Fixture();
        var foreign = new KBArticleVersion(fixture.Version.Id, Guid.NewGuid(), "en", 1,
            fixture.Version.Title, fixture.Version.Content, "foreign source");
        fixture.Versions.FindAsync(fixture.Version.Id, false, Arg.Any<CancellationToken>()).Returns(foreign);
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
    }

    [Fact]
    public async Task Should_Not_Trust_Cross_Project_Repository_Results()
    {
        var fixture = new Fixture();
        var otherProject = new HelpDeskProject(fixture.Project.Id, "Other", "other", fixture.TenantId);
        var input = fixture.Input();
        input.ProjectId = Guid.NewGuid();
        // Correct repositories cannot return this. The returned identity still
        // must match the request before a tool can claim source binding.
        fixture.Projects.FindAsync(input.ProjectId, false, Arg.Any<CancellationToken>()).Returns(otherProject);
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(input));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Not_Bypass_Tenant_Checks_In_Host_Context(bool hostContext)
    {
        var fixture = new Fixture();
        fixture.Tenant.Id.Returns(hostContext ? (Guid?)null : Guid.NewGuid());
        AssertUnavailable(await fixture.Service.GetArticleEvidenceAsync(fixture.Input()));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 12001)]
    [InlineData(-1, 4000)]
    public async Task Should_Reject_Invalid_Passage_Bounds(int offset, int maxCharacters)
    {
        var fixture = new Fixture();
        var input = fixture.Input();
        input.Offset = offset;
        input.MaxCharacters = maxCharacters;
        var exception = await Should.ThrowAsync<BusinessException>(() => fixture.Service.GetArticleEvidenceAsync(input));
        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.InvalidEvidenceRequest);
    }

    [Fact]
    public async Task Should_Enrich_Search_Citations_Only_With_Matching_Versions()
    {
        var fixture = new Fixture();
        fixture.Projects.FindBySlugAsync(fixture.Project.Slug, true, Arg.Any<CancellationToken>()).Returns(fixture.Project);
        fixture.Articles.GetPublishedListAsync(fixture.Project.Id, null, null, true, Arg.Any<CancellationToken>())
            .Returns(new List<KBArticle> { fixture.Article });
        var service = new HelpDeskKbGuidanceMcpAppService(fixture.Projects, fixture.Articles);
        var input = new SearchProjectGuidanceInput { ProjectSlug = fixture.Project.Slug, Query = "Reset" };
        var result = await service.SearchProjectGuidanceAsync(input);
        result.Articles.Single().Citation.VersionId.ShouldBe(fixture.Version.Id);

        fixture.Translation.Update("Reset password", "Reset using the changed instructions.");
        result = await service.SearchProjectGuidanceAsync(input);
        result.Articles.Single().Citation.VersionId.ShouldBeNull();
        result.Articles.Single().Citation.EvidenceStatus.ShouldBe("unverified");
    }

    [Fact]
    public void Should_Discover_A_Read_Only_Permission_Gated_Evidence_Tool()
    {
        var attribute = typeof(HelpDeskKbEvidenceMcpAppService)
            .GetMethod(nameof(HelpDeskKbEvidenceMcpAppService.GetArticleEvidenceAsync))!
            .GetCustomAttribute<SufiAiMcpToolAttribute>()!;
        attribute.Name.ShouldBe(HelpDeskKbGuidanceMcpToolSeedTexts.GetArticleEvidence);
        attribute.RequiresPermission.ShouldBeTrue();
        attribute.PermissionName.ShouldBe(KnowledgeBasePermissions.Articles.Default);
        HelpDeskKbGuidanceMcpToolSeedTexts.EvidenceDisplayNames.Keys.OrderBy(x => x).ShouldBe(new[] { "ar", "en", "es", "fa" });
        HelpDeskKbGuidanceMcpToolSeedTexts.EvidenceDescriptions.Keys.OrderBy(x => x).ShouldBe(new[] { "ar", "en", "es", "fa" });
    }

    private static void AssertUnavailable(ArticleEvidenceResultDto result)
    {
        result.Status.ShouldBe("unavailable");
        result.Citation.ShouldBeNull();
        result.Title.ShouldBeNull();
        result.Content.ShouldBeNull();
        result.NextOffset.ShouldBeNull();
    }

    private sealed class Fixture
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public ICurrentTenant Tenant { get; } = Substitute.For<ICurrentTenant>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public IPermissionChecker Permissions { get; } = Substitute.For<IPermissionChecker>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public IKBArticleRepository Articles { get; } = Substitute.For<IKBArticleRepository>();
        public IKBArticleVersionRepository Versions { get; } = Substitute.For<IKBArticleVersionRepository>();
        public IHelpDeskProjectRepository Projects { get; } = Substitute.For<IHelpDeskProjectRepository>();
        public HelpDeskProject Project { get; }
        public KBArticle Article { get; }
        public KBArticleTranslation Translation { get; }
        public KBArticleVersion Version { get; }
        public HelpDeskKbEvidenceMcpAppService Service { get; }

        public Fixture(string content = "Reset your password.", string language = "en")
        {
            Tenant.Id.Returns(TenantId);
            User.IsAuthenticated.Returns(true);
            Permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Default).Returns(true);
            Features.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBase).Returns(true);
            Project = new HelpDeskProject(Guid.NewGuid(), "Support", "support", TenantId);
            Article = new KBArticle(Guid.NewGuid(), Project.Id, Guid.NewGuid(), TenantId);
            Translation = Article.AddTranslation(Guid.NewGuid(), language, "Reset password", "reset", content);
            Version = KBArticleVersion.CreateFromArticle(Guid.NewGuid(), Article, Translation, "Published baseline");
            Article.AddVersion(Version);
            Article.Publish(new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc));
            Projects.FindAsync(Project.Id, false, Arg.Any<CancellationToken>()).Returns(Project);
            Articles.FindAsync(Article.Id, false, Arg.Any<CancellationToken>()).Returns(Article);
            Versions.FindAsync(Version.Id, false, Arg.Any<CancellationToken>()).Returns(Version);
            Articles.FindTranslationAsync(Article.Id, language, Arg.Any<CancellationToken>()).Returns(Translation);
            Service = new HelpDeskKbEvidenceMcpAppService(Articles, Versions, Projects, Tenant, User, Permissions, Features);
        }

        public GetArticleEvidenceInput Input() => new()
        {
            ProjectId = Project.Id, ArticleId = Article.Id, VersionId = Version.Id, LanguageCode = Version.LanguageCode
        };
    }
}
