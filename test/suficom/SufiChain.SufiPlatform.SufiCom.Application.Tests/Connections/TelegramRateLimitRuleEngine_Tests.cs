using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Connections;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

/// <summary>
/// Pure unit tests for <see cref="TelegramRateLimitRuleEngine"/>. No caches or DBs — the engine
/// is deterministic given the observed counter state, so each rule is exercised in isolation.
/// </summary>
public class TelegramRateLimitRuleEngine_Tests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    private static TelegramRateLimitOptions Defaults() => new();

    private static TelegramConnection ReadyConnection(DateTime? created = null)
    {
        var connection = new TelegramConnection(
            Guid.NewGuid(),
            tenantId: null,
            phoneNumber: "+989120000001",
            displayName: "Support",
            roleLabel: "Support",
            isEnabled: true);
        connection.PrepareForAuth("session-abc");
        connection.MarkReady();
        return connection;
    }

    [Fact]
    public void PerChatInterval_Blocks_Second_Send_Within_Interval()
    {
        var options = Defaults();
        var connection = ReadyConnection();
        var last = Now.AddSeconds(-0.2); // 200ms ago, below the 1000ms default.

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: last, nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: Now);

        decision.Allowed.ShouldBeFalse();
        decision.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramRateLimited);
        decision.RetryAfterSeconds.ShouldNotBeNull();
        decision.RetryAfterSeconds!.Value.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void PerChatInterval_Allows_After_Interval_Elapses()
    {
        var options = Defaults();
        var connection = ReadyConnection();
        var last = Now.AddSeconds(-2); // 2s ago, past the 1s interval.

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: last, nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: Now);

        decision.Allowed.ShouldBeTrue();
    }

    [Fact]
    public void FirstContactCap_Blocks_New_Connection_At_10()
    {
        var options = Defaults();
        var connection = ReadyConnection(created: Now.AddDays(-1)); // < 7 days => new, cap=10

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: true, lastSendAtUtc: null, nowUtc: Now,
            firstContactSentToday: 10, connectionAgeDays: 1, recentSendCount: 0, burstWindowStartedAtUtc: null);

        decision.Allowed.ShouldBeFalse();
        decision.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramFirstContactCapReached);
    }

    [Fact]
    public void FirstContactCap_Uses_Higher_Cap_For_Established_Connection()
    {
        var options = Defaults();
        var connection = ReadyConnection(created: Now.AddDays(-30)); // >= 7 days => established, cap=30

        // 10 first-contacts on an established connection is under the 30 cap => allowed.
        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: true, lastSendAtUtc: null, nowUtc: Now,
            firstContactSentToday: 10, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: null);

        decision.Allowed.ShouldBeTrue();
    }

    [Fact]
    public void Reply_Bypasses_FirstContactCap()
    {
        var options = Defaults();
        var connection = ReadyConnection(created: Now.AddDays(-1));

        // Not a first contact (reply) — cap does not apply even at the limit.
        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: Now.AddSeconds(-2), nowUtc: Now,
            firstContactSentToday: 99, connectionAgeDays: 1, recentSendCount: 0, burstWindowStartedAtUtc: null);

        decision.Allowed.ShouldBeTrue();
    }

    [Fact]
    public void Burst_Pause_Triggers_At_Threshold_In_Window()
    {
        var options = Defaults(); // BurstCount=20, BurstWindowSeconds=60
        var connection = ReadyConnection();

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: Now.AddSeconds(-2), nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 20, burstWindowStartedAtUtc: Now.AddSeconds(-10));

        decision.Allowed.ShouldBeFalse();
        decision.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramBurstPaused);
        decision.RetryAfterSeconds.ShouldBe(options.BurstPauseSeconds);
    }

    [Fact]
    public void Burst_Does_Not_Trigger_Outside_Window()
    {
        var options = Defaults();
        var connection = ReadyConnection();

        // Window opened 120s ago — beyond the 60s window — so the count is stale and not enforced.
        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: Now.AddSeconds(-2), nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 99, burstWindowStartedAtUtc: Now.AddSeconds(-120));

        decision.Allowed.ShouldBeTrue();
    }

    [Fact]
    public void FloodWait_Denies_With_RetryAfter()
    {
        var options = Defaults();
        var connection = ReadyConnection();
        connection.MarkError(errorCode: "FLOOD_WAIT", floodWaitUntil: Now.AddMinutes(5));

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: null, nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: null);

        decision.Allowed.ShouldBeFalse();
        decision.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramRateLimited);
        decision.RetryAfterSeconds.ShouldNotBeNull();
        decision.RetryAfterSeconds!.Value.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void PeerFlood_Denies_Cold_Contacts_But_Allows_Replies()
    {
        var options = Defaults();
        var connection = ReadyConnection();
        connection.MarkError(errorCode: "PEER_FLOOD", spamRestricted: true);

        var cold = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: true, lastSendAtUtc: null, nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: null);

        cold.Allowed.ShouldBeFalse();
        cold.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramSpamRestricted);

        var reply = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: false, lastSendAtUtc: Now.AddSeconds(-2), nowUtc: Now,
            firstContactSentToday: 0, connectionAgeDays: 30, recentSendCount: 0, burstWindowStartedAtUtc: null);

        reply.Allowed.ShouldBeTrue();
    }

    [Fact]
    public void Disabled_Rules_Allow_Everything()
    {
        var options = new TelegramRateLimitOptions
        {
            PerChatIntervalMs = 0,
            FirstContactDailyCapNew = 0,
            FirstContactDailyCapEstablished = 0,
            BurstCount = 0
        };
        var connection = ReadyConnection();

        var decision = TelegramRateLimitRuleEngine.Evaluate(
            options, connection, isFirstContact: true, lastSendAtUtc: Now, nowUtc: Now,
            firstContactSentToday: 9999, connectionAgeDays: 1, recentSendCount: 9999, burstWindowStartedAtUtc: Now);

        decision.Allowed.ShouldBeTrue();
    }
}
