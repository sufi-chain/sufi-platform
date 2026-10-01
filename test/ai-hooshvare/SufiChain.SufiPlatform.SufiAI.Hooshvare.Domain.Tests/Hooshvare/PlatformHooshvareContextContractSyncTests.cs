using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class PlatformHooshvareContextContractSyncTests
{
    private static readonly Regex KeyPattern = new("`([A-Za-z][A-Za-z0-9]*)`", RegexOptions.Compiled);

    [Fact]
    public void Context_Contract_Doc_Keys_Should_Match_PlatformHooshvareContextFields()
    {
        var document = File.ReadAllText(FindContract());
        var rows = document
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("| `", StringComparison.Ordinal))
            .Select(line => line.Split('|'))
            .Where(cells => cells.Length > 5)
            .ToDictionary(
                cells => cells[1].Trim().Trim('`'),
                cells => KeyPattern.Matches(cells[4]).Select(match => match.Groups[1].Value).ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);

        foreach (var (hooshvareKey, fields) in PlatformHooshvareContextFields.ByHooshvareKey)
        {
            rows.ShouldContainKey(hooshvareKey);
            rows[hooshvareKey].OrderBy(key => key).ShouldBe(
                fields.Select(field => field.Key).OrderBy(key => key).ToList());
        }
    }

    private static string FindContract()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "documents",
                "developer-wiki",
                "SufiPlatform",
                "Platform Configuration",
                "Hooshvare Context Contracts.md");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Hooshvare Context Contracts.md was not found above the test output directory.");
    }
}
