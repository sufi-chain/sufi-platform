using System;
using System.IO;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.WebResearch;

/// <summary>
/// sufi-chain#340 (G-162): the web-research settings page must never fall back to a native form
/// POST (HTTP 400 from Blazor). Guards the markup of WebResearchSettings.razor.
/// </summary>
public class WebResearchSettingsMarkupTests
{
    private static string ReadPage()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(dir.FullName, "modules", "ai", "src", "SufiChain.SufiPlatform.SufiAI.Blazor", "Pages", "AI", "WebResearchSettings.razor");
            if (File.Exists(path)) return File.ReadAllText(path);
            dir = dir.Parent;
        }

        throw new FileNotFoundException("WebResearchSettings.razor not found above " + AppContext.BaseDirectory);
    }

    [Fact]
    public void Save_is_not_a_native_submit_button()
    {
        var page = ReadPage();
        Assert.DoesNotContain("Type=\"submit\"", page);
        Assert.DoesNotContain("form=\"web-research-settings-form\"", page);
        Assert.DoesNotContain("OnValidSubmit", page);
    }

    [Fact]
    public void Form_and_actions_wait_for_an_interactive_circuit()
    {
        var page = ReadPage();
        Assert.Contains("!_canEdit || !RendererInfo.IsInteractive", page);
        Assert.Contains("Disabled=\"@(_busy || !RendererInfo.IsInteractive)\" aria-describedby=\"web-research-test-help\"", page);
    }
}