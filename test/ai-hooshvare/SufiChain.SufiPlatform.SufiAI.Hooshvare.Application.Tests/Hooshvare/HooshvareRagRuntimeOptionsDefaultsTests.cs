using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareRagRuntimeOptionsDefaultsTests

{

    [Fact]

    public void SearchMinSimilarity_Should_Be_Question_To_Passage_Floor()

    {

        HooshvareRagRuntimeOptionsDefaults.SearchMinSimilarity.ShouldBe(0.35f);

        HooshvareRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity.ShouldBe(0.1f);

        HooshvareRagRuntimeOptionsDefaults.ArticleExpandMaxResults.ShouldBe(8);

    }



    [Fact]

    public void Should_Persist_KnowledgeBase_Source_And_Project_Filter_When_Rag_Enabled_With_Projects()

    {

        var options = new HooshvareRuntimeOptions { UseRag = true };



        var result = HooshvareRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);



        result.RagSourceName.ShouldBe("KnowledgeBase");

        result.RagFilterByProjectId.ShouldBeTrue();

    }



    [Fact]

    public void Should_Leave_Options_Untouched_When_Rag_Disabled()

    {

        var options = new HooshvareRuntimeOptions { UseRag = false };



        var result = HooshvareRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);



        result.RagSourceName.ShouldBeNull();

        result.RagFilterByProjectId.ShouldBeFalse();

    }



    [Fact]

    public void Should_Not_Force_Project_Filter_Without_Bound_Projects()

    {

        var options = new HooshvareRuntimeOptions { UseRag = true };



        var result = HooshvareRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, []);



        result.RagSourceName.ShouldBeNull();

        result.RagFilterByProjectId.ShouldBeFalse();

    }



    [Fact]

    public void Should_Keep_Explicit_Non_KnowledgeBase_Source()

    {

        var options = new HooshvareRuntimeOptions { UseRag = true, RagSourceName = "Documents" };



        var result = HooshvareRagRuntimeOptionsDefaults.ApplyProjectRagDefaults(options, [Guid.NewGuid()]);



        result.RagSourceName.ShouldBe("Documents");

        result.RagFilterByProjectId.ShouldBeFalse();

    }

}

