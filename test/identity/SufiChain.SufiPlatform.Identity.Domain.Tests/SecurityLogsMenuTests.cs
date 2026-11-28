using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class SecurityLogsMenuTests
{
    [Fact]
    public void Menu_SecurityLogs_RequiresItsOwnPermission()
    {
        var root = FindRepoRoot();
        var menu = File.ReadAllText(Path.Combine(
            root,
            "modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Menus/IdentityMenuContributor.cs"));
        var page = File.ReadAllText(Path.Combine(
            root,
            "modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Pages/SecurityLogManagement.razor"));

        menu.ShouldContain("IdentityPermissions.SecurityLogs.Default");
        page.ShouldContain("IdentityPermissions.SecurityLogs.Default");
        IdentityPermissions.SecurityLogs.Default.ShouldNotBe(IdentityPermissions.Users.Default);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "modules/identity")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("identity module root");
    }
}
