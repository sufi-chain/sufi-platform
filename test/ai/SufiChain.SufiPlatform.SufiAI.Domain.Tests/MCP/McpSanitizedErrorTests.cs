using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

public class McpSanitizedErrorTests
{
    [Fact]
    public void FromException_Should_Include_Request_Id_And_Corrective_Action()
    {
        var error = McpSanitizedError.FromException(
            new InvalidOperationException("Tool list failed."),
            "req-24");

        var message = error.ToClientMessage();
        message.ShouldContain("req-24");
        message.ShouldContain("Tool list failed.");
        message.ShouldContain("Corrective action:");
        message.ShouldContain("allowlist");
    }

    [Fact]
    public void Sanitize_Should_Drop_Stack_Frames_And_Secrets()
    {
        var sanitized = McpSanitizedError.Sanitize(
            "Auth failed bearer sk-live-secretvalue\n   at Sufi.Secret.Method() in Program.cs:line 4\npassword=hunter2");

        sanitized.ShouldNotContain("sk-live");
        sanitized.ShouldNotContain("hunter2");
        sanitized.ShouldNotContain("Program.cs");
        sanitized.ShouldContain("[redacted]");
        sanitized.ShouldContain("Auth failed");
    }
}
