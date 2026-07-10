using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.TagsManagement.Tags;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Components;

public partial class TagLinksDrawer : TagsManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadLinks = "load-links";
        public const string Unassign = "unassign";
    }

    private ITagLinkAppService TagLinkAppService => LazyGetRequiredService(ref _tagLinkAppService);
    private ITagLinkAppService? _tagLinkAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public TagDto? Tag { get; set; }

    private List<TagLinkDto> _links = new();
    private Guid _loadedTagId;

    private string DrawerTitle => Tag == null ? L["TagLinks"] : L["TagLinksFor", Tag.Name];

    protected override async Task OnParametersSetAsync()
    {
        if (Open && Tag != null && Tag.Id != _loadedTagId)
        {
            _loadedTagId = Tag.Id;
            await LoadLinksAsync();
        }
    }

    private async Task LoadLinksAsync()
    {
        if (Tag == null)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _links = await TagLinkAppService.GetLinksByTagAsync(Tag.Id);
        }, LoadingKeys.LoadLinks);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;

        if (!open)
        {
            _loadedTagId = Guid.Empty;
            _links.Clear();
        }

        await OpenChanged.InvokeAsync(open);
    }

    private Task UnassignAsync(TagLinkDto link) => ExecuteWithLoadingAsync(async () =>
    {
        if (!await Message.ConfirmAsync(L["UnassignTagLinkConfirmation", link.EntityType, link.EntityId]))
        {
            return;
        }

        await TagLinkAppService.UnassignAsync(new AssignTagDto
        {
            TagId = link.TagId,
            EntityType = link.EntityType,
            EntityId = link.EntityId
        });

        await Notify.SuccessAsync(L["TagUnassignedSuccessfully"]);
        await LoadLinksAsync();
    }, $"{LoadingKeys.Unassign}-{link.Id}");
}
