using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.TagsManagement.Tags;

namespace SufiChain.SufiAbp.TagsManagement.Blazor.Components;

public partial class TagCreateModal : TagsManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private ITagAppService TagAppService => LazyGetRequiredService(ref _tagAppService);
    private ITagAppService? _tagAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnTagCreated { get; set; }

    private CreateTagDto _model = new();

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new CreateTagDto();
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
        await TagAppService.CreateAsync(_model);
        await OnTagCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
