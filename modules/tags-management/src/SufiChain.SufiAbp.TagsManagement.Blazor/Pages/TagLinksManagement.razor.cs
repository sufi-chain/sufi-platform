using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.TagsManagement.Tags;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Pages;

public partial class TagLinksManagement : TagsManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Search = "search";
        public const string Assign = "assign";
        public const string Unassign = "unassign";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private ITagAppService TagAppService => LazyGetRequiredService(ref _tagAppService);
    private ITagAppService? _tagAppService;

    private ITagLinkAppService TagLinkAppService => LazyGetRequiredService(ref _tagLinkAppService);
    private ITagLinkAppService? _tagLinkAppService;

    private string _entityType = string.Empty;
    private string _entityIdText = string.Empty;
    private string? _availableScopeFilter;

    private bool _hasSearched;
    private Guid _entityId;

    private List<TagDto> _allTags = new();
    private List<TagDto> _assignedTags = new();

    private List<TagDto> _availableTags => _allTags
        .Where(t => !_assignedTags.Any(a => a.Id == t.Id))
        .Where(t => string.IsNullOrWhiteSpace(_availableScopeFilter)
                    || t.Scope.Equals(_availableScopeFilter, StringComparison.OrdinalIgnoreCase))
        .ToList();

    protected override void OnInitialized()
    {
        PageLayout.Title = L["TagLinks"];
    }

    private Task SearchAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(_entityType))
        {
            await Message.ErrorAsync(L["EntityTypeIsRequired"]);
            return;
        }

        if (!Guid.TryParse(_entityIdText, out _entityId))
        {
            await Message.ErrorAsync(L["InvalidEntityId"]);
            return;
        }

        var assigned = await TagLinkAppService.GetTagsByEntityAsync(new EntityTagQueryInput
        {
            EntityType = _entityType,
            EntityId = _entityId
        });
        _assignedTags = assigned.ToList();

        var all = await TagAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 1000
        });
        _allTags = all.Items.ToList();

        _hasSearched = true;
    }, LoadingKeys.Search);

    private Task AssignAsync(TagDto tag) => ExecuteWithLoadingAsync(async () =>
    {
        await TagLinkAppService.AssignAsync(new AssignTagDto
        {
            TagId = tag.Id,
            EntityType = _entityType,
            EntityId = _entityId
        });

        await Notify.SuccessAsync(L["TagAssignedSuccessfully"]);
        await RefreshAssignedAsync();
    }, $"{LoadingKeys.Assign}-{tag.Id}");

    private Task UnassignAsync(TagDto tag) => ExecuteWithLoadingAsync(async () =>
    {
        await TagLinkAppService.UnassignAsync(new AssignTagDto
        {
            TagId = tag.Id,
            EntityType = _entityType,
            EntityId = _entityId
        });

        await Notify.SuccessAsync(L["TagUnassignedSuccessfully"]);
        await RefreshAssignedAsync();
    }, $"{LoadingKeys.Unassign}-{tag.Id}");

    private async Task RefreshAssignedAsync()
    {
        var assigned = await TagLinkAppService.GetTagsByEntityAsync(new EntityTagQueryInput
        {
            EntityType = _entityType,
            EntityId = _entityId
        });
        _assignedTags = assigned.ToList();
    }
}
