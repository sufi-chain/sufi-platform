using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.BackgroundJobs;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.BackgroundJobs;

public class TempFileCleanupWorkerTests : FileManagerApplicationTestBase<SufiFileManagerApplicationTestModule>
{
    [Fact]
    public async Task Should_Remove_Expired_Temp_Files_On_Two_Consecutive_Runs()
    {
        var expiredId = Guid.NewGuid();
        var freshId = Guid.NewGuid();
        var confirmedId = Guid.NewGuid();
        var expiredAt = DateTime.UtcNow.AddDays(-30);

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IFileItemRepository>();
            await repository.InsertAsync(CreateItem(expiredId, "expired.tmp", "blob-expired", expiredAt, isTemp: true), autoSave: true);
            await repository.InsertAsync(CreateItem(freshId, "fresh.tmp", "blob-fresh", DateTime.UtcNow, isTemp: true), autoSave: true);
            await repository.InsertAsync(CreateItem(confirmedId, "kept.bin", "blob-kept", expiredAt, isTemp: false), autoSave: true);
        });

        var disposedLog = new ObjectDisposedLog();
        GetRequiredService<ILoggerFactory>().AddProvider(disposedLog);

        var worker = new TempFileCleanupWorker(
            GetRequiredService<AbpAsyncTimer>(),
            GetRequiredService<IServiceScopeFactory>())
        {
            ServiceProvider = ServiceProvider,
            LazyServiceProvider = GetRequiredService<IAbpLazyServiceProvider>()
        };

        await RunOnceAsync(worker);
        await RunOnceAsync(worker);

        disposedLog.SawObjectDisposedException.ShouldBeFalse();

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IFileItemRepository>();
            var expired = await repository.FindAsync(expiredId);
            expired.ShouldBeNull(disposedLog.LastError ?? "expired temporary file was not removed");
            (await repository.FindAsync(freshId)).ShouldNotBeNull();
            (await repository.FindAsync(confirmedId)).ShouldNotBeNull();
        });
    }

    private async Task RunOnceAsync(TempFileCleanupWorker worker)
    {
        using var scope = ServiceProvider.CreateScope();
        var context = new PeriodicBackgroundWorkerContext(scope.ServiceProvider);
        var method = typeof(TempFileCleanupWorker).GetMethod(
            "DoWorkAsync",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(PeriodicBackgroundWorkerContext) },
            null);
        var task = (Task)method!.Invoke(worker, new object[] { context })!;
        await task;
    }

    private static FileItem CreateItem(Guid id, string name, string blobName, DateTime creationTime, bool isTemp)
    {
        var item = new FileItem(
            id,
            tenantId: null,
            name,
            name,
            blobName,
            "application/octet-stream",
            size: 4,
            FileType.Document);
        if (!isTemp)
        {
            item.Confirm();
        }

        typeof(FileItem).GetProperty(nameof(FileItem.CreationTime))!
            .SetValue(item, creationTime);
        return item;
    }

    private sealed class ObjectDisposedLog : ILoggerProvider
    {
        public bool SawObjectDisposedException { get; private set; }

        public string? LastError { get; private set; }

        public ILogger CreateLogger(string categoryName) => new Logger(this);

        public void Dispose()
        {
        }

        private sealed class Logger : ILogger
        {
            private readonly ObjectDisposedLog _owner;

            public Logger(ObjectDisposedLog owner)
            {
                _owner = owner;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel >= LogLevel.Error)
                {
                    _owner.LastError = exception?.ToString() ?? formatter(state, exception);
                }

                if (ContainsObjectDisposed(exception) || ContainsObjectDisposed(formatter(state, exception)))
                {
                    _owner.SawObjectDisposedException = true;
                }
            }

            private static bool ContainsObjectDisposed(Exception? exception)
            {
                for (var current = exception; current != null; current = current.InnerException)
                {
                    if (current is ObjectDisposedException)
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool ContainsObjectDisposed(string? text)
            {
                return text != null && text.Contains("ObjectDisposedException", StringComparison.Ordinal);
            }
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
