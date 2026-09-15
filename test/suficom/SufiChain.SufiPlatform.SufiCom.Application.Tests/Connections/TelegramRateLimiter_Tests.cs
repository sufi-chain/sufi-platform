using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiCom.Application.Connections;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Connections;
using SufiChain.SufiPlatform.SufiCom.Features;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

/// <summary>
/// Application-layer tests for <see cref="TelegramRateLimiter"/>: strict-first-contact feature
/// toggling, and that a daily-cap denial is surfaced. Uses NSubstitute for the cache, daily
/// repository, and feature checker so the test runs without an ABP host.
/// </summary>
public class TelegramRateLimiter_Tests
{
    private static TelegramConnection ReadyConnection()
    {
        var connection = new TelegramConnection(
            Guid.NewGuid(), null, "+989120000001", "Support", "Support", isEnabled: true);
        connection.PrepareForAuth("session-abc");
        connection.MarkReady();
        return connection;
    }

    private static (TelegramRateLimiter limiter, ITelegramConnectionRateLimitRepository repo, IDistributedCache cache, IFeatureChecker features) CreateLimiter(bool strict, int firstContactSent = 0)
    {
        var cache = Substitute.For<IDistributedCache>();
        // GetStringAsync is an extension over GetAsync; stub the underlying byte[] call so the
        // limiter observes an empty cache (no prior sends, no burst window).
        cache.GetAsync(default!).ReturnsForAnyArgs((byte[]?)null);

        var repo = Substitute.For<ITelegramConnectionRateLimitRepository>();
        var row = new TelegramConnectionRateLimit(Guid.NewGuid(), null, Guid.NewGuid(), DateTime.UtcNow);
        // Simulate prior first-contacts already at the new-connection cap.
        for (var i = 0; i < firstContactSent; i++)
        {
            row.RecordSend(isFirstContact: true, DateTime.UtcNow);
        }
        repo.GetOrCreateForDateAsync(default, default, default).ReturnsForAnyArgs(row);
        repo.UpdateAsync(default!, autoSave: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(row);

        var options = Options.Create(new TelegramRateLimitOptions
        {
            PerChatIntervalMs = 0, // disable interval so only the cap is exercised
            FirstContactDailyCapNew = 10,
            FirstContactDailyCapEstablished = 30,
            EstablishedConnectionAgeDays = 7,
            BurstCount = 0 // disable burst
        });

        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(SufiComFeatures.Names.TelegramStrictFirstContact).Returns(strict);

        var limiter = new TelegramRateLimiter(cache, repo, options, features);
        return (limiter, repo, cache, features);
    }

    [Fact]
    public async Task StrictFeature_On_Forces_Reply_To_FirstContact_Cap()
    {
        var connection = ReadyConnection();
        // Connection created now (< 7 days) so the new-connection cap (10) applies once strict
        // forces isFirstContact=true for the reply. Pre-load the daily counter to the cap.
        var (limiter, _, _, _) = CreateLimiter(strict: true, firstContactSent: 10);

        // Reply (not a cold contact) — with strict mode on, the cap still applies.
        var decision = await limiter.CheckAsync(connection, "tg-chat-1", isFirstContact: false);

        decision.Allowed.ShouldBeFalse();
        decision.Reason.ShouldBe(SufiComDomainErrorCodes.TelegramFirstContactCapReached);
    }

    [Fact]
    public async Task StrictFeature_Off_Allows_Reply_Past_Cap()
    {
        var connection = ReadyConnection();
        var (limiter, _, _, _) = CreateLimiter(strict: false, firstContactSent: 99);

        // Reply with strict off — cap does not apply, the reply is allowed even at 99 contacts.
        var decision = await limiter.CheckAsync(connection, "tg-chat-1", isFirstContact: false);

        decision.Allowed.ShouldBeTrue();
    }

    [Fact]
    public async Task Allowed_Decision_Persists_The_Send()
    {
        var connection = ReadyConnection();
        var (limiter, repo, _, _) = CreateLimiter(strict: false, firstContactSent: 0);

        var decision = await limiter.CheckAsync(connection, "tg-chat-1", isFirstContact: true);

        decision.Allowed.ShouldBeTrue();
        // The daily aggregate must be updated to record the send.
        await repo.ReceivedWithAnyArgs().UpdateAsync(default!, autoSave: default, cancellationToken: default);
    }
}
