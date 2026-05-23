using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiBlazor.Contracts.Editors;

namespace SufiChain.SufiAbp.FileManager.RichTextEditor.Toolbar;

/// <summary>
/// Contributes file manager toolbar items to the rich text editor.
/// Adds buttons for inserting images from the file gallery and attaching files.
/// Items are only visible when a FileGalleryHost component is present on the page.
/// </summary>
public class FileManagerToolbarContributor : IRteToolbarContributor
{
    /// <summary>
    /// Items are added after the default "insert" group items.
    /// </summary>
    public int Order => 110;

    public Task ConfigureToolbarAsync(RteToolbarContext context)
    {
        var dialogService = context.ServiceProvider.GetService<IFileGalleryDialogService>();

        // Insert Image from Gallery button (only when FileGalleryHost is present)
        context.Items.Add(new RteToolbarContributedItem
        {
            Id = "file-manager-image",
            IsVisible = () => dialogService?.IsHostRegistered == true,
            Group = "insert",
            Order = 10, // After link and image
            Icon = "📁",
            Tooltip = "Insert Image from Gallery",
            OnClickAsync = async (actionContext) =>
            {
                var dialogService = actionContext.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (dialogService != null)
                {
                    var result = await dialogService.ShowImageGalleryAsync();
                    if (result != null && !string.IsNullOrEmpty(result.Url))
                    {
                        if (actionContext.InsertImageAsync != null)
                        {
                            await actionContext.InsertImageAsync(result.Url, result.Alt);
                        }
                    }
                }
            }
        });

        // Attach File button - inserts selected file as a download link (only when FileGalleryHost is present)
        context.Items.Add(new RteToolbarContributedItem
        {
            Id = "file-manager-attach",
            IsVisible = () => dialogService?.IsHostRegistered == true,
            Group = "insert",
            Order = 11, // After image gallery
            Icon = "📎",
            Tooltip = "Insert file as download link",
            OnClickAsync = async (actionContext) =>
            {
                var dialogService = actionContext.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (dialogService != null)
                {
                    var result = await dialogService.ShowFileGalleryAsync();
                    if (result != null && !string.IsNullOrEmpty(result.Url))
                    {
                        // Insert as a link using InsertLinkAsync (avoids clipboard/Delta issues with insertHtml)
                        if (actionContext.InsertLinkAsync != null)
                        {
                            await actionContext.InsertLinkAsync(result.Url, result.FileName);
                        }
                    }
                }
            }
        });

        return Task.CompletedTask;
    }
}
