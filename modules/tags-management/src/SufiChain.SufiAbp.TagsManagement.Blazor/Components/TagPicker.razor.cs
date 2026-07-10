using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.TagsManagement.Tags;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Components;

/// <summary>
/// Reusable taxonomy tag picker backed by Tags Management search and optional create.
/// </summary>
public partial class TagPicker : TagsManagementComponentBase
{
    private ITagAppService TagAppService => LazyGetRequiredService(ref _tagAppService);
    private ITagAppService? _tagAppService;

    [Parameter]
    public string Scope { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<TagDto> SelectedItems { get; set; } = Array.Empty<TagDto>();

    [Parameter]
    public EventCallback<IReadOnlyList<TagDto>> SelectedItemsChanged { get; set; }

    [Parameter]
    public Func<string, Task<IEnumerable<TagDto>>>? SearchFunc { get; set; }

    [Parameter]
    public Func<string, Task<TagDto?>>? OnCreateAsync { get; set; }

    [Parameter]
    public bool AllowCreate { get; set; } = true;

    [Parameter]
    public int? MaxTags { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    protected virtual Task<IEnumerable<TagDto>> ResolveSearchAsync(string filter)
    {
        if (SearchFunc != null)
        {
            return SearchFunc(filter);
        }

        return SearchByScopeAsync(filter);
    }

    protected virtual Task<TagDto?> ResolveCreateAsync(string name)
    {
        if (!AllowCreate)
        {
            return Task.FromResult<TagDto?>(null);
        }

        if (OnCreateAsync != null)
        {
            return OnCreateAsync(name);
        }

        return CreateByScopeAsync(name);
    }

    protected virtual async Task<IEnumerable<TagDto>> SearchByScopeAsync(string filter)
    {
        var result = await TagAppService.SearchAsync(new SearchTagsInput
        {
            Scope = Scope,
            Filter = filter,
            MaxResultCount = 20
        });

        return result.Items;
    }

    protected virtual async Task<TagDto?> CreateByScopeAsync(string name)
    {
        return await TagAppService.CreateAsync(new CreateTagDto
        {
            Name = name,
            Scope = Scope
        });
    }
}
