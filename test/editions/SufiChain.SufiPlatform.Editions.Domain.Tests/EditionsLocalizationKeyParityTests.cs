using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Editions;

public class EditionsLocalizationKeyParityTests
{
    [Fact]
    public void Editions_en_fa_ar_es_have_the_same_non_empty_keys()
    {
        var en = LoadTexts("en.json");
        var fa = LoadTexts("fa.json");
        var ar = LoadTexts("ar.json");
        var es = LoadTexts("es.json");

        en.Count.ShouldBeGreaterThan(0);
        fa.Keys.OrderBy(k => k, StringComparer.Ordinal).ShouldBe(en.Keys.OrderBy(k => k, StringComparer.Ordinal));
        ar.Keys.OrderBy(k => k, StringComparer.Ordinal).ShouldBe(en.Keys.OrderBy(k => k, StringComparer.Ordinal));
        es.Keys.OrderBy(k => k, StringComparer.Ordinal).ShouldBe(en.Keys.OrderBy(k => k, StringComparer.Ordinal));

        foreach (var key in en.Keys)
        {
            en[key].ShouldNotBeNullOrWhiteSpace(key);
            fa[key].ShouldNotBeNullOrWhiteSpace(key);
            ar[key].ShouldNotBeNullOrWhiteSpace(key);
            es[key].ShouldNotBeNullOrWhiteSpace(key);
        }
    }

    private static Dictionary<string, string> LoadTexts(string fileName)
    {
        var path = FindWorkspaceFile(
            $"sufi-platform/modules/editions/src/SufiChain.SufiPlatform.Editions.Domain.Shared/Localization/Editions/{fileName}");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var texts = document.RootElement.GetProperty("texts");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in texts.EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static string FindWorkspaceFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            // Tests under sufi-platform/test/... see repo root as parent of sufi-platform
            candidate = Path.Combine(dir.FullName, relativePath.Replace("sufi-platform/", "", StringComparison.Ordinal));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
