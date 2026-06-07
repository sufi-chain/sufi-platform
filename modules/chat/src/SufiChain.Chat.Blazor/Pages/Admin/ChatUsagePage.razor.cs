using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Features;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Usage.View)]
public partial class ChatUsagePage : ChatComponentBase
{
    protected IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
    private IFeatureChecker? _featureChecker;

    protected IChatSettingsAppService? SettingsAppService => LazyGetService(ref _settingsAppService);
    private IChatSettingsAppService? _settingsAppService;

    protected IChatAiUsageAppService? AiUsageAppService => LazyGetService(ref _aiUsageAppService);
    private IChatAiUsageAppService? _aiUsageAppService;

    protected IChatAssistantAvailabilityAppService? AssistantAvailabilityAppService =>
        LazyGetService(ref _assistantAvailabilityAppService);
    private IChatAssistantAvailabilityAppService? _assistantAvailabilityAppService;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected bool ChatEnabled { get; set; }

    protected bool UsageGuardEnabled { get; set; }

    protected bool AiUsageGuardEnabled { get; set; }

    protected bool IsLoading { get; set; }

    protected bool CanViewAiUsage { get; set; }

    protected ChatSettingsDto? Settings { get; set; }

    protected ChatAiUsageDashboardDto? Dashboard { get; set; }

    protected ChatAssistantAvailabilityDto? AssistantAvailability { get; set; }

    protected ChatAiUsagePolicyDto? Policy { get; set; }

    protected IReadOnlyList<ChatAiUsageReservationDto> Reservations { get; set; } = Array.Empty<ChatAiUsageReservationDto>();

    protected int ReservationTotalCount { get; set; }

    protected int ReservationPageIndex { get; set; }

    protected int ReservationPageSize { get; set; } = 10;

    protected sealed record TierSummary(string LabelKey, SbColor Color, ChatUsageTierSettingsDto Dto);

    protected IReadOnlyList<TierSummary> TierSummaries => Settings == null
        ? Array.Empty<TierSummary>()
        : new[]
        {
            new TierSummary("Tier:PublicAnonymous", SbColor.Warning, Settings.PublicAnonymous),
            new TierSummary("Tier:PublicAuthenticated", SbColor.Info, Settings.PublicAuthenticated),
            new TierSummary("Tier:Internal", SbColor.Primary, Settings.Internal)
        };

    protected static class LoadingKeys
    {
        public const string LoadAiDashboard = "load-ai-dashboard";
        public const string LoadReservations = "load-reservations";
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected virtual async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            ChatEnabled = await FeatureChecker.IsEnabledAsync(ChatFeatures.Enable);
            UsageGuardEnabled = ChatEnabled;
            AiUsageGuardEnabled = await FeatureChecker.IsEnabledAsync(ChatFeatures.Ai.UsageGuard);
            CanViewAiUsage = await IsGrantedAsync(ChatPermissions.AiUsage.View);

            if (SettingsAppService != null && await IsGrantedAsync(ChatPermissions.Settings.Manage))
            {
                try
                {
                    Settings = await SettingsAppService.GetAsync();
                }
                catch
                {
                    Settings = null;
                }
            }

            if (CanViewAiUsage)
            {
                await LoadAiDashboardAsync();
                await LoadReservationsAsync();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task LoadAiDashboardAsync()
    {
        if (AiUsageAppService == null || AssistantAvailabilityAppService == null)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            AssistantAvailability = await AssistantAvailabilityAppService.GetAsync();
            Dashboard = await AiUsageAppService.GetDashboardAsync(new GetChatAiUsageDashboardInput());
            Policy = await AiUsageAppService.GetEffectivePolicyAsync();
        }, LoadingKeys.LoadAiDashboard);
    }

    protected virtual async Task LoadReservationsAsync()
    {
        if (AiUsageAppService == null)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await AiUsageAppService.GetReservationsAsync(new GetChatAiUsageReservationsInput
            {
                MaxResultCount = ReservationPageSize,
                SkipCount = ReservationPageIndex * ReservationPageSize
            });

            Reservations = result.Items;
            ReservationTotalCount = (int)result.TotalCount;
        }, LoadingKeys.LoadReservations);
    }

    protected virtual async Task OnReservationPageChangedAsync(int pageIndex)
    {
        ReservationPageIndex = pageIndex;
        await LoadReservationsAsync();
    }

    protected virtual async Task RefreshAsync()
    {
        await LoadAsync();
    }

    protected virtual void OpenSettings()
    {
        NavigationManager.NavigateTo("/admin/chat/settings");
    }
}
