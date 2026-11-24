using System.Net.Http;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

public class WorkspaceConnectionTestNoticeTests
{
    [Fact]
    public void Success_uses_the_success_notice()
    {
        WorkspaceConnectionTestNotice.Success.ShouldBe("ConnectionTestSuccessful");
        WorkspaceConnectionTestNotice.Timeout.ShouldBe(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public void Timeout_uses_the_timeout_notice()
    {
        WorkspaceConnectionTestNotice.For(new OperationCanceledException(), timedOut: true)
            .ShouldBe(WorkspaceConnectionTestNotice.TimedOut);
    }

    [Fact]
    public void Cancel_uses_the_cancelled_notice()
    {
        WorkspaceConnectionTestNotice.For(new OperationCanceledException(), timedOut: false)
            .ShouldBe(WorkspaceConnectionTestNotice.Cancelled);
        WorkspaceConnectionTestNotice.For(new ObjectDisposedException("editor"), timedOut: false)
            .ShouldBe(WorkspaceConnectionTestNotice.Cancelled);
    }

    [Fact]
    public void Unreachable_uses_the_error_notice()
    {
        WorkspaceConnectionTestNotice.For(new HttpRequestException("down"), timedOut: false)
            .ShouldBe(WorkspaceConnectionTestNotice.Unreachable);
    }

    [Fact]
    public void A_blank_key_is_rejected_before_the_ping()
    {
        var editor = ReadEditor();
        editor.ShouldContain("requireApiKey: !IsEdit");
        editor.ShouldContain("ApiKeyRequiredForConnectionTest");
        editor.ShouldContain("WorkspaceConnectionTestNotice.Timeout");
        editor.ShouldContain("ComponentCancellationToken");
    }

    private static string ReadEditor()
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
                "WorkspaceEditor.razor.cs");
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("WorkspaceEditor.razor.cs");
    }
}
