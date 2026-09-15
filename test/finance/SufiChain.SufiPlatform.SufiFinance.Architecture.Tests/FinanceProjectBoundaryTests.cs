using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Architecture;

public class FinanceProjectBoundaryTests
{
    private static readonly string FinanceRoot = FindFinanceRoot();

    private static string FindFinanceRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "pro-modules", "finance", "src");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("The pro-modules/finance/src directory was not found from the test output directory.");
    }

    [Fact]
    public void Submodules_Should_Not_Reference_Other_Submodule_Implementations()
    {
        var submodules = new[] { "Payments", "Wallets", "Invoicing", "ExchangeRates" };
        var violations = new List<string>();

        foreach (var projectFile in Directory.EnumerateFiles(FinanceRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var owner = submodules.FirstOrDefault(submodule =>
                projectFile.Contains($".SufiFinance.{submodule}.", StringComparison.Ordinal));
            if (owner is null)
            {
                continue;
            }

            var projectText = File.ReadAllText(projectFile);
            foreach (var other in submodules.Where(submodule => submodule != owner))
            {
                if (projectText.Contains($".SufiFinance.{other}.", StringComparison.Ordinal))
                {
                    if (projectText.Contains($".SufiFinance.{other}.Domain.Shared", StringComparison.Ordinal)
                        || projectText.Contains($".SufiFinance.{other}.Application.Contracts", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    violations.Add($"{Path.GetFileName(projectFile)} references {other}");
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Payment_Providers_Should_Reference_Payments_Domain_Only()
    {
        var providerProjects = Directory
            .EnumerateFiles(FinanceRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => path.Contains(".SufiFinance.Payments.", StringComparison.Ordinal))
            .Where(path => !path.Contains(".Payments.Domain", StringComparison.Ordinal)
                && !path.Contains(".Payments.Application", StringComparison.Ordinal)
                && !path.Contains(".Payments.HttpApi", StringComparison.Ordinal)
                && !path.Contains(".Payments.Blazor", StringComparison.Ordinal)
                && !path.Contains(".Payments.EntityFrameworkCore", StringComparison.Ordinal)
                && !path.Contains(".Payments.MongoDB", StringComparison.Ordinal));

        var violations = new List<string>();
        foreach (var projectFile in providerProjects)
        {
            var references = File.ReadAllLines(projectFile)
                .Where(line => line.Contains("ProjectReference", StringComparison.Ordinal))
                .ToArray();

            if (references.Length != 1 || !references[0].Contains("Payments.Domain", StringComparison.Ordinal))
            {
                violations.Add(Path.GetFileName(projectFile));
            }
        }

        violations.ShouldBeEmpty();
    }

    [Fact]
    public void Invoicing_Blazor_Should_Not_Reference_Payments_Blazor()
    {
        var projectFile = Directory.EnumerateFiles(
                FinanceRoot,
                "SufiChain.SufiPlatform.SufiFinance.Invoicing.Blazor.csproj",
                SearchOption.AllDirectories)
            .Single();

        File.ReadAllText(projectFile)
            .ShouldNotContain(".SufiFinance.Payments.Blazor");
    }
}
