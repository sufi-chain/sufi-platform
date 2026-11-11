using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.PageToolbars;

namespace SufiChain.SufiPlatform.UI.Services.Layout;

/// <summary>
/// Default implementation of IPageLayout.
/// </summary>
public class DefaultPageLayout : IPageLayout, IPageLayoutNavigation
{
    private string? _title;
    private string? _subtitle;
    private object? _toolbarContent;
    private int _navigationGeneration;
    private bool _navigationOpen;
    private bool _suppressStamp;
    private int _titleGeneration = int.MinValue;
    private int _subtitleGeneration = int.MinValue;
    private int _toolbarGeneration = int.MinValue;
    private int _breadcrumbGeneration = int.MinValue;

    public DefaultPageLayout()
    {
        // Let the layout re-render when a page adds or replaces breadcrumbs after the first render.
        BreadcrumbItems.CollectionChanged += (_, _) =>
        {
            if (!_suppressStamp)
            {
                _breadcrumbGeneration = _navigationGeneration;
            }

            OnPropertyChanged(nameof(BreadcrumbItems));
        };
        ToolbarItems.CollectionChanged += (_, _) =>
        {
            if (!_suppressStamp)
            {
                _toolbarGeneration = _navigationGeneration;
            }

            OnPropertyChanged(nameof(ToolbarItems));
        };
    }

    /// <inheritdoc/>
    public void BeginNavigation()
    {
        _navigationGeneration++;
        _navigationOpen = true;
    }

    /// <inheritdoc/>
    public string? Title
    {
        get => _title;
        set
        {
            if (!_suppressStamp)
            {
                _titleGeneration = _navigationGeneration;
            }

            if (string.Equals(_title, value, StringComparison.Ordinal))
            {
                return;
            }

            _title = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc/>
    public string? Subtitle
    {
        get => _subtitle;
        set
        {
            if (!_suppressStamp)
            {
                _subtitleGeneration = _navigationGeneration;
            }

            if (string.Equals(_subtitle, value, StringComparison.Ordinal))
            {
                return;
            }

            _subtitle = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc/>
    public ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; } = new();

    /// <inheritdoc/>
    public ObservableCollection<PageToolbarItem> ToolbarItems { get; } = new();

    /// <inheritdoc/>
    public object? ToolbarContent
    {
        get => _toolbarContent;
        set
        {
            if (!_suppressStamp)
            {
                _toolbarGeneration = _navigationGeneration;
            }

            // Always notify. Razor ChildContent is often the same delegate across parent
            // renders, while Disabled/Loading inside that fragment already changed.
            // Skipping a same-instance assign leaves the top bar on the first button state.
            _toolbarContent = value;
            OnPropertyChanged();
        }
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // The router renders the next page before the layout handles LocationChanged.
        // Content stamped for the open navigation is the incoming page and must survive.
        // A Reset with no open navigation still clears everything (explicit reset, tests).
        var keepGeneration = _navigationOpen ? _navigationGeneration : int.MinValue;
        _navigationOpen = false;
        _suppressStamp = true;
        try
        {
            if (_titleGeneration != keepGeneration)
            {
                _title = null;
                _titleGeneration = int.MinValue;
                OnPropertyChanged(nameof(Title));
            }

            if (_subtitleGeneration != keepGeneration)
            {
                _subtitle = null;
                _subtitleGeneration = int.MinValue;
                OnPropertyChanged(nameof(Subtitle));
            }

            if (_toolbarGeneration != keepGeneration)
            {
                _toolbarContent = null;
                ToolbarItems.Clear();
                _toolbarGeneration = int.MinValue;
                OnPropertyChanged(nameof(ToolbarContent));
            }

            if (_breadcrumbGeneration != keepGeneration)
            {
                BreadcrumbItems.Clear();
                _breadcrumbGeneration = int.MinValue;
            }
        }
        finally
        {
            _suppressStamp = false;
        }
    }
}
