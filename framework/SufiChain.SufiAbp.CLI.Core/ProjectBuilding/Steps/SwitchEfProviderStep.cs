using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Switches EF Core provider from SQL Server to the selected provider.
/// Supports: SqlServer (default), PostgreSQL, MySQL, MariaDB, SQLite.
/// </summary>
public class SwitchEfProviderStep : ProjectBuildPipelineStep
{
    public override string Description => "Switching EF Core provider...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Only process if EF Core is selected
        if (context.Args.DatabaseProvider != DatabaseProvider.EntityFrameworkCore)
        {
            return Task.CompletedTask;
        }
        
        // If SQL Server (default), no changes needed
        if (!context.Args.EfProvider.HasValue || context.Args.EfProvider.Value == EfProviderKind.SqlServer)
        {
            return Task.CompletedTask;
        }
        
        var provider = context.Args.EfProvider.Value;
        var providerInfo = GetProviderInfo(provider);
        
        // Process all files
        var filesToProcess = context.Files.Keys.ToList();
        
        foreach (var filePath in filesToProcess)
        {
            if (!FileEntry.IsTextFile(filePath))
                continue;
            
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            var modified = false;
            
            // 1. Replace provider package references in .csproj files
            if (filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var updatedContent = ReplaceProviderPackages(content, providerInfo.PackageName);
                if (updatedContent != content)
                {
                    content = updatedContent;
                    modified = true;
                }
            }
            
            // 2. Replace module class in *EntityFrameworkCoreModule.cs
            if (filePath.EndsWith("EntityFrameworkCoreModule.cs", StringComparison.OrdinalIgnoreCase))
            {
                var updatedContent = ReplaceProviderModuleContent(content, providerInfo);
                if (updatedContent != content)
                {
                    content = updatedContent;
                    if (provider == EfProviderKind.MySQL || provider == EfProviderKind.MariaDB)
                    {
                        content = AddMySqlServerVersion(content, providerInfo.UseMethod);
                    }
                    
                    modified = true;
                }
            }
            
            // 3. Replace provider method in DbContext files
            if (filePath.EndsWith("DbContext.cs", StringComparison.OrdinalIgnoreCase))
            {
                var updatedContent = ReplaceProviderUseMethod(content, providerInfo.UseMethod);
                if (updatedContent != content)
                {
                    content = updatedContent;
                    if (provider == EfProviderKind.MySQL || provider == EfProviderKind.MariaDB)
                    {
                        content = AddMySqlServerVersion(content, providerInfo.UseMethod);
                    }
                    
                    modified = true;
                }
            }
            
            // 4. Replace provider method in DbContextFactory files
            if (filePath.EndsWith("DbContextFactory.cs", StringComparison.OrdinalIgnoreCase))
            {
                var updatedContent = ReplaceProviderUseMethod(content, providerInfo.UseMethod);
                if (updatedContent != content)
                {
                    content = updatedContent;
                    if (provider == EfProviderKind.MySQL || provider == EfProviderKind.MariaDB)
                    {
                        content = AddMySqlServerVersion(content, providerInfo.UseMethod);
                    }
                    
                    modified = true;
                }
            }
            
            if (modified)
            {
                context.Files[filePath] = Encoding.UTF8.GetBytes(content);
            }
        }
        
        return Task.CompletedTask;
    }

    private static string ReplaceProviderPackages(string content, string packageName)
    {
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"Volo\.Abp\.EntityFrameworkCore\.(SqlServer|PostgreSql|MySQL|Sqlite)",
            packageName);

        return System.Text.RegularExpressions.Regex.Replace(
            content,
            @"SufiChain\.SufiAbp\.EntityFrameworkCore\.(SqlServer|PostgreSql|MySQL|Sqlite)",
            packageName);
    }

    private static string ReplaceProviderModuleContent(string content, EfProviderInfo providerInfo)
    {
        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"using Volo\.Abp\.EntityFrameworkCore\.(SqlServer|PostgreSql|MySQL|Sqlite);",
            $"using {providerInfo.ModuleNamespace};");

        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"using SufiChain\.SufiAbp\.EntityFrameworkCore\.(SqlServer|PostgreSql|MySQL|Sqlite);",
            $"using {providerInfo.ModuleNamespace};");

        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"typeof\(AbpEntityFrameworkCore(SqlServer|PostgreSql|MySQL|Sqlite)Module\)",
            $"typeof({providerInfo.ModuleClassName})");

        content = System.Text.RegularExpressions.Regex.Replace(
            content,
            @"typeof\(SufiAbpEntityFrameworkCore(SqlServer|PostgreSql|MySQL|Sqlite)Module\)",
            $"typeof({providerInfo.ModuleClassName})");

        return ReplaceProviderUseMethod(content, providerInfo.UseMethod);
    }

    private static string ReplaceProviderUseMethod(string content, string useMethod)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            content,
            @"Use(SqlServer|Npgsql|MySQL|MySql|Sqlite)\(",
            $"{useMethod}(");
    }
    
    /// <summary>
    /// Adds MySqlServerVersion parameter to UseMySql calls.
    /// </summary>
    private string AddMySqlServerVersion(string content, string useMethod)
    {
        // Pattern: UseMySQL(connectionString) or UseMySql(connectionString)
        // Replace with: UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion)
        
        // Find all occurrences of UseMySQL( or UseMySql(
        var lines = content.Split('\n');
        var result = new List<string>();
        
        foreach (var line in lines)
        {
            if (line.Contains($"{useMethod}(") && !line.Contains("MySqlServerVersion"))
            {
                // Check if it's a simple single-line call
                if (line.Contains(");"))
                {
                    // Replace ); with , MySqlServerVersion.LatestSupportedServerVersion);
                    var modified = line.Replace(");", ", MySqlServerVersion.LatestSupportedServerVersion);");
                    result.Add(modified);
                }
                else
                {
                    result.Add(line);
                }
            }
            else
            {
                result.Add(line);
            }
        }
        
        return string.Join('\n', result);
    }
    
    /// <summary>
    /// Gets provider information for the specified EF Core provider.
    /// </summary>
    private EfProviderInfo GetProviderInfo(EfProviderKind provider)
    {
        return provider switch
        {
            EfProviderKind.SqlServer => new EfProviderInfo
            {
                PackageName = "SufiChain.SufiAbp.EntityFrameworkCore.SqlServer",
                ModuleNamespace = "SufiChain.SufiAbp.EntityFrameworkCore.SqlServer",
                ModuleClassName = "SufiAbpEntityFrameworkCoreSqlServerModule",
                UseMethod = "UseSqlServer"
            },
            EfProviderKind.PostgreSQL => new EfProviderInfo
            {
                PackageName = "SufiChain.SufiAbp.EntityFrameworkCore.PostgreSql",
                ModuleNamespace = "SufiChain.SufiAbp.EntityFrameworkCore.PostgreSql",
                ModuleClassName = "SufiAbpEntityFrameworkCorePostgreSqlModule",
                UseMethod = "UseNpgsql"
            },
            EfProviderKind.MySQL => new EfProviderInfo
            {
                PackageName = "SufiChain.SufiAbp.EntityFrameworkCore.MySQL",
                ModuleNamespace = "SufiChain.SufiAbp.EntityFrameworkCore.MySQL",
                ModuleClassName = "SufiAbpEntityFrameworkCoreMySQLModule",
                UseMethod = "UseMySql"
            },
            EfProviderKind.MariaDB => new EfProviderInfo
            {
                PackageName = "SufiChain.SufiAbp.EntityFrameworkCore.MySQL",
                ModuleNamespace = "SufiChain.SufiAbp.EntityFrameworkCore.MySQL",
                ModuleClassName = "SufiAbpEntityFrameworkCoreMySQLModule",
                UseMethod = "UseMySql"
            },
            EfProviderKind.Sqlite => new EfProviderInfo
            {
                PackageName = "SufiChain.SufiAbp.EntityFrameworkCore.Sqlite",
                ModuleNamespace = "SufiChain.SufiAbp.EntityFrameworkCore.Sqlite",
                ModuleClassName = "SufiAbpEntityFrameworkCoreSqliteModule",
                UseMethod = "UseSqlite"
            },
            _ => throw new ArgumentException($"Unsupported EF Core provider: {provider}")
        };
    }
}

/// <summary>
/// EF Core provider information.
/// </summary>
internal class EfProviderInfo
{
    public required string PackageName { get; init; }
    public required string ModuleNamespace { get; init; }
    public required string ModuleClassName { get; init; }
    public required string UseMethod { get; init; }
}
