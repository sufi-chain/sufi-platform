using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shouldly;
using SufiChain.SufiPlatform.Account.Blazor;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class LoginOtpCaptchaFailureTests
{
    [Fact]
    public void Captcha_failure_uses_the_captcha_message_and_logs_the_code()
    {
        var logger = new ListLogger();
        var localizer = new KeyLocalizer();
        var exception = new BusinessException(IdentitySecurityErrorCodes.CaptchaValidationFailed);

        var message = AccountUiErrors.OtpSendFailure(logger, localizer, exception, "OtpSendFailed", out var resendAt);

        resendAt.ShouldBeNull();
        message.ShouldBe(AccountUiErrors.OtpCaptchaFailedKey);
        message.ShouldNotBe("OtpSendFailed");
        logger.Text.ShouldContain(IdentitySecurityErrorCodes.CaptchaValidationFailed);
    }

    [Fact]
    public void A_generic_send_failure_stays_on_the_send_message()
    {
        var logger = new ListLogger();
        var message = AccountUiErrors.OtpSendFailure(
            logger,
            new KeyLocalizer(),
            new InvalidOperationException("down"),
            "OtpSendFailed",
            out _);

        message.ShouldBe("OtpSendFailed");
        logger.Text.ShouldNotContain(IdentitySecurityErrorCodes.CaptchaValidationFailed);
    }

    private sealed class KeyLocalizer : IStringLocalizer<SufiAccountResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class ListLogger : ILogger
    {
        public string Text { get; private set; } = string.Empty;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new Noop();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Text += formatter(state, exception);
        }

        private sealed class Noop : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
