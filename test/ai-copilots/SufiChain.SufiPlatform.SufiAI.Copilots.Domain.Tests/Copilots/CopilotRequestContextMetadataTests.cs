using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRequestContextMetadataTests
{
    [Theory]
    [InlineData("آزمایش فارسی")]
    [InlineData("مرحبا بالعالم")]
    public void Should_Preserve_Unicode_In_Built_And_Merged_Metadata(string text)
    {
        var context = new Dictionary<string, string> { ["title"] = text };
        var built = CopilotRequestContextMetadata.BuildJson(context);
        var merged = CopilotRequestContextMetadata.Merge("{\"kind\":\"voice\"}", context);
        var combined = CopilotRequestContextMetadata.MergeMetadata(merged, built)!;

        foreach (var metadata in new[] { built, merged, combined })
        {
            metadata.ShouldContain(text);
            metadata.ShouldNotContain("\\u");
            CopilotRequestContextMetadata.TryParseContext(metadata)["title"].ShouldBe(text);
        }

        using var document = JsonDocument.Parse(combined);
        document.RootElement.GetProperty("kind").GetString().ShouldBe("voice");
    }

    [Fact]
    public void Should_Roundtrip_Json_Quotes_Backslashes_And_Newlines()
    {
        const string text = "آزمایش \"quoted\" \\ path\n<text>";
        var metadata = CopilotRequestContextMetadata.BuildJson(
            new Dictionary<string, string> { ["title"] = text });

        CopilotRequestContextMetadata.TryParseContext(metadata)["title"].ShouldBe(text);
    }

    [Fact]
    public void Should_Read_Legacy_Unicode_Escapes()
    {
        CopilotRequestContextMetadata.TryParseContext(
            "{\"copilotContext\":{\"title\":\"\\u0622\"}}")
            ["title"].ShouldBe("آ");
    }
}
