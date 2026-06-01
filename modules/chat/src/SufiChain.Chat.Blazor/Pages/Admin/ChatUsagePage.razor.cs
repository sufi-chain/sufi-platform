using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Features;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Usage.View)]
public partial class ChatUsagePage : ChatComponentBase
{
    protected IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    protected bool ChatEnabled { get; set; }

    protected bool UsageGuardEnabled { get; set; }

    protected bool AiUsageGuardEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ChatEnabled = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
        UsageGuardEnabled = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
        AiUsageGuardEnabled = await FeatureChecker.IsEnabledAsync(ChatFeatures.Ai.UsageGuard);
    }
}
