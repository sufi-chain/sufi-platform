using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare;

public class CalendarAssistantPublicNameTests
{
    private static readonly string[] Banned =
    [
        "Copilot",
        "Copiloto",
        "تقویم همراه",
        "کمک‌یار تقویم"
    ];

    [Fact]
    public void Public_Names_Replace_The_Old_Calendar_Copilot_Wording()
    {
        var seed = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Assistant/Data/CalendarAssistantHooshvareSeedTexts.cs");
        var guide = Read("hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Domain/Data/SufiPlatformKnowledgeBaseGuideDataSeedContributor.cs");

        seed.ShouldContain("هوشوارهٔ تقویم");
        seed.ShouldContain("You are the Calendar hooshvare");
        foreach (var phrase in Banned)
        {
            seed.ShouldNotContain(phrase);
            guide.ShouldNotContain(phrase);
        }

        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var texts = LoadTexts(culture);
            texts.ShouldContainKey("CalendarHooshvare:Unauthorized");
            texts.ShouldContainKey("CalendarHooshvare:LoadFailed");
            foreach (var value in texts.Values)
            {
                foreach (var phrase in Banned)
                {
                    value.ShouldNotContain(phrase);
                }
            }
        }

        LoadTexts("fa")["CalendarHooshvare:Title"].ShouldBe("هوشوارهٔ تقویم");
    }

    private static Dictionary<string, string> LoadTexts(string culture)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(ReadPath(
            "modules/calendar/src/SufiChain.SufiPlatform.Calendar.Domain.Shared/Localization/Calendar/" + culture + ".json")));
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.GetProperty("texts").EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static string Read(string relative) => File.ReadAllText(ReadPath(relative));

    private static string ReadPath(string relative)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            foreach (var root in new[] { directory.FullName, Path.Combine(directory.FullName, "sufi-platform") })
            {
                var candidate = Path.Combine(root, relative);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new FileNotFoundException(relative);
    }
}
