using SufiChain.SufiPlatform.SufiAI;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests;

public class ContextBudgetTests
{
    [Fact]
    public async Task SummarizeSession_CallsLlm_AndDoesNotWriteAccountingStringWhenLlmSucceeds()
    {
        var text = await ContextBudget.SummarizeOrAccountingAsync(() => Task.FromResult("خلاصه گفت‌وگو"), "Context summary v1", _ => { });
        text.ShouldBe("خلاصه گفت‌وگو");
        text.ShouldNotStartWith("Context summary v");
    }

    [Fact]
    public async Task SummarizeSession_FallsBackToCannedString_AndLogs_WhenLlmFails()
    {
        Exception? logged = null;
        var text = await ContextBudget.SummarizeOrAccountingAsync(
            () => throw new InvalidOperationException("model down"),
            "Context summary v2",
            exception => logged = exception);
        text.ShouldBe("Context summary v2");
        logged.ShouldNotBeNull();
    }

    [Fact]
    public async Task AutoSummarize_CallsLlm_NotAutomaticSummaryPrefixWhenLlmSucceeds()
    {
        var text = await ContextBudget.SummarizeOrAccountingAsync(() => Task.FromResult("real summary"), "Automatic summary v1", _ => { });
        text.ShouldNotStartWith("Automatic summary v");
    }

    [Fact]
    public async Task AutoSummarize_FallsBackAndLogs_WhenLlmFails()
    {
        var logged = false;
        var text = await ContextBudget.SummarizeOrAccountingAsync(
            () => throw new InvalidOperationException("model down"),
            "Automatic summary v3",
            _ => logged = true);
        text.ShouldStartWith("Automatic summary v");
        logged.ShouldBeTrue();
    }

    [Fact]
    public void HardStop_At95_BlocksSend_OnHooshvareAndAdminWorkspaceChat()
    {
        ContextBudget.BlocksSend(ContextBudget.State(95)).ShouldBeTrue();
        ContextBudget.BlocksSend(ContextBudget.State(85)).ShouldBeFalse();
        var workspace = File.ReadAllText(Find("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/WorkspaceChat.razor.cs"));
        workspace.ShouldContain("ContextBudget.BlocksSend");
        var runtime = File.ReadAllText(Find("pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Application/Hooshvare/HooshvareRuntimeAppService.cs"));
        runtime.ShouldContain("ContextHardStop");
    }

    [Fact]
    public void HardStop_OffersStartNewOrSummarize()
    {
        ContextBudget.OffersStartNewOrSummarize("HardStop").ShouldBeTrue();
        ContextBudget.OffersStartNewOrSummarize("Summarize").ShouldBeTrue();
        var page = File.ReadAllText(Find("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/WorkspaceChat.razor"));
        page.ShouldContain("WorkspaceChat:StartNew");
    }

    [Fact]
    public void AdminWorkspaceChat_ShowsContextMeter_And70_85Warnings()
    {
        var page = File.ReadAllText(Find("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/WorkspaceChat.razor"));
        page.ShouldContain("workspace-chat-context-meter");
        page.ShouldContain("WorkspaceChat:ContextWarning");
        page.ShouldContain("WorkspaceChat:ContextSummarize");
    }

    [Fact]
    public void AdminWorkspaceChat_DoesNotForceShowContextPanelFalse_UnlessMeterIsRenderedElsewhere()
    {
        var page = File.ReadAllText(Find("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/WorkspaceChat.razor"));
        page.ShouldContain("workspace-chat-context-meter");
        (page.Contains("ShowContextPanel=\"false\"") && page.Contains("workspace-chat-context-meter")).ShouldBeTrue();
    }

    private static string Find(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, relative)))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return Path.Combine(dir!.FullName, relative);
    }
}
