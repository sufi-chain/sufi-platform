namespace SufiChain.SufiPlatform.UI.Localization;

/// <summary>
/// Persists the preferred UI language for the current authenticated user when
/// the host has a user-preference store.
/// </summary>
public interface IPreferredLanguageService
{
    Task SetAsync(string cultureName);
}
