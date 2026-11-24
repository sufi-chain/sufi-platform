namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Folder rows used to ignore the explorer search term, so a query kept every child folder.
/// </summary>
public static class FolderNameFilter
{
    public static void Apply(FolderContentsDto contents, string? filter)
    {
        if (contents.Folders.Count == 0 || string.IsNullOrWhiteSpace(filter))
        {
            return;
        }

        var term = filter.Trim();
        contents.Folders = contents.Folders
            .Where(folder => !string.IsNullOrEmpty(folder.Name) &&
                             folder.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
