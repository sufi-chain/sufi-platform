using SufiChain.SufiPlatform.SufiCom.Chat.Bot.Runtime;
using SufiChain.SufiPlatform.SufiCom.Chat.Features;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using SufiChain.SufiPlatform.Settings;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Integration;

/// <summary>
/// sufi-chain#571: the chat-bot worker calls the provider through this source.
/// A disabled setting or feature must not invoke the mock or write a usage row.
/// </summary>
public class ChatBotAiCompletionSource_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatBotAiCompletionSource _source;
    private readonly ISettingManager _settingManager;
    private readonly IChatAiUsageReservationRepository _reservations;
    private readonly ConfigurableAiService _ai;
    private readonly ConfigurableFeatureChecker _features;

    public ChatBotAiCompletionSource_Tests()
    {
        _source = GetRequiredService<IChatBotAiCompletionSource>();
        _settingManager = GetRequiredService<ISettingManager>();
        _reservations = GetRequiredService<IChatAiUsageReservationRepository>();
        _ai = GetRequiredService<ConfigurableAiService>();
        _features = GetRequiredService<ConfigurableFeatureChecker>();
    }

    [Fact]
    public async Task When_ai_is_off_the_bot_worker_does_not_call_the_provider_or_reserve_credit()
    {
        var called = 0;
        var previous = _ai.BeforeComplete;
        var previousContent = _ai.ResponseContent;
        _ai.ResponseContent = "bot should not run";
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        var sessionId = Guid.NewGuid();
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: false);

            var result = await _source.CompleteAsync(Request(sessionId));

            result.Denied.ShouldBeTrue();
            result.Completed.ShouldBeFalse();
            called.ShouldBe(0);
            (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == sessionId);
        }
        finally
        {
            _ai.BeforeComplete = previous;
            _ai.ResponseContent = previousContent;
        }
    }

    [Fact]
    public async Task When_ai_is_on_the_bot_worker_returns_the_mocked_reply_and_records_usage()
    {
        var previous = _ai.BeforeComplete;
        var previousContent = _ai.ResponseContent;
        _ai.BeforeComplete = null;
        _ai.ResponseContent = "bot reply";
        var sessionId = Guid.NewGuid();
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);

            var result = await _source.CompleteAsync(Request(sessionId));

            result.Denied.ShouldBeFalse();
            result.Completed.ShouldBeTrue();
            result.Text.ShouldBe("bot reply");
            (await _reservations.GetListAsync()).ShouldContain(row => row.SessionId == sessionId);
        }
        finally
        {
            _ai.BeforeComplete = previous;
            _ai.ResponseContent = previousContent;
        }
    }

    [Fact]
    public async Task When_the_feature_is_off_the_bot_worker_does_not_call_the_provider()
    {
        var called = 0;
        var previous = _ai.BeforeComplete;
        _ai.BeforeComplete = _ =>
        {
            called++;
            return Task.CompletedTask;
        };
        _features.Disable(ChatFeatures.Ai.Enable);
        var sessionId = Guid.NewGuid();
        try
        {
            await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager, enabled: true);

            var result = await _source.CompleteAsync(Request(sessionId));

            result.Denied.ShouldBeTrue();
            called.ShouldBe(0);
            (await _reservations.GetListAsync()).ShouldNotContain(row => row.SessionId == sessionId);
        }
        finally
        {
            _features.Enable(ChatFeatures.Ai.Enable);
            _ai.BeforeComplete = previous;
        }
    }

    private static ChatBotAiCompletionRequest Request(Guid sessionId) => new()
    {
        ChatSessionId = sessionId,
        BotDefinitionId = Guid.NewGuid(),
        UserText = "hello"
    };
}
