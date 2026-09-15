using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiBlazor.Contracts.Editors;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors;

/// <summary>
/// File Manager gallery actions for the unified editor (image and file insert).
/// </summary>
public class SufiFileManagerEditorContributor : IEditorToolbarContributor
{
    public int Order => 110;

    public SbEditorSurface Surfaces =>
        SbEditorSurface.Toolbar | SbEditorSurface.BubbleMenu | SbEditorSurface.SlashMenu | SbEditorSurface.Code;

    public Task ConfigureAsync(EditorToolbarContext context)
    {
        var dialogService = context.ServiceProvider.GetService<IFileGalleryDialogService>();

        context.Items.Add(new EditorToolbarItem
        {
            Id = "file-manager-image",
            Group = "insert",
            Order = 10,
            IconName = "image",
            LabelKey = "Editor:InsertImageFromGallery",
            Tooltip = "Insert image from gallery",
            Surfaces = Surfaces,
            IsVisible = _ => dialogService?.IsHostRegistered == true,
            OnClickAsync = async action =>
            {
                var service = action.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (service == null)
                {
                    return;
                }

                var result = await service.ShowImageGalleryAsync();
                if (result == null || string.IsNullOrEmpty(result.Url))
                {
                    return;
                }

                await action.Document.InsertImageAsync(result.Url, result.Alt);
            }
        });

        context.Items.Add(new EditorToolbarItem
        {
            Id = "file-manager-file",
            Group = "insert",
            Order = 11,
            IconName = "paperclip",
            LabelKey = "Editor:InsertFileFromGallery",
            Tooltip = "Insert file as download link",
            Surfaces = Surfaces,
            IsVisible = _ => dialogService?.IsHostRegistered == true,
            OnClickAsync = async action =>
            {
                var service = action.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (service == null)
                {
                    return;
                }

                var result = await service.ShowFileGalleryAsync();
                if (result == null || string.IsNullOrEmpty(result.Url))
                {
                    return;
                }

                await action.Document.InsertFileAsync(result.Url, result.FileName ?? result.Url, result.MimeType);
            }
        });

        return Task.CompletedTask;
    }
}
