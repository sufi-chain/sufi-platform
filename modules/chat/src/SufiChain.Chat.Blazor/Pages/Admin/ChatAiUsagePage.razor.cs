using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiBlazor.Components.Data;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.AiUsage.View)]
public partial class ChatAiUsagePage : ChatComponentBase
{
    protected IChatAiUsageAppService AiUsageAppService => LazyGetRequiredService(ref _aiUsageAppService);
    private IChatAiUsageAppService? _aiUsageAppService;

    protected IChatAssistantAvailabilityAppService AssistantAvailabilityAppService =>
        LazyGetRequiredService(ref _assistantAvailabilityAppService);
    private IChatAssistantAvailabilityAppService? _assistantAvailabilityAppService;

    protected ChatAiUsageDashboardDto? Dashboard { get; set; }

    protected ChatAssistantAvailabilityDto? AssistantAvailability { get; set; }

    protected ChatAiUsagePolicyDto? Policy { get; set; }

    protected IReadOnlyList<ChatAiUsageReservationDto> Reservations { get; set; } = Array.Empty<ChatAiUsageReservationDto>();

    protected int ReservationTotalCount { get; set; }

    protected int ReservationPageIndex { get; set; }

    protected int ReservationPageSize { get; set; } = 10;

    protected static class LoadingKeys
    {
        public const string LoadDashboard = "load-dashboard";
        public const string LoadReservations = "load-reservations";
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardAsync();
        await LoadReservationsAsync();
    }

    protected virtual async Task LoadDashboardAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            AssistantAvailability = await AssistantAvailabilityAppService.GetAsync();
            Dashboard = await AiUsageAppService.GetDashboardAsync(new GetChatAiUsageDashboardInput());
            Policy = await AiUsageAppService.GetEffectivePolicyAsync();
        }, LoadingKeys.LoadDashboard);
    }

    protected virtual async Task LoadReservationsAsync()
    {
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
        await LoadDashboardAsync();
        await LoadReservationsAsync();
    }
}
