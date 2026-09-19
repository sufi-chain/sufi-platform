using System.Linq.Expressions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Relations;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.HelpDesk.Relations;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleRelationEntityResolverTests
{
    [Fact]
    public async Task Should_Resolve_Trusted_Project_Scope_Without_Content_And_Separate_Relate_Access()
    {
        var fixture = new Fixture();
        var article = fixture.AddArticle();
        var result = await fixture.Resolver.ResolveAsync([fixture.Reference(article)]);
        result.Single().Scope.ShouldBe(new TagEntityReference(HelpDeskRelationEntityTypes.Project, fixture.Project.Id));
        result.Single().TenantId.ShouldBe(fixture.TenantId);
        result.Single().CanRead.ShouldBeTrue();
        result.Single().CanRelate.ShouldBeFalse();

        fixture.Permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Edit).Returns(true);
        (await fixture.Resolver.ResolveAsync([fixture.Reference(article)])).Single().CanRelate.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Batch_Articles_And_Projects_Instead_Of_Querying_Each_Endpoint()
    {
        var fixture = new Fixture();
        var first = fixture.AddArticle();
        var second = fixture.AddArticle();
        var result = await fixture.Resolver.ResolveAsync([fixture.Reference(first), fixture.Reference(second)]);
        result.Count.ShouldBe(2);
        await fixture.Articles.Received(1).GetListAsync(
            Arg.Any<Expression<Func<KBArticle, bool>>>(), false, Arg.Any<CancellationToken>());
        await fixture.Projects.Received(1).GetListAsync(
            Arg.Any<Expression<Func<HelpDeskProject, bool>>>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Exclude_Private_Draft_And_Foreign_Tenant_Records()
    {
        var fixture = new Fixture();
        var privateArticle = fixture.AddArticle();
        privateArticle.SetVisibility(ArticleVisibility.Private);
        var draft = fixture.AddArticle();
        draft.Unpublish(new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc));
        var foreign = fixture.AddArticle(Guid.NewGuid());
        var result = await fixture.Resolver.ResolveAsync(
            [fixture.Reference(privateArticle), fixture.Reference(draft), fixture.Reference(foreign)]);
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Recheck_Project_Activation_And_Permission()
    {
        var fixture = new Fixture();
        var article = fixture.AddArticle();
        var reference = fixture.Reference(article);
        (await fixture.Resolver.ResolveAsync([reference])).Count.ShouldBe(1);
        fixture.Project.Deactivate();
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        fixture.Project.Activate();
        fixture.Permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Default).Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Not_Resolve_Anonymous_Or_Disabled_Feature_Requests()
    {
        var fixture = new Fixture();
        var reference = fixture.Reference(fixture.AddArticle());
        fixture.User.IsAuthenticated.Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        fixture.User.IsAuthenticated.Returns(true);
        fixture.Features.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBase).Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        await fixture.Articles.DidNotReceive().GetListAsync(
            Arg.Any<Expression<Func<KBArticle, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Reject_Wrong_Types_And_Excessive_Batches()
    {
        var fixture = new Fixture();
        await Should.ThrowAsync<ArgumentException>(() => fixture.Resolver.ResolveAsync(
            [new TagEntityReference(HelpDeskRelationEntityTypes.Ticket, Guid.NewGuid())]));
        var references = Enumerable.Range(0, 101)
            .Select(_ => new TagEntityReference(HelpDeskRelationEntityTypes.Article, Guid.NewGuid())).ToList();
        await Should.ThrowAsync<ArgumentException>(() => fixture.Resolver.ResolveAsync(references));
    }

    private sealed class Fixture
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public IKBArticleRepository Articles { get; } = Substitute.For<IKBArticleRepository>();
        public IHelpDeskProjectRepository Projects { get; } = Substitute.For<IHelpDeskProjectRepository>();
        public IPermissionChecker Permissions { get; } = Substitute.For<IPermissionChecker>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public List<KBArticle> Records { get; } = [];
        public HelpDeskProject Project { get; }
        public KBArticleRelationEntityResolver Resolver { get; }

        public Fixture()
        {
            var tenant = Substitute.For<ICurrentTenant>();
            tenant.Id.Returns(TenantId);
            User.IsAuthenticated.Returns(true);
            Permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Default).Returns(true);
            Features.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBase).Returns(true);
            Project = new HelpDeskProject(Guid.NewGuid(), "Support", "support", TenantId);
            Articles.GetListAsync(Arg.Any<Expression<Func<KBArticle, bool>>>(), false, Arg.Any<CancellationToken>())
                .Returns(call => Records.Where(call.Arg<Expression<Func<KBArticle, bool>>>().Compile()).ToList());
            Projects.GetListAsync(Arg.Any<Expression<Func<HelpDeskProject, bool>>>(), false, Arg.Any<CancellationToken>())
                .Returns(call => new[] { Project }.Where(call.Arg<Expression<Func<HelpDeskProject, bool>>>().Compile()).ToList());
            Resolver = new KBArticleRelationEntityResolver(Articles, Projects, tenant, User, Permissions, Features);
        }

        public KBArticle AddArticle(Guid? tenantId = null)
        {
            var article = new KBArticle(Guid.NewGuid(), Project.Id, Guid.NewGuid(), tenantId ?? TenantId);
            article.AddTranslation(Guid.NewGuid(), "en", "Article", Guid.NewGuid().ToString("N"), "Evidence.");
            article.Publish(new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc));
            Records.Add(article);
            return article;
        }

        public TagEntityReference Reference(KBArticle article) => new(HelpDeskRelationEntityTypes.Article, article.Id);
    }
}
