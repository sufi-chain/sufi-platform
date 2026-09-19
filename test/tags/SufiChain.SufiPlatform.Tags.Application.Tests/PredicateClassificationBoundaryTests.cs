using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Tags.Caching;
using SufiChain.SufiPlatform.Tags.Settings;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Caching;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class PredicateClassificationBoundaryTests
{
    [Theory]
    [InlineData("read")]
    [InlineData("update")]
    [InlineData("delete")]
    [InlineData("assign")]
    [InlineData("links")]
    public async Task Classification_APIs_Should_Reject_Predicate_ID_Without_Mutation(string operation)
    {
        var predicate = Tag.CreateRelationPredicate(Guid.NewGuid(), "Related", "knowledge", "related-article");
        var tags = Substitute.For<ITagRepository>();
        tags.GetAsync(predicate.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(predicate);
        tags.FindAsync(predicate.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(predicate);
        var links = Substitute.For<ITagLinkRepository>();
        var policy = Substitute.For<ITagsPolicyProvider>();
        policy.GetAsync(Arg.Any<CancellationToken>()).Returns(new TagsPolicyDto());
        var service = new TagAppService(tags, new TagManager(tags), policy, Substitute.For<IDistributedCache<TagCacheItem>>());
        var linkService = new TagLinkAppService(tags, links, policy, Substitute.For<IDistributedCache<TagLinkCacheItem>>());

        Task Execute() => operation switch
        {
            "read" => service.GetAsync(predicate.Id),
            "update" => service.UpdateAsync(predicate.Id, new UpdateTagDto { Name = "Changed", Scope = "elsewhere" }),
            "delete" => service.DeleteAsync(predicate.Id),
            "assign" => linkService.AssignAsync(new AssignTagDto { TagId = predicate.Id, EntityType = "helpdesk.article", EntityId = Guid.NewGuid() }),
            "links" => linkService.GetLinksByTagAsync(predicate.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        (await Should.ThrowAsync<BusinessException>(Execute)).Code.ShouldBe(TagsErrorCodes.PredicateRequiresRelationWorkflow);
        predicate.Name.ShouldBe("Related");
        predicate.Scope.ShouldBe("knowledge");
        await tags.DidNotReceive().UpdateAsync(Arg.Any<Tag>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await tags.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await links.DidNotReceive().InsertAsync(Arg.Any<TagLink>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
