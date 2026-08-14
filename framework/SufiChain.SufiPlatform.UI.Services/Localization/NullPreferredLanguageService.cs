namespace SufiChain.SufiPlatform.UI.Services.Localization;

/// <summary>
/// Keeps language switching cookie-only for anonymous applications and hosts
/// that do not include a user-preference store.
/// </summary>
public class NullPreferredLanguageService : UI.Localization.IPreferredLanguageService
{
    public Task SetAsync(string cultureName)
    {
        return Task.CompletedTask;
    }
}
