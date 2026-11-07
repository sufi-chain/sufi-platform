using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: host charges and quota increments follow the SQL unique index and an atomic update.
/// </summary>
public class HostUsageCounterConcurrencyTests
{
    [Fact]
    public void The_sql_server_model_keeps_a_host_unique_index()
    {
        ChatUsageCounterStore.SqlServerHostFilter.ShouldBe("[TenantId] IS NULL");
        var options = new DbContextOptionsBuilder<UsageModelContext>()
            .UseSqlServer("Server=127.0.0.1;Database=unused;TrustServerCertificate=True")
            .Options;
        using var db = new UsageModelContext(options);
        var indexes = db.Model.FindEntityType(typeof(ChatUsageCounter))!.GetIndexes().Where(index => index.IsUnique).ToList();
        indexes.ShouldContain(index => index.GetFilter() == "[TenantId] IS NOT NULL");
        indexes.ShouldContain(index =>
            index.GetDatabaseName() == "IX_SufiCom.Chat.UsageDailyAggregates_HostCounter" &&
            index.GetFilter() == "[TenantId] IS NULL");
    }

    [Fact]
    public async Task Two_synchronized_host_charges_leave_one_row()
    {
        await using var db = await UsageDatabase.OpenAsync();
        var chargeKey = ChatUsageGuard.AssistantPartChargeKey(Guid.NewGuid(), 0);
        var barrier = new Barrier(2);

        var results = await Task.WhenAll(
            InsertHostChargeAsync(db.ConnectionString, chargeKey, barrier),
            InsertHostChargeAsync(db.ConnectionString, chargeKey, barrier));

        results.Count(added => added).ShouldBe(1);
        await using var verify = db.Create();
        (await verify.Set<ChatUsageCounter>().CountAsync(row => row.CounterKey == chargeKey && row.TenantId == null)).ShouldBe(1);
    }

    [Fact]
    public async Task Two_different_operations_increment_the_same_host_counter_together()
    {
        await using var db = await UsageDatabase.OpenAsync();
        var counterKey = "Session:" + Guid.NewGuid().ToString("D") + ":Messages";
        await using (var seed = db.Create())
        {
            await ChatUsageCounterStore.IncrementAsync(
                seed,
                tenantId: null,
                counterKey,
                ChatUsageCounterPeriod.Session,
                DateTime.MinValue,
                DateTime.MaxValue,
                count: 1,
                tokenCount: 0,
                Guid.NewGuid(),
                beforeMutation: null,
                CancellationToken.None);
        }

        string? incrementSql = null;
        var barrier = new Barrier(2);
        await Task.WhenAll(
            IncrementAsync(db.ConnectionString, counterKey, barrier, sql => incrementSql = sql),
            IncrementAsync(db.ConnectionString, counterKey, barrier, _ => { }));

        await using var verify = db.Create();
        var count = await verify.Set<ChatUsageCounter>()
            .Where(row => row.TenantId == null && row.CounterKey == counterKey)
            .Select(row => row.Count)
            .SingleAsync();
        count.ShouldBe(3);
        incrementSql.ShouldNotBeNull();
        incrementSql.ShouldContain("+");
        incrementSql.ShouldContain("Count");
    }

    private static async Task<bool> InsertHostChargeAsync(string connectionString, string chargeKey, Barrier barrier)
    {
        await using var db = UsageDatabase.Connect(connectionString);
        return await ChatUsageCounterStore.InsertChargeAsync(
            db,
            tenantId: null,
            chargeKey,
            Guid.NewGuid(),
            beforeInsert: () =>
            {
                barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                return Task.CompletedTask;
            },
            CancellationToken.None);
    }

    private static async Task IncrementAsync(string connectionString, string counterKey, Barrier barrier, Action<string?> capture)
    {
        var interceptor = new IncrementSqlCapture();
        await using var db = UsageDatabase.Connect(connectionString, interceptor);
        await ChatUsageCounterStore.IncrementAsync(
            db,
            tenantId: null,
            counterKey,
            ChatUsageCounterPeriod.Session,
            DateTime.MinValue,
            DateTime.MaxValue,
            count: 1,
            tokenCount: 0,
            Guid.NewGuid(),
            beforeMutation: () =>
            {
                barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                return Task.CompletedTask;
            },
            CancellationToken.None);
        capture(interceptor.Sql);
    }

    private sealed class UsageModelContext : DbContext
    {
        public UsageModelContext(DbContextOptions<UsageModelContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureSufiComChat(Database.ProviderName);
        }
    }

    private sealed class UsageDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _keepAlive;
        public string ConnectionString { get; }

        private UsageDatabase(SqliteConnection keepAlive, string connectionString)
        {
            _keepAlive = keepAlive;
            ConnectionString = connectionString;
        }

        public static async Task<UsageDatabase> OpenAsync()
        {
            var connectionString = "Data Source=hostusage-" + Guid.NewGuid().ToString("N") + ";Mode=Memory;Cache=Shared";
            var keepAlive = new SqliteConnection(connectionString);
            await keepAlive.OpenAsync();
            var database = new UsageDatabase(keepAlive, connectionString);
            await using var setup = database.Create();
            await setup.Database.EnsureCreatedAsync();
            return database;
        }

        public UsageModelContext Create(DbCommandInterceptor? interceptor = null)
        {
            var builder = new DbContextOptionsBuilder<UsageModelContext>().UseSqlite(ConnectionString);
            if (interceptor != null)
            {
                builder.AddInterceptors(interceptor);
            }

            return new UsageModelContext(builder.Options);
        }

        public static UsageModelContext Connect(string connectionString, DbCommandInterceptor? interceptor = null)
        {
            var builder = new DbContextOptionsBuilder<UsageModelContext>().UseSqlite(connectionString);
            if (interceptor != null)
            {
                builder.AddInterceptors(interceptor);
            }

            return new UsageModelContext(builder.Options);
        }

        public ValueTask DisposeAsync()
        {
            return _keepAlive.DisposeAsync();
        }
    }

    private sealed class IncrementSqlCapture : DbCommandInterceptor
    {
        public string? Sql { get; private set; }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Remember(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Remember(command);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void Remember(DbCommand command)
        {
            if (command.CommandText.Contains("Count", StringComparison.Ordinal))
            {
                Sql = command.CommandText;
            }
        }
    }
}
