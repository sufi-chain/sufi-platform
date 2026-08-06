using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.PageToolbars;

namespace SufiChain.SufiPlatform.UI.Services.Layout;

/// <summary>
/// Default implementation of IPageLayout.
/// </summary>
public class DefaultPageLayout : IPageLayout
{
    private string? _title;
    private object? _toolbarContent;

    /// <inheritdoc/>
    public string? Title
    {
        get => _title;
        set
        {
            _title = value;
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
            //Console.WriteLine($"[DefaultPageLayout] ToolbarContent setter: {_toolbarContent?.GetType().Name ?? "null"} -> {value?.GetType().Name ?? "null"}");
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
        //Console.WriteLine("[DefaultPageLayout] Reset() called");
        Title = null;
        ToolbarContent = null;
        BreadcrumbItems.Clear();
        ToolbarItems.Clear();
        //Console.WriteLine("[DefaultPageLayout] Reset() complete");
    }
}
