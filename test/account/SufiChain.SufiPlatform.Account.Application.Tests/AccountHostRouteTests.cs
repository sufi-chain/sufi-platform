using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class AccountHostRouteTests
{
    [Fact]
    public void Every_module_account_page_has_a_host_counterpart()
    {
        var root = FindRepoRoot();
        var module = Path.Combine(root, "sufi-platform", "modules", "account", "src", "SufiChain.SufiPlatform.Account.Blazor", "Pages");
        var host = Path.Combine(root, "hosts", "SufiChane.SufiPlatform", "src", "SufiChane.SufiPlatform.Account.Blazor");
        var hostMarkup = string.Join('\n', Directory.GetFiles(host, "*.razor", SearchOption.AllDirectories).Select(File.ReadAllText));
        var moduleRoutes = Directory.GetFiles(module, "*.razor")
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), "@page\\s+\"([^\"]+)\"").Select(match => match.Groups[1].Value))
            .ToList();

        moduleRoutes.ShouldContain("/account/link-user");
        foreach (var route in moduleRoutes)
        {
            hostMarkup.ShouldContain($"@page \"{route}\"");
        }

        var linkPage = File.ReadAllText(Path.Combine(host, "Pages", "Account", "CreateLinkUser.razor"));
        linkPage.ShouldContain("@inherits SufiChain.SufiPlatform.Account.Blazor.Pages.CreateLinkUser");
        File.ReadAllText(Path.Combine(module, "CreateLinkUser.razor.cs")).ShouldContain("InvalidLinkUserToken");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "sufi-platform"))
                && Directory.Exists(Path.Combine(dir.FullName, "hosts")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("sufi-chain root was not found from the test output directory.");
    }
}
