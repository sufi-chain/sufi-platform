using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Tags.Tags;

namespace SufiChain.SufiPlatform.Tags.Blazor.Components;

public partial class TagEditModal : TagsComponentBase
{
    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private ITagAppService TagAppService => LazyGetRequiredService(ref _tagAppService);
    private ITagAppService? _tagAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public TagDto? Tag { get; set; }
    [Parameter] public EventCallback OnTagUpdated { get; set; }

    private UpdateTagDto _model = new();
    private Guid _tagId;

    protected override void OnParametersSet()
    {
        if (Open && Tag != null && Tag.Id != _tagId)
        {
            _tagId = Tag.Id;
            _model = new UpdateTagDto
            {
                Name = Tag.Name,
                Scope = Tag.Scope,
                Color = Tag.Color
            };
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await TagAppService.UpdateAsync(_tagId, _model);
        await OnTagUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
