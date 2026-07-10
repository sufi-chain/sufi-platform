using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using SufiChain.SufiAbp.UI.Progression;

namespace SufiChain.SufiAbp.UI.Blazor.Components;

/// <summary>
/// Automatically shows the page progress indicator during internal Blazor navigations.
/// </summary>
public partial class SufiAbpNavigationProgress : ComponentBase, IDisposable
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected IUiPageProgressService PageProgressService { get; set; } = default!;

    private IDisposable? _locationChangingRegistration;
    private bool _handlersRegistered;
    private bool _navigationProgressVisible;
    private string? _navigationStartUri;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_handlersRegistered)
        {
            _handlersRegistered = true;
            _locationChangingRegistration = NavigationManager.RegisterLocationChangingHandler(OnLocationChangingAsync);
        }

        return Task.CompletedTask;
    }

    private ValueTask OnLocationChangingAsync(LocationChangingContext context)
    {
        if (IsSameLocation(context.TargetLocation, NavigationManager.Uri))
        {
            return ValueTask.CompletedTask;
        }

        _navigationStartUri = NavigationManager.Uri;
        _navigationProgressVisible = true;
        _ = PageProgressService.ShowIndeterminateAsync();

        _ = InvokeAsync(async () =>
        {
            await Task.Yield();

            if (!_navigationProgressVisible)
            {
                return;
            }

            if (IsSameLocation(NavigationManager.Uri, _navigationStartUri))
            {
                await HideNavigationProgressAsync();
            }
        });

        return ValueTask.CompletedTask;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!_navigationProgressVisible)
        {
            return;
        }

        _ = InvokeAsync(async () =>
        {
            await Task.Yield();
            await HideNavigationProgressAsync();
        });
    }

    private async Task HideNavigationProgressAsync()
    {
        if (!_navigationProgressVisible)
        {
            return;
        }

        _navigationProgressVisible = false;
        _navigationStartUri = null;
        await PageProgressService.HideAsync();
    }

    private static bool IsSameLocation(string left, string right)
    {
        return string.Equals(NormalizeLocation(left), NormalizeLocation(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeLocation(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return string.Empty;
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var absoluteUri))
        {
            return uri.TrimEnd('/');
        }

        var path = absoluteUri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        var query = absoluteUri.Query;
        var fragment = absoluteUri.Fragment;

        return string.IsNullOrEmpty(path) ? "/" + query + fragment : path + query + fragment;
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        _locationChangingRegistration?.Dispose();

        if (_navigationProgressVisible)
        {
            _ = PageProgressService.HideAsync();
        }
    }
}
