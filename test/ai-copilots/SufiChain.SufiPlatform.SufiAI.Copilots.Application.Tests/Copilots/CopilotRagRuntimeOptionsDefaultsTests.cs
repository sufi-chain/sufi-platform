using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRagRuntimeOptionsDefaultsTests
{
    [Fact]
    public void SearchMinSimilarity_Should_Be_Question_To_Passage_Floor()
    {
        CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity.ShouldBe(0.35f);
        CopilotRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity.ShouldBe(0.1f);
        CopilotRagRuntimeOptionsDefaults.ArticleExpandMaxResults.ShouldBe(8);
    }

    [Fact]
    public void Should_Persist_KnowledgeBase_Source_And_Project_Filter_When_Rag_Enabled_With_Projects()
    {
        var options = new CopilotRuntimeOptions { UseRag = true };

        var result = CopilotRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);

        result.RagSourceName.ShouldBe("KnowledgeBase");
        result.RagFilterByProjectId.ShouldBeTrue();
    }

    [Fact]
    public void Should_Leave_Options_Untouched_When_Rag_Disabled()
    {
        var options = new CopilotRuntimeOptions { UseRag = false };

        var result = CopilotRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);

        result.RagSourceName.ShouldBeNull();
        result.RagFilterByProjectId.ShouldBeFalse();
    }

    [Fact]
    public void Should_Not_Force_Project_Filter_Without_Bound_Projects()
    {
        var options = new CopilotRuntimeOptions { UseRag = true };

        var result = CopilotRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, []);

        result.RagSourceName.ShouldBeNull();
        result.RagFilterByProjectId.ShouldBeFalse();
    }

    [Fact]
    public void Should_Keep_Explicit_Non_KnowledgeBase_Source()
    {
        var options = new CopilotRuntimeOptions { UseRag = true, RagSourceName = "Documents" };

        var result = CopilotRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);

        result.RagSourceName.ShouldBe("Documents");
        result.RagFilterByProjectId.ShouldBeFalse();
    }
}
