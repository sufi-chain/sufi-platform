using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.TagsManagement.Tags;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

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

    private SbDataGrid<TagDto>? _gridRef;
    private string? _scopeFilter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private bool _showLinksDrawer;
    private TagDto? _selectedTag;
    private TagDto? _selectedTagForLinks;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["Tags"];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await RefreshGridAsync();
        }
    }

    private async Task<SbDataResponse<TagDto>> LoadTagsDataAsync(SbDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(_scopeFilter))
        {
            var result = await TagAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
                MaxResultCount = request.PageSize
            });

            _totalCount = result.TotalCount;
            return new SbDataResponse<TagDto>(result.Items, result.TotalCount);
        }

        var scopedResult = await TagAppService.GetListByScopeAsync(_scopeFilter);
        var all = scopedResult.Items.ToList();
        var page = all
            .Skip(Math.Max(0, request.PageIndex * request.PageSize))
            .Take(request.PageSize)
            .ToList();

        _totalCount = all.Count;
        return new SbDataResponse<TagDto>(page, all.Count);
    }

    private Task RefreshGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadTags);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshGridAsync();
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

    private void ShowLinksDrawer(TagDto tag)
    {
        _selectedTagForLinks = tag;
        _showLinksDrawer = true;
    }

    private void SetCreateOpen(bool open)
    {
        _showCreateModal = open;
    }

    private void SetEditOpen(bool open)
    {
        _showEditModal = open;
    }

    private void SetLinksDrawerOpen(bool open)
    {
        _showLinksDrawer = open;

        if (!open)
        {
            _selectedTagForLinks = null;
        }
    }

    private async Task OnTagCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["TagCreatedSuccessfully"]);
        await RefreshGridAsync();
    }

    private async Task OnTagUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["TagUpdatedSuccessfully"]);
        await RefreshGridAsync();
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
            await RefreshGridAsync();
        }, LoadingKeys.DeleteTag);
    }
}
