using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;
using SufiChain.SufiPlatform.UI.Blazor.Messages;
using SufiChain.SufiPlatform.UI.Messages;
using SufiChain.SufiPlatform.UI.Notifications;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor;

public class MessageServiceSuccessTests
{
    [Fact]
    public async Task MessageService_Success_ShowsToast_NotModal()
    {
        var notify = new RecordingNotifications();
        var (service, modals) = Create(notify);
        await service.SuccessAsync("Saved");
        notify.Successes.ShouldBe(new[] { "Saved" });
        modals.ShouldBeEmpty();
    }

    [Fact]
    public async Task MessageService_SuccessBlocking_ShowsModal()
    {
        var notify = new RecordingNotifications();
        var (service, modals) = Create(notify);
        await service.SuccessAsync("Saved", options: o => o.Blocking = true);
        notify.Successes.ShouldBeEmpty();
        modals.Single().Type.ShouldBe(UiMessageType.Success);
    }

    [Fact]
    public async Task MessageService_Error_StaysModal()
    {
        var notify = new RecordingNotifications();
        var (service, modals) = Create(notify);
        await service.ErrorAsync("Failed");
        notify.Successes.ShouldBeEmpty();
        modals.Single().Type.ShouldBe(UiMessageType.Error);
    }

    [Fact]
    public async Task MessageService_Confirm_Unchanged()
    {
        var (service, _) = Create(new RecordingNotifications());
        var answer = await service.ConfirmAsync("Delete?");
        answer.ShouldBeTrue();
    }

    private static (SufiBlazorMessageService Service, List<MessageRequest> Modals) Create(IUiNotificationService notify)
    {
        var modals = new List<MessageRequest>();
        var service = new SufiBlazorMessageService(circuitIdAccessor: new FixedCircuit("c1"), notifications: notify);
        service.RegisterMessageHandler(request =>
        {
            modals.Add(request);
            return Task.CompletedTask;
        });
        service.RegisterConfirmHandler(_ => Task.FromResult(true));
        return (service, modals);
    }

    private sealed class FixedCircuit : IBlazorCircuitIdAccessor
    {
        public FixedCircuit(string id) => CurrentCircuitId = id;
        public string? CurrentCircuitId { get; }
    }

    private sealed class RecordingNotifications : IUiNotificationService
    {
        public List<string> Successes { get; } = [];
        public Task SuccessAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null)
        {
            Successes.Add(message);
            return Task.CompletedTask;
        }

        public Task InfoAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null) => Task.CompletedTask;
        public Task WarnAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null) => Task.CompletedTask;
        public Task ErrorAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null) => Task.CompletedTask;
    }
}
