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

    [Fact]
    public void Zwnj_and_plain_forms_match_either_way_and_empty_does_not()
    {
        LocalizationTextValueFilter.Contains("گفت‌وگو", "گفتگو").ShouldBeTrue();
        LocalizationTextValueFilter.Contains("گفتگو", "گفت‌وگو").ShouldBeTrue();
        LocalizationTextValueFilter.Contains("Hello", "hello").ShouldBeTrue();
        LocalizationTextValueFilter.Contains(null, "گفتگو").ShouldBeFalse();
        LocalizationTextValueFilter.Contains("گفت‌وگو", "").ShouldBeFalse();
    }

    [Fact]
    public void Search_matches_a_Persian_value_and_still_matches_the_key()
    {
        var row = new LocalizationTextWithBaseValueDto
        {
            Key = "Ai:Conversation",
            Value = "گفت‌وگو",
            BaseValue = "Conversation"
        };

        LocalizationTextValueFilter.MatchesSearch(row, "گفتگو").ShouldBeTrue();
        LocalizationTextValueFilter.MatchesSearch(row, "conversation").ShouldBeTrue();
        LocalizationTextValueFilter.MatchesSearch(row, "missing").ShouldBeFalse();
    }

    private static LocalizationTextWithBaseValueDto Row(string key, string value) =>
        new() { Key = key, Value = value };
}
