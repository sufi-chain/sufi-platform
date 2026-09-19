using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Permissions;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Relations;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Repositories;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.HelpDesk.Relations;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Ticketing;

public class TicketRelationEntityResolverTests
{
    [Fact]
    public void Should_Expose_Ticket_Adapter_Under_The_Tags_Contract()
    {
        var services = new ServiceCollection();
        services.AddAssemblyOf<TicketRelationEntityResolver>();
        services.ShouldContain(x => x.ServiceType == typeof(ITagRelationEntityResolver) &&
            x.ImplementationType == typeof(TicketRelationEntityResolver));
    }

    [Fact]
    public async Task Should_Resolve_Trusted_Project_Scope_Without_Content_And_Separate_Relate_Access()
    {
        var fixture = new Fixture();
        var article = fixture.AddTicket();
        var result = await fixture.Resolver.ResolveAsync([fixture.Reference(article)]);
        result.Single().Scope.ShouldBe(new TagEntityReference(HelpDeskRelationEntityTypes.Project, fixture.Project.Id));
        result.Single().TenantId.ShouldBe(fixture.TenantId);
        result.Single().CanRead.ShouldBeTrue();
        result.Single().CanRelate.ShouldBeFalse();

        fixture.Permissions.IsGrantedAsync(TicketingPermissions.Tickets.Update).Returns(true);
        (await fixture.Resolver.ResolveAsync([fixture.Reference(article)])).Single().CanRelate.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Batch_Tickets_And_Projects_Instead_Of_Querying_Each_Endpoint()
    {
        var fixture = new Fixture();
        var first = fixture.AddTicket();
        var second = fixture.AddTicket();
        var result = await fixture.Resolver.ResolveAsync([fixture.Reference(first), fixture.Reference(second)]);
        result.Count.ShouldBe(2);
        await fixture.Tickets.Received(1).GetListAsync(
            Arg.Any<Expression<Func<Ticket, bool>>>(), false, Arg.Any<CancellationToken>());
        await fixture.Projects.Received(1).GetListAsync(
            Arg.Any<Expression<Func<HelpDeskProject, bool>>>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Exclude_Unscoped_Deleted_And_Foreign_Tenant_Records()
    {
        var fixture = new Fixture();
        var unscoped = fixture.AddTicket();
        unscoped.SetProjectCategory(null, null);
        var deleted = fixture.AddTicket();
        deleted.IsDeleted = true;
        var foreign = fixture.AddTicket(Guid.NewGuid());
        var result = await fixture.Resolver.ResolveAsync(
            [fixture.Reference(unscoped), fixture.Reference(deleted), fixture.Reference(foreign)]);
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Recheck_Project_Activation_And_Permission()
    {
        var fixture = new Fixture();
        var article = fixture.AddTicket();
        var reference = fixture.Reference(article);
        (await fixture.Resolver.ResolveAsync([reference])).Count.ShouldBe(1);
        fixture.Project.Deactivate();
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        fixture.Project.Activate();
        fixture.Permissions.IsGrantedAsync(TicketingPermissions.Tickets.Default).Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Not_Resolve_Anonymous_Or_Disabled_Feature_Requests()
    {
        var fixture = new Fixture();
        var reference = fixture.Reference(fixture.AddTicket());
        fixture.User.IsAuthenticated.Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        fixture.User.IsAuthenticated.Returns(true);
        fixture.Features.IsEnabledAsync(HelpDeskFeatures.Names.Ticketing).Returns(false);
        (await fixture.Resolver.ResolveAsync([reference])).ShouldBeEmpty();
        await fixture.Tickets.DidNotReceive().GetListAsync(
            Arg.Any<Expression<Func<Ticket, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Reject_Wrong_Types_And_Excessive_Batches()
    {
        var fixture = new Fixture();
        await Should.ThrowAsync<ArgumentException>(() => fixture.Resolver.ResolveAsync(
            [new TagEntityReference(HelpDeskRelationEntityTypes.Article, Guid.NewGuid())]));
        var references = Enumerable.Range(0, 101)
            .Select(_ => new TagEntityReference(HelpDeskRelationEntityTypes.Ticket, Guid.NewGuid())).ToList();
        await Should.ThrowAsync<ArgumentException>(() => fixture.Resolver.ResolveAsync(references));
    }

    [Fact]
    public async Task Requester_Permission_Alone_Should_Not_Enable_Relations()
    {
        var fixture = new Fixture();
        fixture.Permissions.IsGrantedAsync(TicketingPermissions.Tickets.Default).Returns(false);
        fixture.Permissions.IsGrantedAsync(TicketingPermissions.Tickets.My).Returns(true);
        (await fixture.Resolver.ResolveAsync([fixture.Reference(fixture.AddTicket())])).ShouldBeEmpty();
        await fixture.Tickets.DidNotReceive().GetListAsync(
            Arg.Any<Expression<Func<Ticket, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    private sealed class Fixture
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public ITicketRepository Tickets { get; } = Substitute.For<ITicketRepository>();
        public IHelpDeskProjectRepository Projects { get; } = Substitute.For<IHelpDeskProjectRepository>();
        public IPermissionChecker Permissions { get; } = Substitute.For<IPermissionChecker>();
        public ICurrentUser User { get; } = Substitute.For<ICurrentUser>();
        public IFeatureChecker Features { get; } = Substitute.For<IFeatureChecker>();
        public List<Ticket> Records { get; } = [];
        public HelpDeskProject Project { get; }
        public TicketRelationEntityResolver Resolver { get; }

        public Fixture()
        {
            var tenant = Substitute.For<ICurrentTenant>();
            tenant.Id.Returns(TenantId);
            User.IsAuthenticated.Returns(true);
            Permissions.IsGrantedAsync(TicketingPermissions.Tickets.Default).Returns(true);
            Features.IsEnabledAsync(HelpDeskFeatures.Names.Ticketing).Returns(true);
            Project = new HelpDeskProject(Guid.NewGuid(), "Support", "support", TenantId);
            Tickets.GetListAsync(Arg.Any<Expression<Func<Ticket, bool>>>(), false, Arg.Any<CancellationToken>())
                .Returns(call => Records.Where(call.Arg<Expression<Func<Ticket, bool>>>().Compile()).ToList());
            Projects.GetListAsync(Arg.Any<Expression<Func<HelpDeskProject, bool>>>(), false, Arg.Any<CancellationToken>())
                .Returns(call => new[] { Project }.Where(call.Arg<Expression<Func<HelpDeskProject, bool>>>().Compile()).ToList());
            Resolver = new TicketRelationEntityResolver(Tickets, Projects, tenant, User, Permissions, Features);
        }

        public Ticket AddTicket(Guid? tenantId = null)
        {
            var article = new Ticket(Guid.NewGuid(), Guid.NewGuid().ToString("N"), "Support request", "Private body",
                TicketPriority.Normal, Guid.NewGuid(), null, tenantId ?? TenantId, Project.Id);
            Records.Add(article);
            return article;
        }

        public TagEntityReference Reference(Ticket article) => new(HelpDeskRelationEntityTypes.Ticket, article.Id);
    }
}
