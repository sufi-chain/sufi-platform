using Microsoft.Extensions.Logging;
using Shouldly;
using SufiChain.SufiPlatform.Account.Blazor.Pages;
using SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class ConfirmEmailCleanupTests
{
    [Fact]
    public void Dispose_can_run_twice()
    {
        var page = new ConfirmEmail();

        Should.NotThrow(() =>
        {
            page.Dispose();
            page.Dispose();
        });
    }
}

public class SufiExceptionLogTests
{
    [Fact]
    public void LogError_includes_the_business_exception_code()
    {
        var logger = new CaptureLogger();
        var exception = new BusinessException("AIHooshvare:HooshvareNotFound");

        SufiExceptionLog.LogError(logger, exception, "An error occurred");
        SufiExceptionLog.LogError(logger, new InvalidOperationException("boom"), "User exception occurred");

        logger.Messages[0].ShouldContain("An error occurred");
        logger.Messages[0].ShouldContain("Code=AIHooshvare:HooshvareNotFound");
        logger.Messages[1].ShouldContain("User exception occurred");
        logger.Messages[1].ShouldContain("Code=");
        logger.Messages[1].ShouldNotContain("AIHooshvare:HooshvareNotFound");
    }

    private sealed class CaptureLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
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
