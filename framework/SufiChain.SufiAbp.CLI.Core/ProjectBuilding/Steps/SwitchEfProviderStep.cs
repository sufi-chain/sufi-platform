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
            
            // 1. Replace package reference in .csproj files
            if (filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                if (content.Contains("Volo.Abp.EntityFrameworkCore.SqlServer"))
                {
                    content = content.Replace(
                        "Volo.Abp.EntityFrameworkCore.SqlServer",
                        providerInfo.PackageName);
                    modified = true;
                }
            }
            
            // 2. Replace module class in *EntityFrameworkCoreModule.cs
            if (filePath.EndsWith("EntityFrameworkCoreModule.cs", StringComparison.OrdinalIgnoreCase))
            {
                // Replace using statement
                if (content.Contains("using Volo.Abp.EntityFrameworkCore.SqlServer;"))
                {
                    content = content.Replace(
                        "using Volo.Abp.EntityFrameworkCore.SqlServer;",
                        $"using {providerInfo.ModuleNamespace};");
                    modified = true;
                }
                
                // Replace DependsOn attribute
                if (content.Contains("typeof(AbpEntityFrameworkCoreSqlServerModule)"))
                {
                    content = content.Replace(
                        "typeof(AbpEntityFrameworkCoreSqlServerModule)",
                        $"typeof({providerInfo.ModuleClassName})");
                    modified = true;
                }
                
                // Replace UseSqlServer in Configure method
                if (content.Contains("UseSqlServer("))
                {
                    content = content.Replace("UseSqlServer(", $"{providerInfo.UseMethod}(");
                    
                    // Special case for MySQL: Add MySqlServerVersion parameter
                    if (provider == EfProviderKind.MySQL || provider == EfProviderKind.MariaDB)
                    {
                        content = AddMySqlServerVersion(content, providerInfo.UseMethod);
                    }
                    
                    modified = true;
                }
            }
            
            // 3. Replace UseSqlServer in DbContext files
            if (filePath.EndsWith("DbContext.cs", StringComparison.OrdinalIgnoreCase))
            {
                if (content.Contains("UseSqlServer("))
                {
                    content = content.Replace("UseSqlServer(", $"{providerInfo.UseMethod}(");
                    
                    // Special case for MySQL: Add MySqlServerVersion parameter
                    if (provider == EfProviderKind.MySQL || provider == EfProviderKind.MariaDB)
                    {
                        content = AddMySqlServerVersion(content, providerInfo.UseMethod);
                    }
                    
                    modified = true;
                }
            }
            
            // 4. Replace UseSqlServer in DbContextFactory files
            if (filePath.EndsWith("DbContextFactory.cs", StringComparison.OrdinalIgnoreCase))
            {
                if (content.Contains("UseSqlServer("))
                {
                    content = content.Replace("UseSqlServer(", $"{providerInfo.UseMethod}(");
                    
                    // Special case for MySQL: Add MySqlServerVersion parameter
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
                PackageName = "Volo.Abp.EntityFrameworkCore.SqlServer",
                ModuleNamespace = "Volo.Abp.EntityFrameworkCore.SqlServer",
                ModuleClassName = "AbpEntityFrameworkCoreSqlServerModule",
                UseMethod = "UseSqlServer"
            },
            EfProviderKind.PostgreSQL => new EfProviderInfo
            {
                PackageName = "Volo.Abp.EntityFrameworkCore.PostgreSql",
                ModuleNamespace = "Volo.Abp.EntityFrameworkCore.PostgreSql",
                ModuleClassName = "AbpEntityFrameworkCorePostgreSqlModule",
                UseMethod = "UseNpgsql"
            },
            EfProviderKind.MySQL => new EfProviderInfo
            {
                PackageName = "Volo.Abp.EntityFrameworkCore.MySQL",
                ModuleNamespace = "Volo.Abp.EntityFrameworkCore.MySQL",
                ModuleClassName = "AbpEntityFrameworkCoreMySQLModule",
                UseMethod = "UseMySql"
            },
            EfProviderKind.MariaDB => new EfProviderInfo
            {
                PackageName = "Volo.Abp.EntityFrameworkCore.MySQL",
                ModuleNamespace = "Volo.Abp.EntityFrameworkCore.MySQL",
                ModuleClassName = "AbpEntityFrameworkCoreMySQLModule",
                UseMethod = "UseMySql"
            },
            EfProviderKind.Sqlite => new EfProviderInfo
            {
                PackageName = "Volo.Abp.EntityFrameworkCore.Sqlite",
                ModuleNamespace = "Volo.Abp.EntityFrameworkCore.Sqlite",
                ModuleClassName = "AbpEntityFrameworkCoreSqliteModule",
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
