using Microsoft.AspNetCore.Components;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Pages.ShortLinkGenerator;

public partial class ShortLinkRedirectBase : ShortLinkGeneratorComponentBase
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public string BaseKey { get; set; } = string.Empty;

    [Parameter] public string ShortCode { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        var currentUri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = currentUri.Query;

        var redirectUrl =
            $"api/short-link/redirect/{Uri.EscapeDataString(BaseKey)}/{Uri.EscapeDataString(ShortCode)}{query}";

        NavigationManager.NavigateTo(redirectUrl, forceLoad: true);
    }
}
