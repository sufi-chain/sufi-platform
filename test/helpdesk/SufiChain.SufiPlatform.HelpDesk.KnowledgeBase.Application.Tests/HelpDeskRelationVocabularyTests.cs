using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Relations;
using SufiChain.SufiPlatform.HelpDesk.Relations;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class HelpDeskRelationVocabularyTests
{
    [Fact]
    public void Should_Declare_The_Four_Approved_Predicates_Without_Changing_Business_State()
    {
        var entries = new HelpDeskRelationVocabularyContributor().GetEntries();
        entries.Select(x => x.StableKey).ShouldBe(new[] { "documents-resolution-for", "related-article", "supersedes-article", "duplicate-ticket" });
        entries.Single(x => x.StableKey == "related-article").IsSymmetric.ShouldBeTrue();
        entries.Single(x => x.StableKey == "supersedes-article").RequiresAcyclicGraph.ShouldBeTrue();
        entries.Single(x => x.StableKey == "documents-resolution-for").TargetType.ShouldBe(HelpDeskRelationEntityTypes.Ticket);
    }

    [Fact]
    public void Should_Expose_Owning_Module_Adapters_Under_Tags_Contracts()
    {
        var services = new ServiceCollection();
        services.AddAssemblyOf<HelpDeskRelationVocabularyContributor>();
        services.AddAssemblyOf<KBArticleRelationEntityResolver>();
        services.ShouldContain(x => x.ServiceType == typeof(ITagRelationVocabularyContributor) &&
            x.ImplementationType == typeof(HelpDeskRelationVocabularyContributor));
        services.ShouldContain(x => x.ServiceType == typeof(ITagRelationEntityResolver) &&
            x.ImplementationType == typeof(KBArticleRelationEntityResolver));
        services.ShouldContain(x => x.ServiceType == typeof(IKnowledgeProposalEvidenceValidator) &&
            x.ImplementationType == typeof(KBKnowledgeProposalEvidenceValidator));
    }
}
