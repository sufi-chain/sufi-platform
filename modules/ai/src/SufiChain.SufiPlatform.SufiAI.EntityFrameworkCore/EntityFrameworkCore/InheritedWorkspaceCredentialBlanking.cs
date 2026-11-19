namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

/// <summary>
/// Idempotent statements that drop copied host credentials from inherited projections.
/// Host scope and tenant scope share one statement: it matches every inherited row in
/// the database being migrated, including a shared database that stores many tenants.
/// Down does not restore the keys.
/// </summary>
public static class InheritedWorkspaceCredentialBlanking
{
    public const string WorkspaceSql =
        "UPDATE [SufiAI.Workspaces] SET [ApiKey] = NULL WHERE [IsInherited] = 1 AND [ApiKey] IS NOT NULL AND [ApiKey] <> ''";

    public const string RouteSql =
        "UPDATE [SufiAI.ModelConfigurations] SET [ApiKey] = NULL WHERE [ApiKey] IS NOT NULL AND [ApiKey] <> '' AND [WorkspaceId] IN (SELECT [Id] FROM [SufiAI.Workspaces] WHERE [IsInherited] = 1)";
}
