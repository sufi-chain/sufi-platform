using System;
using System.IO;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.WebResearch;

/// <summary>
/// sufi-chain#551: workspace chat exposes Stop and stores a long reply in body-sized parts.
/// The turn has no hard-coded duration cutoff.
/// </summary>
public class WorkspaceChatStopMarkupTests
{
    [Fact]
    public void Stop_cancels_the_turn_and_long_replies_are_split()
    {
        var page = Read("WorkspaceChat.razor");
        var code = Read("WorkspaceChat.razor.cs");

        Assert.Contains("OnStop=\"StopTurnAsync\"", page);
        Assert.Contains("StopTurnAsync", code);
        Assert.Contains("ChatMessageBodySplitter", code);
        Assert.DoesNotContain("TimeSpan.FromSeconds", code);
        Assert.DoesNotContain("ChatReplyTimeout", code);
    }

    private static string Read(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(
                dir.FullName,
                "modules",
                "ai",
                "src",
                "SufiChain.SufiPlatform.SufiAI.Blazor",
                "Pages",
                "AI",
                fileName);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(fileName + " not found above " + AppContext.BaseDirectory);
    }
}
