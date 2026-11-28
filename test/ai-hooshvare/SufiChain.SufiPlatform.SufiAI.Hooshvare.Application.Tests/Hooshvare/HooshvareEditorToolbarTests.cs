using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareEditorToolbarTests
{
    [Fact]
    public void HooshvareEditor_RegistersSaveAndLeaveInPageToolbar()
    {
        var source = Read("pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Pages/AI/Hooshvare/HooshvareEditorPage.razor");
        source.ShouldContain("<SufiPageToolbar>");
        source.ShouldContain("OnClick=\"@SaveAsync\"");
        source.ShouldContain("Hooshvare:Leave");
        source.ShouldContain("Disabled=\"@(!_formReady || _loading)\"");
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
