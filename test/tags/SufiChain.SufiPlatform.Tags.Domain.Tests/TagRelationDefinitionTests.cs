using Shouldly;
using SufiChain.SufiPlatform.Tags.Relations;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class TagRelationDefinitionTests
{
    [Fact]
    public void Directed_Definition_Should_Reject_Reversed_Types_And_Preserve_Order()
    {
        var definition = Create("helpdesk.article", "helpdesk.ticket");
        var article = new TagEntityReference("helpdesk.article", Guid.NewGuid());
        var ticket = new TagEntityReference("helpdesk.ticket", Guid.NewGuid());
        definition.AcceptsEndpoints(article, ticket).ShouldBeTrue();
        definition.AcceptsEndpoints(ticket, article).ShouldBeFalse();
        definition.Canonicalize(article, ticket).ShouldBe((article, ticket));
        Should.Throw<ArgumentException>(() => definition.Canonicalize(ticket, article));
    }

    [Fact]
    public void Symmetric_Definition_Should_Canonicalize_Reverse_Assertions_And_Reject_Self()
    {
        var definition = Create("helpdesk.article", "helpdesk.article", symmetric: true);
        var first = new TagEntityReference("helpdesk.article", Guid.NewGuid());
        var second = new TagEntityReference("helpdesk.article", Guid.NewGuid());
        definition.Canonicalize(first, second).ShouldBe(definition.Canonicalize(second, first));
        definition.AcceptsEndpoints(first, first).ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false, false, "helpdesk.ticket")]
    [InlineData(true, false, true, "helpdesk.article")]
    [InlineData(false, true, true, "helpdesk.article")]
    [InlineData(false, false, true, "helpdesk.ticket")]
    public void Should_Reject_Inconsistent_Definition(bool symmetric, bool self, bool acyclic, string target)
    {
        Should.Throw<ArgumentException>(() => Create("helpdesk.article", target, symmetric, self, acyclic));
    }

    [Fact]
    public void Host_And_Tenant_Definitions_Should_Have_Distinct_Nonnull_Namespaces()
    {
        var tenant = Guid.NewGuid();
        Create("helpdesk.article", "helpdesk.ticket").TenantScopeKey.ShouldBe("host");
        new TagRelationDefinition(Guid.NewGuid(), tenant, Guid.NewGuid(), "documents-resolution-for", 2,
            "helpdesk.article", "helpdesk.ticket").TenantScopeKey.ShouldBe(tenant.ToString("N"));
    }

    [Theory]
    [InlineData("Invalid Key", 1)]
    [InlineData("valid-key", 0)]
    public void Should_Reject_Invalid_Key_Or_Revision(string key, int revision)
    {
        Should.Throw<ArgumentException>(() => new TagRelationDefinition(Guid.NewGuid(), null, Guid.NewGuid(),
            key, revision, "helpdesk.article", "helpdesk.ticket"));
    }

    private static TagRelationDefinition Create(string source, string target, bool symmetric = false,
        bool self = false, bool acyclic = false) => new(Guid.NewGuid(), null, Guid.NewGuid(), "related-item", 1,
            source, target, symmetric, self, acyclic);
}
