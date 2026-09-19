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
    public void Initial_Should_Create_Knowledge_Relation_Tables()
    {
        var migration = new InitialAI();
        var tables = migration.UpOperations.OfType<CreateTableOperation>().Select(x => x.Name).ToList();
        tables.ShouldContain("SufiAI.KnowledgeRelationProposals");
        tables.ShouldContain("SufiAI.KnowledgeRelationApplicationIntents");
        migration.UpOperations.ShouldNotContain(x => x is DropTableOperation || x is DropColumnOperation);
    }

    [Fact]
    public void SqlServer_Model_Should_Match_The_Single_AI_Migration()
    {
        var options = new DbContextOptionsBuilder<AIDbContext>().UseSqlServer(
            "Server=localhost;Database=KnowledgeMigrationModelOnly;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly(typeof(InitialAI).Assembly.GetName().Name)
                .MigrationsHistoryTable("__EFMigrationsHistory_AI")).Options;
        using var context = new AIDbContext(options);
        context.Database.HasPendingModelChanges().ShouldBeFalse();
        var script = context.GetService<IMigrator>().GenerateScript(
            fromMigration: null,
            toMigration: "20260907181329_InitialAI",
            MigrationsSqlGenerationOptions.Idempotent);
        script.ShouldContain("[__EFMigrationsHistory_AI]");
        script.ShouldContain("CREATE TABLE [SufiAI.KnowledgeRelationProposals]");
        script.ShouldContain("CREATE TABLE [SufiAI.KnowledgeRelationApplicationIntents]");
        script.ShouldNotContain("DROP TABLE");
    }
}
