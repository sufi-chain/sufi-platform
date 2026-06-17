using System;
using System.Collections.Generic;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;

namespace SufiChain.SufiAbp.FileManager.Blazor.Components.FileManager;

/// <summary>
/// State management for the FileManager component
/// </summary>
public class FileManagerState
{
    /// <summary>
    /// Currently selected folder ID
    /// </summary>
    public Guid? CurrentFolderId { get; set; }

    /// <summary>
    /// Current folder path
    /// </summary>
    public string CurrentPath { get; set; } = "/";

    /// <summary>
    /// Current folder contents
    /// </summary>
    public FolderContentsDto? Contents { get; set; }

    /// <summary>
    /// Selected files
    /// </summary>
    public HashSet<Guid> SelectedFiles { get; set; } = new();

    /// <summary>
    /// Selected folders
    /// </summary>
    public HashSet<Guid> SelectedFolders { get; set; } = new();

    /// <summary>
    /// Current view mode
    /// </summary>
    public FileViewMode ViewMode { get; set; } = FileViewMode.LargeIcons;

    /// <summary>
    /// Current explorer source mode.
    /// </summary>
    public FileExplorerSourceMode SourceMode { get; set; } = FileExplorerSourceMode.DirectoryMap;

    /// <summary>
    /// Navigation history for back/forward
    /// </summary>
    public List<NavigationHistoryItem> History { get; set; } = new();

    /// <summary>
    /// Current position in history
    /// </summary>
    public int HistoryIndex { get; set; } = -1;

    /// <summary>
    /// Whether files are currently loading
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Search filter
    /// </summary>
    public string SearchFilter { get; set; } = "";

    /// <summary>
    /// Sort field
    /// </summary>
    public string SortField { get; set; } = "Name";

    /// <summary>
    /// Sort direction
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Clipboard state
    /// </summary>
    public ClipboardStateDto? Clipboard { get; set; }

    /// <summary>
    /// Whether renaming is in progress
    /// </summary>
    public bool IsRenaming { get; set; }

    /// <summary>
    /// Item being renamed
    /// </summary>
    public Guid? RenamingItemId { get; set; }

    /// <summary>
    /// Check if any items are selected
    /// </summary>
    public bool HasSelection => SelectedFiles.Count > 0 || SelectedFolders.Count > 0;

    /// <summary>
    /// Get total selection count
    /// </summary>
    public int SelectionCount => SelectedFiles.Count + SelectedFolders.Count;

    /// <summary>
    /// Check if can go back in history
    /// </summary>
    public bool CanGoBack => HistoryIndex > 0;

    /// <summary>
    /// Check if can go forward in history
    /// </summary>
    public bool CanGoForward => HistoryIndex < History.Count - 1;

    /// <summary>
    /// Check if clipboard has content
    /// </summary>
    public bool CanPaste => SourceMode == FileExplorerSourceMode.DirectoryMap && Clipboard?.HasContent == true;

    /// <summary>
    /// Clear selection
    /// </summary>
    public void ClearSelection()
    {
        SelectedFiles.Clear();
        SelectedFolders.Clear();
    }

    /// <summary>
    /// Navigate to a folder and add to history
    /// </summary>
    public void NavigateTo(Guid? folderId, string path)
    {
        // Remove forward history if we're in the middle
        if (HistoryIndex < History.Count - 1)
        {
            History.RemoveRange(HistoryIndex + 1, History.Count - HistoryIndex - 1);
        }

        // Add new entry
        History.Add(new NavigationHistoryItem { FolderId = folderId, Path = path });
        HistoryIndex = History.Count - 1;

        CurrentFolderId = folderId;
        CurrentPath = path;
        ClearSelection();
    }

    /// <summary>
    /// Go back in history
    /// </summary>
    public void GoBack()
    {
        if (CanGoBack)
        {
            HistoryIndex--;
            var item = History[HistoryIndex];
            CurrentFolderId = item.FolderId;
            CurrentPath = item.Path;
            ClearSelection();
        }
    }

    /// <summary>
    /// Go forward in history
    /// </summary>
    public void GoForward()
    {
        if (CanGoForward)
        {
            HistoryIndex++;
            var item = History[HistoryIndex];
            CurrentFolderId = item.FolderId;
            CurrentPath = item.Path;
            ClearSelection();
        }
    }
}

/// <summary>
/// File view mode options
/// </summary>
public enum FileViewMode
{
    LargeIcons,
    SmallIcons,
    List,
    Details,
    Tiles
}

/// <summary>
/// Navigation history item
/// </summary>
public class NavigationHistoryItem
{
    public Guid? FolderId { get; set; }
    public string Path { get; set; } = "/";
}

/// <summary>
/// Event args for drag-drop operations
/// </summary>
public class DragDropEventArgs
{
    public List<Guid> FileIds { get; set; } = new();
    public List<Guid> FolderIds { get; set; } = new();
    public Guid? TargetFolderId { get; set; }
    public string? TargetPath { get; set; }
}

