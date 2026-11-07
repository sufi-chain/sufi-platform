using System.IO;
using Microsoft.Data.Sqlite;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

/// <summary>
/// sufi-chain#551: an existing database can already contain duplicate host usage rows.
/// The migration has to collapse them before the unique index is created.
/// </summary>
public class HostUsageCounterIndexUpgradeTests
{
    [Fact]
    public async Task Upgrade_collapses_seeded_host_duplicates_then_the_index_rejects_another()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await ExecuteAsync(connection, """
            CREATE TABLE "SufiCom.Chat.UsageDailyAggregates" (
                Id TEXT NOT NULL PRIMARY KEY,
                TenantId TEXT NULL,
                CounterKey TEXT NOT NULL,
                Period INTEGER NOT NULL,
                PeriodStart TEXT NOT NULL,
                PeriodEnd TEXT NOT NULL,
                Count INTEGER NOT NULL,
                TokenCount INTEGER NOT NULL
            );
            INSERT INTO "SufiCom.Chat.UsageDailyAggregates" VALUES
                ('a', NULL, 'AssistantOperation:one:0', 1, 'min', 'max', 2, 4),
                ('b', NULL, 'AssistantOperation:one:0', 1, 'min', 'max', 3, 3),
                ('c', NULL, 'Session:other:Messages', 1, 'min', 'max', 8, 1),
                ('d', 'tenant-1', 'AssistantOperation:one:0', 1, 'min', 'max', 9, 9);
            """);

        await ExecuteAsync(connection, HostUsageCounterIndexMigration.ReconcileSql("Microsoft.EntityFrameworkCore.Sqlite"));
        await ExecuteAsync(
            connection,
            $"""
            CREATE UNIQUE INDEX "{HostUsageCounterIndexMigration.IndexName}"
            ON "SufiCom.Chat.UsageDailyAggregates" (CounterKey, Period, PeriodStart)
            WHERE {HostUsageCounterIndexMigration.Filter("Microsoft.EntityFrameworkCore.Sqlite")};
            """);

        var rows = await ReadAsync(connection, """
            SELECT Id, TenantId, CounterKey, Count, TokenCount
            FROM "SufiCom.Chat.UsageDailyAggregates"
            ORDER BY Id;
            """);
        rows.ShouldBe(new[]
        {
            ("a", null, "AssistantOperation:one:0", 5L, 7L),
            ("c", null, "Session:other:Messages", 8L, 1L),
            ("d", "tenant-1", "AssistantOperation:one:0", 9L, 9L)
        });

        var duplicate = await Should.ThrowAsync<SqliteException>(() => ExecuteAsync(connection, """
            INSERT INTO "SufiCom.Chat.UsageDailyAggregates" VALUES
                ('e', NULL, 'AssistantOperation:one:0', 1, 'min', 'max', 1, 0);
            """));
        duplicate.SqliteErrorCode.ShouldBe(19);
    }

    [Fact]
    public void The_migration_reconciles_before_it_creates_the_index()
    {
        var migration = ReadMigration();
        var sqlAt = migration.IndexOf("migrationBuilder.Sql(", StringComparison.Ordinal);
        var indexAt = migration.IndexOf("migrationBuilder.CreateIndex(", StringComparison.Ordinal);
        sqlAt.ShouldBeGreaterThan(-1);
        indexAt.ShouldBeGreaterThan(sqlAt);
        migration.ShouldContain("HostUsageCounterIndexMigration.ReconcileSql");
        HostUsageCounterIndexMigration.ReconcileSql("Microsoft.EntityFrameworkCore.SqlServer")
            .ShouldContain("DELETE");
        HostUsageCounterIndexMigration.Filter("Microsoft.EntityFrameworkCore.SqlServer")
            .ShouldBe("[TenantId] IS NULL");
    }

    private static string ReadMigration()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(
                dir.FullName,
                "hosts",
                "SufiChane.SufiPlatform",
                "src",
                "SufiChane.SufiPlatform.EntityFrameworkCore",
                "Migrations",
                "ComChat",
                "20261107160000_AddHostChatUsageCounterUniqueIndex.cs");
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("Host usage index migration was not found above " + AppContext.BaseDirectory);
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<(string Id, string? TenantId, string CounterKey, long Count, long TokenCount)[]> ReadAsync(
        SqliteConnection connection,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<(string, string?, string, long, long)>();
        while (await reader.ReadAsync())
        {
            rows.Add((
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.GetString(2),
                reader.GetInt64(3),
                reader.GetInt64(4)));
        }

        return rows.ToArray();
    }
}
