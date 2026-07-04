using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.TagsManagement.Tags;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Pages;

public partial class TagsManagement : TagsManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadTags = "load-tags";
        public const string DeleteTag = "delete-tag";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private ITagAppService TagAppService => LazyGetRequiredService(ref _tagAppService);
    private ITagAppService? _tagAppService;

    private List<TagDto> _tags = new();
    private string? _scopeFilter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private TagDto? _selectedTag;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["Tags"];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadTagsAsync();
        }
    }

    private Task LoadTagsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(_scopeFilter))
        {
            var result = await TagAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                SkipCount = _pageIndex * _pageSize,
                MaxResultCount = _pageSize
            });

            _tags = result.Items.ToList();
            _totalCount = result.TotalCount;
        }
        else
        {
            var result = await TagAppService.GetListByScopeAsync(_scopeFilter);
            var all = result.Items.ToList();
            _totalCount = all.Count;
            _tags = all
                .Skip(_pageIndex * _pageSize)
                .Take(_pageSize)
                .ToList();
        }
    }, LoadingKeys.LoadTags);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadTagsAsync();
    }

    private async Task OnScopeFilterChanged(string? value)
    {
        _scopeFilter = value;
        _pageIndex = 0;
        await LoadTagsAsync();
    }

    private void ShowCreateModal()
    {
        _showCreateModal = true;
    }

    private void ShowEditModal(TagDto tag)
    {
        _selectedTag = tag;
        _showEditModal = true;
    }

    private void SetCreateOpen(bool open)
    {
        _showCreateModal = open;
    }

    private void SetEditOpen(bool open)
    {
        _showEditModal = open;
    }

    private async Task OnTagCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["TagCreatedSuccessfully"]);
        await LoadTagsAsync();
    }

    private async Task OnTagUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["TagUpdatedSuccessfully"]);
        await LoadTagsAsync();
    }

    private async Task DeleteTagAsync(TagDto tag)
    {
        if (!await Message.ConfirmAsync(L["DeleteTagConfirmation", tag.Name]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await TagAppService.DeleteAsync(tag.Id);
            await Notify.SuccessAsync(L["TagDeletedSuccessfully"]);
            await LoadTagsAsync();
        }, LoadingKeys.DeleteTag);
    }
}
