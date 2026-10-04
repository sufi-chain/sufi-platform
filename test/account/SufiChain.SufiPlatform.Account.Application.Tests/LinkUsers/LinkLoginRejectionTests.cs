using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.AspNetCore;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.Account.LinkUsers;

public class LinkLoginRejectionTests
{
    [Fact]
    public void DescribeFailure_Should_Report_An_Invalid_Token_Without_Echoing_It()
    {
        var provider = NewProvider(TimeSpan.FromDays(1));
        var token = "not-a-real-link-token";

        var reason = provider.DescribeFailure(
            token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            Guid.NewGuid().ToString(),
            "stamp",
            supportsSecurityStamp: true);

        reason.ShouldBe(LinkLoginRejectionReasons.TokenInvalid);
        reason.ShouldNotContain(token);
    }

    [Fact]
    public void DescribeFailure_Should_Report_An_Expired_Token()
    {
        var provider = NewProvider(TimeSpan.FromTicks(-1));
        var userId = Guid.NewGuid().ToString();
        var token = provider.Protect(
            DateTimeOffset.UtcNow,
            userId,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            "stamp");

        var reason = provider.DescribeFailure(
            token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            userId,
            "stamp",
            supportsSecurityStamp: true);

        reason.ShouldBe(LinkLoginRejectionReasons.TokenExpired);
        reason.ShouldNotContain(token);
    }

    [Fact]
    public void DescribeFailure_Should_Report_A_Purpose_Mismatch()
    {
        var provider = NewProvider(TimeSpan.FromDays(1));
        var userId = Guid.NewGuid().ToString();
        var token = provider.Protect(DateTimeOffset.UtcNow, userId, "OtherPurpose", "stamp");

        var reason = provider.DescribeFailure(
            token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            userId,
            "stamp",
            supportsSecurityStamp: true);

        reason.ShouldBe(LinkLoginRejectionReasons.TokenPurposeMismatch);
    }

    [Fact]
    public void DescribeFailure_Should_Report_A_User_Mismatch()
    {
        var provider = NewProvider(TimeSpan.FromDays(1));
        var token = provider.Protect(
            DateTimeOffset.UtcNow,
            Guid.NewGuid().ToString(),
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            "stamp");

        var reason = provider.DescribeFailure(
            token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            Guid.NewGuid().ToString(),
            "stamp",
            supportsSecurityStamp: true);

        reason.ShouldBe(LinkLoginRejectionReasons.TokenUserMismatch);
    }

    [Fact]
    public void DescribeFailure_Should_Report_A_Security_Stamp_Mismatch()
    {
        var provider = NewProvider(TimeSpan.FromDays(1));
        var userId = Guid.NewGuid().ToString();
        var token = provider.Protect(
            DateTimeOffset.UtcNow,
            userId,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            "old-stamp");

        var reason = provider.DescribeFailure(
            token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose,
            userId,
            "new-stamp",
            supportsSecurityStamp: true);

        reason.ShouldBe(LinkLoginRejectionReasons.TokenSecurityStampMismatch);
    }

    [Fact]
    public void Write_Should_Log_The_Reason_At_Warning_Without_A_Token()
    {
        var logger = new CapturingLogger();
        var token = "super-secret-link-token";
        var sourceUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var targetTenantId = Guid.NewGuid();

        LinkLoginRejectionLog.Write(
            logger,
            LinkLoginRejectionReasons.NotLinked,
            sourceUserId,
            sourceTenantId: null,
            targetUserId,
            targetTenantId);

        logger.Level.ShouldBe(LogLevel.Warning);
        logger.Text.ShouldContain(LinkLoginRejectionReasons.NotLinked);
        logger.Text.ShouldContain(sourceUserId.ToString());
        logger.Text.ShouldContain(targetUserId.ToString());
        logger.Text.ShouldContain(targetTenantId.ToString());
        logger.Text.ShouldNotContain(token);
        logger.Text.ShouldNotContain("Token");
    }

    private static InspectableLinkUserTokenProvider NewProvider(TimeSpan lifespan)
    {
        var options = Options.Create(new DataProtectionTokenProviderOptions
        {
            TokenLifespan = lifespan
        });
        return new InspectableLinkUserTokenProvider(new EphemeralDataProtectionProvider(), options);
    }

    private sealed class InspectableLinkUserTokenProvider : LinkUserTokenProvider
    {
        public InspectableLinkUserTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options)
            : base(
                dataProtectionProvider,
                options,
                NullLogger<DataProtectorTokenProvider<IdentityUser>>.Instance)
        {
        }

        public string Protect(DateTimeOffset created, string userId, string purpose, string stamp)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true), leaveOpen: true))
            {
                writer.Write(created.UtcTicks);
                writer.Write(userId);
                writer.Write(purpose ?? string.Empty);
                writer.Write(stamp ?? string.Empty);
            }

            return Convert.ToBase64String(Protector.Protect(stream.ToArray()));
        }
    }

    private sealed class CapturingLogger : ILogger
    {
        public LogLevel Level { get; private set; }
        public string Text { get; private set; } = string.Empty;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Level = logLevel;
            Text = formatter(state, exception);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose()
            {
            }
        }
    }
}
