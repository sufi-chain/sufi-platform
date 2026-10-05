using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public class ProApplicationLicenseProductTests
{
    private const string IssuerAssemblyName = "SufiChain.SufiPlatform.SufiSaas.Application";

    private static readonly Dictionary<string, string> RequiredProductMembers = new(StringComparer.Ordinal)
    {
        ["SufiChain.SufiPlatform.SufiFinance.Application"] = "SufiFinance",
        ["SufiChain.SufiPlatform.SufiFinance.Invoicing.Application"] = "SufiFinance",
        ["SufiChain.SufiPlatform.SufiFinance.Payments.Application"] = "SufiFinance",
        ["SufiChain.SufiPlatform.SufiFinance.Wallets.Application"] = "SufiFinance",
        ["SufiChain.SufiPlatform.SufiFinance.ExchangeRates.Application"] = "SufiFinance",
        ["SufiChain.SufiPlatform.SufiCRM.Application"] = "SufiCRM",
        ["SufiChain.SufiPlatform.SufiCRM.Contacts.Application"] = "SufiCRM"
    };

    [Fact]
    public void Every_pro_application_assembly_declares_LicenseProduct()
    {
        var root = FindProModulesRoot();
        var productMembers = ReadProductMembers(Path.Combine(
            root,
            "licensing",
            "src",
            "SufiChain.SufiPlatform.Licensing.Abstractions",
            "LicenseProducts.cs"));
        var projects = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .Where(path => !IsBuildOutput(path) && IsProApplicationProject(path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

        projects.ShouldNotBeEmpty();
        projects.ShouldContain(path => Path.GetFileNameWithoutExtension(path) == IssuerAssemblyName);

        var failures = new List<string>();
        foreach (var project in projects)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(project);
            if (assemblyName == IssuerAssemblyName)
            {
                continue;
            }

            var attributeMembers = ReadLicenseProductMembers(Path.GetDirectoryName(project)!);
            if (attributeMembers.Count == 0)
            {
                failures.Add($"{assemblyName} has no [assembly: LicenseProduct]");
                continue;
            }

            foreach (var member in attributeMembers)
            {
                if (!productMembers.Contains(member))
                {
                    failures.Add($"{assemblyName} uses LicenseProducts.{member}, which is not a product code");
                }
            }

            if (RequiredProductMembers.TryGetValue(assemblyName, out var required) &&
                !attributeMembers.Contains(required))
            {
                failures.Add($"{assemblyName} must declare LicenseProducts.{required}");
            }
        }

        failures.ShouldBeEmpty();
    }

    private static bool IsBuildOutput(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProApplicationProject(string projectPath)
    {
        var name = Path.GetFileNameWithoutExtension(projectPath);
        return name.EndsWith(".Application", StringComparison.Ordinal);
    }

    private static List<string> ReadLicenseProductMembers(string projectDirectory)
    {
        var members = new List<string>();
        foreach (var source in Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                     .Where(path => !IsBuildOutput(path)))
        {
            foreach (Match match in LicenseProductPattern.Matches(File.ReadAllText(source)))
            {
                members.Add(match.Groups[1].Value);
            }
        }

        return members;
    }

    private static HashSet<string> ReadProductMembers(string licenseProductsPath)
    {
        var members = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in ProductMemberPattern.Matches(File.ReadAllText(licenseProductsPath)))
        {
            members.Add(match.Groups[1].Value);
        }

        members.ShouldNotBeEmpty();
        return members;
    }

    private static string FindProModulesRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "pro-modules");
            if (Directory.Exists(Path.Combine(candidate, "licensing")) &&
                Directory.Exists(Path.Combine(candidate, "finance")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("The pro-modules directory was not found from the test output directory.");
    }

    private static readonly Regex LicenseProductPattern = new(
        @"\[\s*assembly\s*:\s*LicenseProduct\s*\(\s*LicenseProducts\s*\.\s*(?<member>[A-Za-z0-9_]+)",
        RegexOptions.CultureInvariant);

    private static readonly Regex ProductMemberPattern = new(
        @"public\s+const\s+string\s+(?<member>[A-Za-z0-9_]+)\s*=",
        RegexOptions.CultureInvariant);
}
