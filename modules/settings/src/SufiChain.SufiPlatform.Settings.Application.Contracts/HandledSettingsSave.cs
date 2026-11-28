namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// A settings save that already showed field errors. Validation stays on that path.
/// Any other exception still propagates.
/// </summary>
public static class HandledSettingsSave
{
    public static bool IsValidation(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().Name, "AbpValidationException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static async Task<bool> RunAsync(Func<Task> persist)
    {
        try
        {
            await persist();
            return true;
        }
        catch (Exception exception) when (IsValidation(exception))
        {
            return false;
        }
    }
}
