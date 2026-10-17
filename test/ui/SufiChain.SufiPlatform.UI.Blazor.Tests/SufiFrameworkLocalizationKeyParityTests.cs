using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SufiFrameworkLocalizationKeyParityTests
{
    [Fact]
    public void SufiFramework_fa_en_ar_es_share_the_new_exception_keys()
    {
        var en = LoadTexts("en.json");
        var fa = LoadTexts("fa.json");
        var ar = LoadTexts("ar.json");
        var es = LoadTexts("es.json");

        foreach (var key in new[] { "EntityNotFound", "DefaultErrorMessageWithReference", "Unauthorized", "AnErrorOccurred", "PleaseCorrectErrors", "ValidationError" })
        {
            en.ContainsKey(key).ShouldBeTrue(key);
            fa.ContainsKey(key).ShouldBeTrue(key);
            ar.ContainsKey(key).ShouldBeTrue(key);
            es.ContainsKey(key).ShouldBeTrue(key);

            en[key].ShouldNotBeNullOrWhiteSpace(key);
            fa[key].ShouldNotBeNullOrWhiteSpace(key);
            ar[key].ShouldNotBeNullOrWhiteSpace(key);
            es[key].ShouldNotBeNullOrWhiteSpace(key);
        }

        // fa/en stay in full parity for the framework resource (existing convention for this pair).
        fa.Keys.OrderBy(k => k, StringComparer.Ordinal)
            .ShouldBe(en.Keys.OrderBy(k => k, StringComparer.Ordinal));
    }

    private static Dictionary<string, string> LoadTexts(string fileName)
    {
        var path = FindFile(
            Path.Combine(
                "framework",
                "SufiChain.SufiPlatform.UI.Domain.Shared",
                "Localization",
                "SufiFramework",
                fileName));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var texts = document.RootElement.GetProperty("texts");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in texts.EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static string FindFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(dir.FullName, "sufi-platform", relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
