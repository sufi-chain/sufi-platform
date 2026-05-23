using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.UI.Branding;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.UI.Blazor.Components;

/// <summary>
/// Component that manages the browser page title based on the current page layout.
/// </summary>
public partial class SufiAbpPageTitle : ComponentBase, IDisposable
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected IBrandingProvider BrandingProvider { get; set; } = default!;

    protected override void OnInitialized()
    {
        PageLayout.PropertyChanged += OnPageLayoutChanged;
    }

    private string GetPageTitle()
    {
        if (string.IsNullOrEmpty(PageLayout.Title))
        {
            return BrandingProvider.AppName;
        }
        return $"{PageLayout.Title} | {BrandingProvider.AppName}";
    }

    private void OnPageLayoutChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPageLayout.Title))
        {
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        PageLayout.PropertyChanged -= OnPageLayoutChanged;
    }
}
