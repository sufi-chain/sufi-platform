using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class TextToolCallMarkupTests
{
    [Fact]
    public void Parseable_markup_becomes_a_real_tool_call()
    {
        var text = """
            Checking prices.
            <dots_function_call>
            <invoke name="mcp__iranmcp__get_latest_news">
            <parameter name="query">tehran</parameter>
            </invoke>
            </dots_function_call>
            """;

        var result = TextToolCallMarkup.Process(text);
        result.HadMarkup.ShouldBeTrue();
        result.RequiresNotice.ShouldBeFalse();
        result.Calls.Count.ShouldBe(1);
        result.Calls[0].Name.ShouldBe("get_latest_news");
        result.Calls[0].Arguments["query"].ShouldBe("tehran");
        result.CleanText.ShouldContain("Checking prices.");
        result.CleanText.ShouldNotContain("dots_function_call");
    }

    [Fact]
    public void Unparseable_markup_is_stripped_with_notice()
    {
        var text = """
            Here you go
            <dots_function_call>
            <<<broken
            </dots_function_call>
            """;

        var result = TextToolCallMarkup.Process(text);
        result.HadMarkup.ShouldBeTrue();
        result.RequiresNotice.ShouldBeTrue();
        result.Calls.ShouldBeEmpty();
        result.CleanText.ShouldBe("Here you go");
    }

    [Fact]
    public void Routing_model_markup_is_processed_the_same_way()
    {
        var routing = """
            <function_call>
            <invoke name="external.tgju-mcp.tj_get_prices">
            <parameter name="q">gold</parameter>
            </invoke>
            </function_call>
            """;

        var result = TextToolCallMarkup.Process(routing);
        result.HadMarkup.ShouldBeTrue();
        result.RequiresNotice.ShouldBeFalse();
        result.Calls.Single().Name.ShouldBe("external.tgju-mcp.tj_get_prices");
        result.Calls.Single().Arguments["q"].ShouldBe("gold");
        result.CleanText.ShouldBeEmpty();
    }
}
