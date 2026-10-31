using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Settings;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk;

public class HelpDeskSettingsSectionTests
{
    [Fact]
    public async Task Update_stores_prefix_and_clears_an_empty_hooshvare_id()
    {
        var store = new SectionMemorySettings();
        var service = Host(new HelpDeskSettingsAppService(store, store));

        await service.UpdateAsync(new HelpDeskSettingsDto
        {
            DefaultAiHooshvareId = "  ",
            TicketNumberPrefix = " HD ",
            TicketNumberSequenceStart = 4,
            LiveChatDefaultInboxId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString(),
            LiveChatAutoHandoffToAiAfterMinutes = 0,
            LiveChatQueueWaitWarnMinutes = 5,
            LiveChatPresenceHeartbeatSeconds = 30,
            TicketingRetentionClosedTicketDays = 730
        });

        store.Get(HelpDeskSettings.TicketNumberPrefix).ShouldBe("HD");
        store.Get(HelpDeskSettings.DefaultAiHooshvareId).ShouldBeNull();
        store.Get(HelpDeskSettings.TicketNumberSequenceStart).ShouldBe("4");
        store.Get(HelpDeskSettings.LiveChatAllowOperatorSendDuringAiMode).ShouldBe("false");
    }

    [Fact]
    public async Task A_zero_sequence_start_is_rejected()
    {
        var store = new SectionMemorySettings();
        var service = Host(new HelpDeskSettingsAppService(store, store));
        var input = new HelpDeskSettingsDto { TicketNumberPrefix = "HD-", TicketNumberSequenceStart = 0 };

        await Should.ThrowAsync<AbpValidationException>(() => service.UpdateAsync(input));
        store.Get(HelpDeskSettings.TicketNumberPrefix).ShouldBeNull();
    }

    private static T Host<T>(T service) where T : Volo.Abp.Application.Services.ApplicationService
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentTenant, HostTenant>();
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private sealed class HostTenant : ICurrentTenant
    {
        public bool IsAvailable => false;
        public Guid? Id => null;
        public string? Name => null;
        public IDisposable Change(Guid? id, string? name = null) => new Noop();
        private sealed class Noop : IDisposable { public void Dispose() { } }
    }
}
