using Microsoft.AspNetCore.Components;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor.Pages.ShortLinks;

public partial class ShortLinkRedirectBase : ShortLinksComponentBase
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public string BaseKey { get; set; } = string.Empty;

    [Parameter] public string ShortCode { get; set; } = string.Empty;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = currentUri.Query;

        var redirectUrl =
            $"api/short-link/redirect/{Uri.EscapeDataString(BaseKey)}/{Uri.EscapeDataString(ShortCode)}{query}";

        NavigationManager.NavigateTo(redirectUrl, forceLoad: true);
    }
}
