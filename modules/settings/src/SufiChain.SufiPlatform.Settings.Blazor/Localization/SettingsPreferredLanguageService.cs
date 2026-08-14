using Microsoft.AspNetCore.Components.Authorization;
using SufiChain.SufiPlatform.UI.Localization;

namespace SufiChain.SufiPlatform.Settings.Blazor.Localization;

public class SettingsPreferredLanguageService : IPreferredLanguageService
{
    protected ICurrentUserLanguagePreferenceAppService AppService { get; }
    protected AuthenticationStateProvider AuthenticationStateProvider { get; }

    public SettingsPreferredLanguageService(
        ICurrentUserLanguagePreferenceAppService appService,
        AuthenticationStateProvider authenticationStateProvider)
    {
        AppService = appService;
        AuthenticationStateProvider = authenticationStateProvider;
    }

    public virtual async Task SetAsync(string cultureName)
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        await AppService.UpdateAsync(new UpdateCurrentUserLanguagePreferenceInput
        {
            CultureName = cultureName
        });
    }
}
