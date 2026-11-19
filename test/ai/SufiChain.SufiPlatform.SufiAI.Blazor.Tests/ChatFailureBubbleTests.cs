using System.Globalization;
using Bunit;
using Bunit.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.SufiCom.Chat;
using Volo.Abp.Timing;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Components;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Services;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Tests;

/// <summary>
/// A failed provider turn must leave a non-empty assistant bubble. The shared timeline is what
/// the tenant shared-workspace chat renders.
/// </summary>
public class ChatFailureBubbleTests : BunitContext
{
    public ChatFailureBubbleTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IStringLocalizerFactory>(new TestLocalizerFactory());
        Services.AddSingleton<IClock, TestClock>();
        Services.AddSingleton<ChatMessageTimelineJsInterop>(sp => new DisposableTimelineJs(sp.GetRequiredService<IJSRuntime>()));
    }

    [Fact]
    public void A_provider_failure_stays_in_the_shared_timeline_bubble()
    {
        const string error = "AI provider authorization failed: check the workspace API key.";
        var cut = Render<ChatMessageTimeline>(parameters => parameters.Add(
            component => component.Messages,
            new List<ChatMessageDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Body = error,
                    SenderKind = SufiChain.SufiPlatform.SufiCom.ChatMessageSenderKind.Assistant
                }
            }));

        Assert.Contains(error, cut.Markup);
        cut.Render();
        Assert.Contains(error, cut.Markup);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    private sealed class TestClock : IClock
    {
        public DateTime Now => DateTime.UtcNow;

        public DateTimeKind Kind => DateTimeKind.Utc;

        public bool SupportsMultipleTimezone => false;

        public DateTime Normalize(DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        public DateTime ConvertToUserTime(DateTime utcDateTime) => utcDateTime;

        public DateTimeOffset ConvertToUserTime(DateTimeOffset utcDateTime) => utcDateTime;

        public DateTime ConvertToUtc(DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    private sealed class DisposableTimelineJs : ChatMessageTimelineJsInterop, IDisposable
    {
        public DisposableTimelineJs(IJSRuntime jsRuntime)
            : base(jsRuntime, NullLogger<ChatMessageTimelineJsInterop>.Instance)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class TestLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new TestLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new TestLocalizer();
    }

    private sealed class TestLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
