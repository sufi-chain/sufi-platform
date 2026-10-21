using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

/// <summary>
/// Localization proof for sufi-chain#394: the event editor error/retry keys exist in every culture,
/// and the Persian refresh label keeps its ZWNJ join («به\u200Cروزرسانی», not «بروزرسانی»).
/// </summary>
public class CalendarBlazorLocalizationTests
{
    private static readonly string[] Cultures = ["fa", "en", "ar", "es"];

    [Theory]
    [InlineData("EventEditorLoadFailed")]
    [InlineData("EventEditorRetry")]
    public void Event_Editor_Recovery_Keys_Exist_In_Every_Culture(string key)
    {
        foreach (var culture in Cultures)
        {
            LoadTexts(culture).TryGetValue(key, out var value).ShouldBeTrue($"{culture} is missing {key}");
            value.ShouldNotBeNullOrWhiteSpace($"{culture}.{key}");
        }
    }

    [Fact]
    public void Fa_Refresh_Keeps_The_Zwnj_Join()
    {
        var fa = LoadTexts("fa");

        // «به\u200Cروزرسانی» (ZWNJ U+200C); a plain «بروزرسانی» is the UI-27 regression.
        fa["Refresh"].ShouldBe("\u0628\u0647\u200C\u0631\u0648\u0632\u0631\u0633\u0627\u0646\u06CC");
        fa["Refresh"].ShouldContain("\u200C");

        // The all-day label is joined the same way: «تمام\u200Cروز».
        fa["AllDay"].ShouldContain("\u200C");
    }

    private static Dictionary<string, string> LoadTexts(string culture)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(FindFile(
            "modules/calendar/src/SufiChain.SufiPlatform.Calendar.Domain.Shared/Localization/Calendar",
            culture + ".json")));

        var texts = document.RootElement.GetProperty("texts");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in texts.EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static string FindFile(string relativeDirectory, string fileName)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativeDirectory, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(directory.FullName, "sufi-platform", relativeDirectory, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(Path.Combine(relativeDirectory, fileName));
    }
}
