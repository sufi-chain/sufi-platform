using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Shouldly;
using SufiChane.SufiPlatform.Migrations.AI;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class KnowledgeMigrationTests
{
    [Fact]
    public void Upgrade_Should_Only_Add_Proposal_Table_And_Guard_Populated_Downgrade()
    {
        var migration = new AddKnowledgeRelationProposals();
        migration.UpOperations.Count.ShouldBe(4);
        migration.UpOperations.OfType<CreateTableOperation>().Single().Name.ShouldBe("SufiAI.KnowledgeRelationProposals");
        migration.UpOperations.OfType<CreateIndexOperation>().Count(x => x.IsUnique).ShouldBe(2);
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
        migration.DownOperations.First().ShouldBeOfType<SqlOperation>().Sql.ShouldContain("THROW 51000");
    }

    [Fact]
    public void SqlServer_Model_And_Scripts_Should_Match_The_AI_Migration_History()
    {
        var options = new DbContextOptionsBuilder<AIDbContext>().UseSqlServer(
            "Server=localhost;Database=KnowledgeMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(AddKnowledgeRelationProposals).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_AI")).Options;
        using var context = new AIDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var migrator = context.GetService<IMigrator>();
        var upgrade = migrator.GenerateScript("20260907181329_InitialAI", "20260916195011_AddKnowledgeRelationProposals",
            MigrationsSqlGenerationOptions.Idempotent);
        upgrade.ShouldContain("[__EFMigrationsHistory_AI]");
        upgrade.ShouldContain("CREATE TABLE [SufiAI.KnowledgeRelationProposals]");
        upgrade.ShouldNotContain("DROP TABLE");
        var rollback = migrator.GenerateScript("20260916195011_AddKnowledgeRelationProposals", "20260907181329_InitialAI");
        rollback.ShouldContain("THROW 51000");
    }

    [Fact]
    public void Upgrade_Should_Add_Application_Intent_Table_And_Guard_Populated_Downgrade()
    {
        var migration = new AddKnowledgeRelationApplicationIntents();
        migration.UpOperations.OfType<CreateTableOperation>().Single().Name.ShouldBe("SufiAI.KnowledgeRelationApplicationIntents");
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
        migration.DownOperations.First().ShouldBeOfType<SqlOperation>().Sql.ShouldContain("THROW 51000");
        var options = new DbContextOptionsBuilder<AIDbContext>().UseSqlServer(
            "Server=localhost;Database=KnowledgeMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(AddKnowledgeRelationProposals).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_AI")).Options;
        using var context = new AIDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var upgrade = context.GetService<IMigrator>().GenerateScript(
            "20260916195011_AddKnowledgeRelationProposals", "20260916204611_AddKnowledgeRelationApplicationIntents",
            MigrationsSqlGenerationOptions.Idempotent);
        upgrade.ShouldContain("CREATE TABLE [SufiAI.KnowledgeRelationApplicationIntents]");
        upgrade.ShouldContain("[__EFMigrationsHistory_AI]");
    }
}
