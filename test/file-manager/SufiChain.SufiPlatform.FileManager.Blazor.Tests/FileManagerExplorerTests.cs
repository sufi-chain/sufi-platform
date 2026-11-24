using SufiChain.SufiPlatform.FileManager.FileFolders;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class FileManagerExplorerTests
{
    [Fact]
    public void Search_keeps_only_folders_whose_names_match()
    {
        var contents = new FolderContentsDto
        {
            Folders =
            {
                new FolderTreeNodeDto { Name = "qa479-notes" },
                new FolderTreeNodeDto { Name = "Invoices" }
            }
        };

        FolderNameFilter.Apply(contents, "qa479");

        contents.Folders.Count.ShouldBe(1);
        contents.Folders[0].Name.ShouldBe("qa479-notes");
    }

    [Fact]
    public void Blank_search_keeps_every_folder()
    {
        var contents = new FolderContentsDto
        {
            Folders = { new FolderTreeNodeDto { Name = "Invoices" } }
        };

        FolderNameFilter.Apply(contents, "  ");

        contents.Folders.Count.ShouldBe(1);
    }

    [Fact]
    public void Details_and_list_views_render_the_rename_editor()
    {
        var razor = Read("Components", "FileManager", "FileView.razor");
        var details = razor.Split("case FileViewMode.Details:", 2)[1];
        var list = razor.Split("case FileViewMode.List:", 2)[1].Split("case FileViewMode.Details:", 2)[0];

        details.ShouldContain("fm-rename-input");
        details.ShouldContain("FinishRename");
        list.ShouldContain("fm-rename-input");
    }

    [Fact]
    public void View_mode_and_folder_are_stored_in_local_storage()
    {
        var razor = Read("Components", "FileManager", "FileManager.razor");

        razor.ShouldContain("ILocalStorageService");
        razor.ShouldContain("SufiFileManager.");
        razor.ShouldContain("ReadStoredViewModeAsync");
        razor.ShouldContain("PersistFolderAsync");
    }

    [Fact]
    public void Upload_drop_zone_handles_drag_and_drop()
    {
        var razor = Read("Components", "Upload", "FileUploader.razor");

        razor.ShouldContain("@ondragenter=\"HandleDragEnter\"");
        razor.ShouldContain("@ondragover:preventDefault");
        razor.ShouldContain("@ondrop=\"HandleDrop\"");
        razor.ShouldContain("@ondrop:preventDefault");
    }

    private static string Read(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(new[]
            {
                dir.FullName,
                "modules",
                "file-manager",
                "src",
                "SufiChain.SufiPlatform.FileManager.Blazor"
            }.Concat(parts).ToArray());
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(string.Join("/", parts));
    }
}
