using SufiChain.SufiPlatform.Account;

namespace SufiChain.SufiPlatform.Account.Otp;

public static class RegisterOtpStartup
{
    public sealed record Result(bool SelfRegistrationEnabled, OtpOptionsDto? Options, bool Failed);

    public static async Task<Result> LoadAsync(
        Func<Task<bool>> readSelfRegistration,
        Func<Task<OtpOptionsDto?>> readOptions,
        Action<Exception> warn)
    {
        bool enabled;
        try
        {
            enabled = await readSelfRegistration();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            warn(ex);
            return new Result(false, null, true);
        }

        if (!enabled)
        {
            return new Result(false, null, false);
        }

        try
        {
            var options = await readOptions();
            return new Result(true, options, false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            warn(ex);
            return new Result(true, null, true);
        }
    }

    public static bool ShowsForm(Result result) =>
        result.SelfRegistrationEnabled
        && result.Options is { IsEnabled: true, AllowRegistration: true };
}
