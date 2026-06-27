using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiBlazor.Contracts.Editors;

namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Editors;

public class FileManagerMarkdownToolbarContributor : IMdToolbarContributor
{
    public int Order => 110;

    public Task ConfigureToolbarAsync(MdToolbarContext context)
    {
        var dialogService = context.ServiceProvider.GetService<IFileGalleryDialogService>();

        context.Items.Add(new MdToolbarContributedItem
        {
            Id = "file-manager-md-image",
            IsVisible = () => dialogService?.IsHostRegistered == true,
            Group = "insert",
            Order = 10,
            Icon = "📁",
            Tooltip = "Insert Image from Gallery",
            OnClickAsync = async actionContext =>
            {
                var service = actionContext.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (service == null)
                {
                    return;
                }

                var result = await service.ShowImageGalleryAsync();
                if (result != null && !string.IsNullOrEmpty(result.Url))
                {
                    if (actionContext.EditorId.Contains("html", System.StringComparison.OrdinalIgnoreCase) &&
                        actionContext.InsertTextAsync != null)
                    {
                        await actionContext.InsertTextAsync($"<img src=\"{result.Url}\" alt=\"{result.Alt ?? string.Empty}\" />");
                    }
                    else if (actionContext.InsertImageMarkdownAsync != null)
                    {
                        await actionContext.InsertImageMarkdownAsync(result.Url, result.Alt);
                    }
                }
            }
        });

        context.Items.Add(new MdToolbarContributedItem
        {
            Id = "file-manager-md-attach",
            IsVisible = () => dialogService?.IsHostRegistered == true,
            Group = "insert",
            Order = 11,
            Icon = "📎",
            Tooltip = "Insert file as download link",
            OnClickAsync = async actionContext =>
            {
                var service = actionContext.ServiceProvider.GetService<IFileGalleryDialogService>();
                if (service == null)
                {
                    return;
                }

                var result = await service.ShowFileGalleryAsync();
                if (result != null && !string.IsNullOrEmpty(result.Url))
                {
                    if (actionContext.EditorId.Contains("html", System.StringComparison.OrdinalIgnoreCase) &&
                        actionContext.InsertTextAsync != null)
                    {
                        await actionContext.InsertTextAsync($"<a href=\"{result.Url}\">{result.FileName ?? result.Url}</a>");
                    }
                    else if (actionContext.InsertLinkMarkdownAsync != null)
                    {
                        await actionContext.InsertLinkMarkdownAsync(result.Url, result.FileName);
                    }
                }
            }
        });

        return Task.CompletedTask;
    }
}
