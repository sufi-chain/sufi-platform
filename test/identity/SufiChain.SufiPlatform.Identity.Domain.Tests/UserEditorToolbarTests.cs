using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity.Domain.Tests;

public class UserEditorToolbarTests
{
    [Fact]
    public void UserEditor_RegistersSaveAndCancelInPageToolbar()
    {
        var source = Read("modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Components/UserEditor.razor");
        var code = Read("modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Components/UserEditor.razor.cs");
        source.ShouldContain("<SufiPageToolbar>");
        source.ShouldContain("SaveFromToolbarAsync");
        source.ShouldContain("@L[\"Cancel\"]");
        source.ShouldContain("Disabled=\"@(!_ready)\"");
        code.ShouldContain("EditContext");
        code.ShouldContain("SaveAsync()");
    }

    [Fact]
    public void UserEditor_Mobile_ToolbarActionsReachable()
    {
        var source = Read("modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Components/UserEditor.razor");
        source.ShouldContain("<FooterContent>");
        source.ShouldContain("<SufiPageToolbar>");
    }

    private static string Read(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, relative)))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return File.ReadAllText(Path.Combine(dir!.FullName, relative));
    }
}
