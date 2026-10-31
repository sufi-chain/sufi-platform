using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.UI.Blazor.Layout;

/// <summary>
/// Keeps the entity name (for example the record being edited) as the last breadcrumb,
/// after the menu-derived trail. Idempotent and re-applied whenever the layout rebuilds breadcrumbs.
/// Dispose it with the page.
/// </summary>
public sealed class EntityBreadcrumbTracker : IDisposable
{
    private readonly IPageLayout _layout;
    private string? _name;
    private BreadcrumbItem? _added;
    private readonly Func<Action, Task>? _dispatch;
    private bool _applying;
    private bool _disposed;

    /// <param name="layout">The page layout.</param>
    /// <param name="dispatch">Runs the re-apply on the UI thread, typically <c>a => InvokeAsync(a)</c>.</param>
    public EntityBreadcrumbTracker(IPageLayout layout, Func<Action, Task>? dispatch = null)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _dispatch = dispatch;
        _layout.BreadcrumbItems.CollectionChanged += OnBreadcrumbsChanged;
    }

    public void Set(string? entityName)
    {
        _name = string.IsNullOrWhiteSpace(entityName) ? null : entityName.Trim();
        Apply();
    }

    // ObservableCollection forbids changes inside CollectionChanged, so re-apply after the event.
    // A yielded continuation would resume on a single-threaded test context that is blocked
    // waiting for the crumb, so the no-dispatch path queues the thread pool instead.
    private void OnBreadcrumbsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_applying)
        {
            return;
        }

        if (_dispatch != null)
        {
            _ = _dispatch(Apply);
            return;
        }

        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            if (!_disposed)
            {
                Apply();
            }
        }, null);
    }

    private void Apply()
    {
        if (_applying)
        {
            return;
        }

        _applying = true;
        try
        {
            var items = _layout.BreadcrumbItems;
            if (_added != null && (!items.Contains(_added) || !string.Equals(_added.Text, _name, StringComparison.Ordinal)))
            {
                items.Remove(_added);
                _added = null;
            }

            if (_name == null || _added != null || items.Count == 0)
            {
                return;
            }

            if (string.Equals(items[^1].Text?.Trim(), _name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _added = new BreadcrumbItem(_name);
            items.Add(_added);
        }
        finally
        {
            _applying = false;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _layout.BreadcrumbItems.CollectionChanged -= OnBreadcrumbsChanged;
        if (_added != null)
        {
            _layout.BreadcrumbItems.Remove(_added);
            _added = null;
        }
    }
}
