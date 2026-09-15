using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Components;

public partial class SufiSettingsGroupLayout
{
    private readonly string _contentId = $"settings-content-{Guid.NewGuid():N}";
    private readonly string _headingId = $"settings-heading-{Guid.NewGuid():N}";

    [Parameter] public List<SettingComponentGroup> Groups { get; set; } = new();
    [Parameter] public string? SelectedGroupId { get; set; }
    [Parameter] public EventCallback<string> SelectedGroupIdChanged { get; set; }
    [Parameter] public bool Busy { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
