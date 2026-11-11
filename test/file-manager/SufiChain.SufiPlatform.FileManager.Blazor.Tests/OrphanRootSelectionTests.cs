using SufiChain.SufiPlatform.FileManager.Blazor.Components.FileManager;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class OrphanRootSelectionTests
{
    [Fact]
    public void Delete_stays_disabled_until_an_allowed_folder_is_selected()
    {
        var orphanId = System.Guid.NewGuid();
        var liveId = System.Guid.NewGuid();
        var state = new FileManagerState
        {
            Contents = new FolderContentsDto
            {
                ParentFolder = new FolderTreeNodeDto { Path = "/" },
                Folders =
                {
                    new FolderTreeNodeDto { Id = orphanId, Name = "QA B59 1111", CanDelete = true },
                    new FolderTreeNodeDto { Id = liveId, Name = "Live", CanDelete = false }
                }
            }
        };

        state.CanGoUp.ShouldBeTrue();
        state.CanDeleteSelection.ShouldBeFalse();

        state.SelectedFolders.Add(liveId);
        state.CanDeleteSelection.ShouldBeFalse();

        state.SelectedFolders.Clear();
        state.SelectedFolders.Add(orphanId);
        state.CanDeleteSelection.ShouldBeTrue();
    }

    [Fact]
    public void Up_is_disabled_when_the_listing_has_no_parent()
    {
        var state = new FileManagerState
        {
            Contents = new FolderContentsDto()
        };

        state.CanGoUp.ShouldBeFalse();
    }
}
