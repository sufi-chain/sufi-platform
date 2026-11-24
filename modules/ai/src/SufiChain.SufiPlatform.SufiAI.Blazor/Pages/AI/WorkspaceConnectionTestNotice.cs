using System.Net.Http;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

/// <summary>
/// Test Connection always ends in a notice. The shared loading helper swallows cancellation,
/// so the editor decides the message before that helper sees the exception.
/// </summary>
public static class WorkspaceConnectionTestNotice
{
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    public const string Success = "ConnectionTestSuccessful";
    public const string TimedOut = "ConnectionTestTimedOut";
    public const string Cancelled = "ConnectionTestCancelled";
    public const string Unreachable = "ConnectionTestUnreachable";
    public const string ApiKeyRequired = "ApiKeyRequiredForConnectionTest";

    public static string? For(Exception exception, bool timedOut)
    {
        if (exception is OperationCanceledException)
        {
            return timedOut ? TimedOut : Cancelled;
        }

        if (exception is HttpRequestException)
        {
            return Unreachable;
        }

        if (exception is ObjectDisposedException)
        {
            return Cancelled;
        }

        return null;
    }
}
