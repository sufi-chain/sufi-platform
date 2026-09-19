using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Shouldly;
using SufiChane.SufiPlatform.Migrations.Tags;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

/// <summary>Validates the actual host artifacts and SQL generation without opening a database connection.</summary>
public class TagsMigrationTests
{
    [Fact]
    public void Initial_Should_Create_Classification_And_Relation_Schema()
    {
        var migration = new InitialTags();
        var tables = migration.UpOperations.OfType<CreateTableOperation>().Select(x => x.Name).ToList();
        tables.ShouldContain("SufiTags.Tags");
        tables.ShouldContain("SufiTags.RelationDefinitions");
        tables.ShouldContain("SufiTags.Relations");
        tables.ShouldContain("SufiTags.RelationRevisions");
        tables.ShouldContain("SufiTags.RelationMutationReceipts");
        var tags = migration.UpOperations.OfType<CreateTableOperation>().Single(x => x.Name == "SufiTags.Tags");
        tags.Columns.Single(x => x.Name == "Kind").DefaultValue.ShouldBe(0);
        tags.Columns.Single(x => x.Name == "StableKey").IsNullable.ShouldBeTrue();
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
    }

    [Fact]
    public void SqlServer_Snapshot_Should_Match_Model_And_Scripts_Should_Use_Tags_History()
    {
        var options = new DbContextOptionsBuilder<TagsDbContext>().UseSqlServer(
            "Server=localhost;Database=TagsMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(InitialTags).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_Tags")).Options;
        using var context = new TagsDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var script = context.GetService<IMigrator>().GenerateScript(
            fromMigration: null,
            toMigration: "20260825171531_InitialTags",
            MigrationsSqlGenerationOptions.Idempotent);
        script.ShouldContain("[__EFMigrationsHistory_Tags]");
        script.ShouldContain("CREATE TABLE [SufiTags.Relations]");
        script.ShouldContain("CREATE TABLE [SufiTags.RelationMutationReceipts]");
        script.ShouldNotContain("DROP TABLE");
    }
}
