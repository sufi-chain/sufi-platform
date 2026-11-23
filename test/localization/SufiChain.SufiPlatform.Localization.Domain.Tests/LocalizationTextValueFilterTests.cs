using Shouldly;
using SufiChain.SufiPlatform.Localization.Dtos;
using Xunit;

namespace SufiChain.SufiPlatform.Localization;

public class LocalizationTextValueFilterTests
{
    [Fact]
    public void Value_contains_filter_returns_only_matching_rows()
    {
        var rows = new[]
        {
            Row("Greeting", "سلام"),
            Row("Prompt", "گفت‌وگو"),
            Row("Other", "سایر")
        };

        var matches = rows.Where(row => LocalizationTextValueFilter.Matches(row, "گفت‌وگو")).ToList();

        matches.Count.ShouldBe(1);
        matches[0].Key.ShouldBe("Prompt");
    }

    [Fact]
    public void Value_filter_also_matches_the_base_text()
    {
        var row = new LocalizationTextWithBaseValueDto
        {
            Key = "Title",
            Value = string.Empty,
            BaseValue = "گفت‌وگو"
        };

        LocalizationTextValueFilter.Matches(row, "گفت").ShouldBeTrue();
        LocalizationTextValueFilter.Matches(row, "پرامپت").ShouldBeFalse();
    }

    private static LocalizationTextWithBaseValueDto Row(string key, string value) =>
        new() { Key = key, Value = value };
}
