namespace SufiChain.SufiPlatform.AspNetCore.Authentication.Server.Controllers;

public static class LogoutSession
{
    public static async Task RunAsync(
        Func<Task> securityLog,
        Func<Task> signOut,
        IReadOnlyList<Func<Task>> extraSignOuts,
        Action<Exception> warn)
    {
        await StepAsync(securityLog, warn);
        await StepAsync(signOut, warn);
        foreach (var extra in extraSignOuts)
        {
            await StepAsync(extra, warn);
        }
    }

    private static async Task StepAsync(Func<Task> step, Action<Exception> warn)
    {
        try
        {
            await step();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            warn(ex);
        }
    }
}
