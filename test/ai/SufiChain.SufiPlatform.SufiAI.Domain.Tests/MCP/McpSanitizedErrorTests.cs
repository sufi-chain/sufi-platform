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

    [Fact]
    public void Connection_error_drops_html_secrets_and_keyed_urls()
    {
        var summary = McpSanitizedError.SummarizeConnection(
            "HTTP 405 Method Not Allowed\nAuthorization: Bearer sk-test-000\nhttps://user:secret@mcp.example/path?token=test-token-unknown\n<!doctype html><html><body>nope</body></html>");

        summary.ShouldStartWith("HTTP 405 Method Not Allowed");
        summary.ShouldContain("did not answer as an MCP server");
        summary.Length.ShouldBeLessThanOrEqualTo(McpSanitizedError.ConnectionSummaryMaxLength);
        summary.ShouldNotContain("doctype");
        summary.ShouldNotContain("sk-test");
        summary.ShouldNotContain("test-token-unknown");
        summary.ShouldNotContain("secret@");
        summary.ShouldNotContain("/path");

        var keyed = McpSanitizedError.SummarizeConnection(
            "Could not connect to https://user:secret@mcp.example/path?token=test-token-unknown");
        keyed.ShouldContain("https://mcp.example");
        keyed.ShouldNotContain("test-token-unknown");
        keyed.ShouldNotContain("secret@");
    }
}
