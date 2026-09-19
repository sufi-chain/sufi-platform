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
    public void Upgrade_Should_Preserve_Classification_And_Only_Add_Tags_Schema()
    {
        var migration = new AddTagsRelations();
        var columns = migration.UpOperations.OfType<AddColumnOperation>().ToList();
        columns.Count.ShouldBe(2);
        columns.Single(x => x.Name == "Kind").DefaultValue.ShouldBe(0);
        columns.Single(x => x.Name == "StableKey").IsNullable.ShouldBeTrue();
        migration.UpOperations.OfType<CreateTableOperation>().Select(x => x.Name).OrderBy(x => x)
            .ShouldBe(new[] { "SufiTags.RelationDefinitions", "SufiTags.RelationRevisions", "SufiTags.Relations" }.OrderBy(x => x));
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
        migration.DownOperations.OfType<SqlOperation>().First().Sql.ShouldContain("THROW 51000");
    }

    [Fact]
    public void SqlServer_Snapshot_Should_Match_Model_And_Scripts_Should_Use_Tags_History()
    {
        var options = new DbContextOptionsBuilder<TagsDbContext>().UseSqlServer(
            "Server=localhost;Database=TagsMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(AddTagsRelations).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_Tags")).Options;
        using var context = new TagsDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var migrator = context.GetService<IMigrator>();
        var upgrade = migrator.GenerateScript("20260825171531_InitialTags", "20260916191244_AddTagsRelations",
            MigrationsSqlGenerationOptions.Idempotent);
        upgrade.ShouldContain("[__EFMigrationsHistory_Tags]");
        upgrade.ShouldContain("ADD [Kind] int NOT NULL DEFAULT 0");
        upgrade.ShouldNotContain("DROP TABLE");
        var rollback = migrator.GenerateScript("20260916191244_AddTagsRelations", "20260825171531_InitialTags");
        rollback.ShouldContain("THROW 51000");
    }

    [Fact]
    public void Upgrade_Should_Add_Receipt_Table_And_Guard_Populated_Downgrade()
    {
        var migration = new AddTagRelationMutationReceipts();
        migration.UpOperations.OfType<CreateTableOperation>().Single().Name.ShouldBe("SufiTags.RelationMutationReceipts");
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
        migration.DownOperations.First().ShouldBeOfType<SqlOperation>().Sql.ShouldContain("THROW 51000");
        var options = new DbContextOptionsBuilder<TagsDbContext>().UseSqlServer(
            "Server=localhost;Database=TagsMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(AddTagsRelations).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_Tags")).Options;
        using var context = new TagsDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var upgrade = context.GetService<IMigrator>().GenerateScript(
            "20260916191244_AddTagsRelations", "20260916204920_AddTagRelationMutationReceipts",
            MigrationsSqlGenerationOptions.Idempotent);
        upgrade.ShouldContain("CREATE TABLE [SufiTags.RelationMutationReceipts]");
        upgrade.ShouldContain("[__EFMigrationsHistory_Tags]");
    }
}
