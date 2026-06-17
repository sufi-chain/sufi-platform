using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the solution for Entity Framework Core database provider.
/// Supports all <see cref="EfProviderKind"/> sub-providers (SqlServer, PostgreSQL, MySQL, MariaDB, Sqlite).
/// </summary>
public class SwitchToEfCoreStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring for Entity Framework Core...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (context.Args.DatabaseProvider != DatabaseProvider.EntityFrameworkCore)
            return Task.CompletedTask;

        var provider = context.Args.EfProvider ?? EfProviderKind.SqlServer;
        var isSingle = context.Args.SolutionKind == SolutionKind.WebApp;

        if (isSingle)
        {
            // WebApp template: replace embedded MongoDB with EF Core (no separate DB project)
            ConfigureSingleTemplateForEfCore(context, provider);
        }
        else
        {
            // Layered/tiered: remove MongoDB project, create EF Core project
            var mongoProjectName = $"{context.Args.SolutionName}.MongoDB";
            context.ProjectsToRemove.Add(mongoProjectName);

            CreateEfCoreProject(context, provider);
            AddEfCoreProjectToSolution(context);
            UpdateHostProjectReferences(context);
            UpdateModuleDependencies(context);
        }

        // Switch SufiAbp module DB packages (e.g. FileManager.MongoDB -> FileManager.EntityFrameworkCore)
        SwitchSpModuleDbPackages(context);
        
        // Swap ABP infrastructure MongoDB packages to EF Core in host .csproj files
        SwitchAbpInfraPackagesInHostCsproj(context);
        
        // Inject ABP EF Core module typeof() and using statements (replacing removed MongoDB refs)
        InjectAbpEfCoreModuleDependencies(context);
        
        // Update connection strings
        UpdateConnectionStrings(context, provider);
        
        return Task.CompletedTask;
    }

    #region Provider Mappings

    /// <summary>
    /// Gets the ABP EF Core provider NuGet package name.
    /// </summary>
    private static string GetAbpProviderPackage(EfProviderKind provider) => provider switch
    {
        EfProviderKind.SqlServer => "Volo.Abp.EntityFrameworkCore.SqlServer",
        EfProviderKind.PostgreSQL => "Volo.Abp.EntityFrameworkCore.PostgreSql",
        EfProviderKind.MySQL => "Volo.Abp.EntityFrameworkCore.MySQL",
        EfProviderKind.MariaDB => "Volo.Abp.EntityFrameworkCore.MySQL",
        EfProviderKind.Sqlite => "Volo.Abp.EntityFrameworkCore.Sqlite",
        _ => "Volo.Abp.EntityFrameworkCore.SqlServer"
    };

    /// <summary>
    /// Gets the ABP EF Core provider module class name.
    /// </summary>
    private static string GetAbpProviderModuleName(EfProviderKind provider) => provider switch
    {
        EfProviderKind.SqlServer => "AbpEntityFrameworkCoreSqlServerModule",
        EfProviderKind.PostgreSQL => "AbpEntityFrameworkCorePostgreSqlModule",
        EfProviderKind.MySQL => "AbpEntityFrameworkCoreMySQLModule",
        EfProviderKind.MariaDB => "AbpEntityFrameworkCoreMySQLModule",
        EfProviderKind.Sqlite => "AbpEntityFrameworkCoreSqliteModule",
        _ => "AbpEntityFrameworkCoreSqlServerModule"
    };

    /// <summary>
    /// Gets the ABP EF Core provider namespace.
    /// </summary>
    private static string GetAbpProviderNamespace(EfProviderKind provider) => provider switch
    {
        EfProviderKind.SqlServer => "Volo.Abp.EntityFrameworkCore.SqlServer",
        EfProviderKind.PostgreSQL => "Volo.Abp.EntityFrameworkCore.PostgreSql",
        EfProviderKind.MySQL => "Volo.Abp.EntityFrameworkCore.MySQL",
        EfProviderKind.MariaDB => "Volo.Abp.EntityFrameworkCore.MySQL",
        EfProviderKind.Sqlite => "Volo.Abp.EntityFrameworkCore.Sqlite",
        _ => "Volo.Abp.EntityFrameworkCore.SqlServer"
    };

    /// <summary>
    /// Gets the ABP DbContextOptions Use*() method name.
    /// </summary>
    private static string GetUseProviderMethod(EfProviderKind provider) => provider switch
    {
        EfProviderKind.SqlServer => "UseSqlServer",
        EfProviderKind.PostgreSQL => "UseNpgsql",
        EfProviderKind.MySQL => "UseMySQL",
        EfProviderKind.MariaDB => "UseMySQL",
        EfProviderKind.Sqlite => "UseSqlite",
        _ => "UseSqlServer"
    };

    /// <summary>
    /// Gets the DbContextOptionsBuilder Use*() call for design-time factory.
    /// </summary>
    private static string GetDbContextFactoryBuilderCall(EfProviderKind provider, string connectionStringExpr) => provider switch
    {
        EfProviderKind.SqlServer => $".UseSqlServer({connectionStringExpr})",
        EfProviderKind.PostgreSQL => $".UseNpgsql({connectionStringExpr})",
        EfProviderKind.MySQL => $".UseMySql({connectionStringExpr}, ServerVersion.AutoDetect({connectionStringExpr}))",
        EfProviderKind.MariaDB => $".UseMySql({connectionStringExpr}, ServerVersion.AutoDetect({connectionStringExpr}))",
        EfProviderKind.Sqlite => $".UseSqlite({connectionStringExpr})",
        _ => $".UseSqlServer({connectionStringExpr})"
    };

    /// <summary>
    /// Gets the default connection string template for a provider.
    /// {db} is replaced with the actual database name.
    /// </summary>
    private static string GetDefaultConnectionString(EfProviderKind provider, string dbName) => provider switch
    {
        EfProviderKind.SqlServer => $"Server=(localdb)\\\\MSSQLLocalDB;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True",
        EfProviderKind.PostgreSQL => $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres",
        EfProviderKind.MySQL => $"Server=localhost;Port=3306;Database={dbName};Uid=root;Pwd=",
        EfProviderKind.MariaDB => $"Server=localhost;Port=3306;Database={dbName};Uid=root;Pwd=",
        EfProviderKind.Sqlite => $"Data Source={dbName}.db",
        _ => $"Server=(localdb)\\\\MSSQLLocalDB;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True"
    };

    /// <summary>
    /// Gets additional NuGet packages needed for the design-time factory (MySQL/MariaDB needs Pomelo).
    /// </summary>
    private static string GetAdditionalDesignTimePackages(EfProviderKind provider) => provider switch
    {
        EfProviderKind.MySQL or EfProviderKind.MariaDB =>
            @"    <PackageReference Include=""Pomelo.EntityFrameworkCore.MySql"" Version=""9.0.0"" />",
        _ => ""
    };

    /// <summary>
    /// Gets additional usings needed for the design-time factory.
    /// </summary>
    private static string GetFactoryAdditionalUsings(EfProviderKind provider) => provider switch
    {
        EfProviderKind.MySQL or EfProviderKind.MariaDB => "\nusing Microsoft.EntityFrameworkCore;\nusing Pomelo.EntityFrameworkCore.MySql.Infrastructure;",
        _ => ""
    };

    #endregion

    private void CreateEfCoreProject(ProjectBuildContext context, EfProviderKind provider)
    {
        var solutionName = context.Args.SolutionName;
        var projectName = context.Args.ProjectName;
        var projectFolder = $"src\\{solutionName}.EntityFrameworkCore";
        var hasFileManager = context.Symbols.Contains("module:file-manager");

        // Create .csproj file
        var csprojContent = GenerateCsprojContent(solutionName, provider, hasFileManager);
        context.Files[$"{projectFolder}\\{solutionName}.EntityFrameworkCore.csproj"] = Encoding.UTF8.GetBytes(csprojContent);

        // Create DbContext
        var dbContextContent = GenerateDbContextContent(solutionName, projectName, hasFileManager);
        context.Files[$"{projectFolder}\\{projectName}DbContext.cs"] = Encoding.UTF8.GetBytes(dbContextContent);

        // Create Module
        var moduleContent = GenerateModuleContent(solutionName, projectName, provider, hasFileManager);
        context.Files[$"{projectFolder}\\{projectName}EntityFrameworkCoreModule.cs"] = Encoding.UTF8.GetBytes(moduleContent);

        // Create DbContextFactory (for migrations)
        var factoryContent = GenerateDbContextFactoryContent(solutionName, projectName, provider);
        context.Files[$"{projectFolder}\\{projectName}DbContextFactory.cs"] = Encoding.UTF8.GetBytes(factoryContent);

        // Create DbSchemaMigrator
        var migratorContent = GenerateDbSchemaMigratorContent(solutionName, projectName);
        context.Files[$"{projectFolder}\\EntityFrameworkCore{projectName}DbSchemaMigrator.cs"] = Encoding.UTF8.GetBytes(migratorContent);

        // Create empty Migrations folder marker
        context.Files[$"{projectFolder}\\Migrations\\.gitkeep"] = Array.Empty<byte>();
    }

    /// <summary>
    /// Configures the WebApp template for EF Core: replaces embedded MongoDB DbContext with EF Core version,
    /// adds EF Core packages, and injects ConfigureEfCore in the module.
    /// </summary>
    private void ConfigureSingleTemplateForEfCore(ProjectBuildContext context, EfProviderKind provider)
    {
        var solutionName = context.Args.SolutionName;
        var projectName = context.Args.ProjectName;
        var providerPackage = GetAbpProviderPackage(provider);
        var providerModuleName = GetAbpProviderModuleName(provider);
        var useMethod = GetUseProviderMethod(provider);
        var hasFileManager = context.Symbols.Contains("module:file-manager");

        // Find and replace Data/*DbContext.cs (embedded MongoDB DbContext)
        var dbContextPath = context.Files.Keys
            .FirstOrDefault(f => f.Contains("Data") && f.EndsWith("DbContext.cs") && !f.Contains("MongoDb"));
        if (dbContextPath != null)
        {
            var blobStoringUsing = hasFileManager
                ? "\nusing SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;"
                : "";
            var blobStoringConfigure = hasFileManager
                ? "\n        builder.ConfigureSufiAbpBlobStoringDatabase();"
                : "";

            var dbContextContent = $@"using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;
using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;{blobStoringUsing}
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;
using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

namespace {solutionName}.Data;

[ConnectionStringName(""Default"")]
public class {projectName}DbContext : AbpDbContext<{projectName}DbContext>
{{
    public {projectName}DbContext(DbContextOptions<{projectName}DbContext> options)
        : base(options)
    {{
    }}

    protected override void OnModelCreating(ModelBuilder builder)
    {{
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureBackgroundJobs();{blobStoringConfigure}
    }}
}}
";
            context.Files[dbContextPath] = Encoding.UTF8.GetBytes(dbContextContent);
        }

        // Ensure correct EF Core provider package in host .csproj when user selected non-SqlServer
        var hostCsprojPath = context.Files.Keys
            .FirstOrDefault(f => f.EndsWith(".csproj") && f.Contains(projectName) && !f.Contains("Client") && !f.Contains("Contracts"));
        if (hostCsprojPath != null && provider != EfProviderKind.SqlServer)
        {
            var csprojContent = Encoding.UTF8.GetString(context.Files[hostCsprojPath]);
            csprojContent = Regex.Replace(
                csprojContent,
                @"<PackageReference Include=""Volo\.Abp\.EntityFrameworkCore\.(SqlServer|PostgreSql|MySql|Sqlite)""[^/]*/>",
                $"<PackageReference Include=\"{providerPackage}\" Version=\"$(AbpVersion)\" />");
            context.Files[hostCsprojPath] = Encoding.UTF8.GetBytes(csprojContent);
        }

        // Add ConfigureEfCore call and method in module (ConfigureMongoDB was removed by template markers)
        var modulePath = context.Files.Keys
            .FirstOrDefault(f => f.EndsWith("Module.cs") && !f.Contains("Contracts") && !f.Contains("Client") && !f.Contains("Application") && !f.Contains("Domain") && !f.Contains("HttpApi"));
        if (modulePath != null)
        {
            var content = Encoding.UTF8.GetString(context.Files[modulePath]);

            var configureEfCoreMethod = $@"
    private void ConfigureEfCore(ServiceConfigurationContext context)
    {{
        context.Services.AddAbpDbContext<{projectName}DbContext>(options =>
        {{
            options.AddDefaultRepositories(includeAllEntities: true);
        }});

        Configure<AbpDbContextOptions>(options =>
        {{
            options.{useMethod}();
        }});
    }}
";
            content = content.Replace(
                "ConfigureSwaggerServices(context.Services);",
                "ConfigureSwaggerServices(context.Services);\n        ConfigureEfCore(context);");
            content = content.Replace(
                "ConfigureSwaggerServices(context.Services);\n        ConfigureEfCore(context);\n        ConfigureEfCore(context);",
                "ConfigureSwaggerServices(context.Services);\n        ConfigureEfCore(context);");

            var methodInsertIndex = content.IndexOf("private void ConfigureSwaggerServices", StringComparison.Ordinal);
            if (methodInsertIndex >= 0)
            {
                content = content.Insert(methodInsertIndex, configureEfCoreMethod);
            }

            var providerNamespace = GetAbpProviderNamespace(provider);
            if (!content.Contains("using Volo.Abp.EntityFrameworkCore;"))
            {
                content = content.Replace("using Volo.Abp.Data;", "using Volo.Abp.Data;\nusing Volo.Abp.EntityFrameworkCore;");
            }
            if (!content.Contains($"using {providerNamespace};"))
            {
                content = content.Replace("using Volo.Abp.EntityFrameworkCore;", $"using Volo.Abp.EntityFrameworkCore;\nusing {providerNamespace};");
            }
            if (!content.Contains($"using {solutionName}.Data;"))
            {
                content = content.Replace($"using {solutionName}.Menus;", $"using {solutionName}.Data;\nusing {solutionName}.Menus;");
            }

            if (!content.Contains(providerModuleName))
            {
                content = content.Replace(
                    "typeof(AbpAutofacModule),",
                    $"typeof(AbpAutofacModule),\n    typeof({providerModuleName}),");
            }

            context.Files[modulePath] = Encoding.UTF8.GetBytes(content);
        }
    }

    private string GenerateCsprojContent(string solutionName, EfProviderKind provider, bool hasFileManager)
    {
        var providerPackage = GetAbpProviderPackage(provider);
        var additionalPackages = GetAdditionalDesignTimePackages(provider);
        var additionalPackagesBlock = string.IsNullOrEmpty(additionalPackages) ? "" : $"\n{additionalPackages}";
        var blobStoringPackage = hasFileManager
            ? @"
    <PackageReference Include=""SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore"" Version=""$(SufiVersion)"" />"
            : "";

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <Import Project=""..\..\versions.props"" />

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>{solutionName}.EntityFrameworkCore</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""{providerPackage}"" Version=""$(AbpVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.Identity.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore"" Version=""$(SufiVersion)"" />{blobStoringPackage}
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Tools"" Version=""$(AspNetCoreVersion)"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>{additionalPackagesBlock}
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\{solutionName}.Domain\{solutionName}.Domain.csproj"" />
  </ItemGroup>

</Project>
";
    }

    private string GenerateDbContextContent(string solutionName, string projectName, bool hasFileManager)
    {
        var blobStoringUsing = hasFileManager
            ? "\nusing SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;"
            : "";
        var blobStoringConfigure = hasFileManager
            ? "\n        builder.ConfigureSufiAbpBlobStoringDatabase();"
            : "";

        return $@"using Microsoft.EntityFrameworkCore;
	using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;
	using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;{blobStoringUsing}
	using Volo.Abp.Data;
	using Volo.Abp.EntityFrameworkCore;
	using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
	using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;
	using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

namespace {solutionName}.EntityFrameworkCore;

[ConnectionStringName(""Default"")]
public class {projectName}DbContext : AbpDbContext<{projectName}DbContext>
{{
    public {projectName}DbContext(DbContextOptions<{projectName}DbContext> options)
        : base(options)
    {{
    }}

    protected override void OnModelCreating(ModelBuilder builder)
    {{
        base.OnModelCreating(builder);

        // ABP modules
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        builder.ConfigureAuditLogging();
        builder.ConfigureBackgroundJobs();{blobStoringConfigure}

        // Configure your own entities here
    }}
}}
";
    }

    private string GenerateModuleContent(string solutionName, string projectName, EfProviderKind provider, bool hasFileManager)
    {
        var providerNamespace = GetAbpProviderNamespace(provider);
        var providerModuleName = GetAbpProviderModuleName(provider);
        var useMethod = GetUseProviderMethod(provider);
        var blobStoringUsing = hasFileManager
            ? "\nusing SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;"
            : "";
        var blobStoringDependsOn = hasFileManager
            ? ",\n    typeof(SufiAbpBlobStoringDatabaseEntityFrameworkCoreModule)"
            : "";

        return $@"using Microsoft.Extensions.DependencyInjection;
	using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;
	using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;{blobStoringUsing}
	using Volo.Abp.EntityFrameworkCore;
	using {providerNamespace};
	using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
	using Volo.Abp.Modularity;
	using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;
	using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;
	using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;
using {solutionName}.Domain;

namespace {solutionName}.EntityFrameworkCore;

[DependsOn(
    typeof({projectName}DomainModule),
    typeof({providerModuleName}),
	    typeof(SufiAbpPermissionManagementEntityFrameworkCoreModule),
	    typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),
	    typeof(SufiAbpIdentityEntityFrameworkCoreModule),
	    typeof(SufiAbpOpenIddictEntityFrameworkCoreModule),
	    typeof(SufiAbpFeatureManagementEntityFrameworkCoreModule),
	    typeof(SufiAbpTenantManagementEntityFrameworkCoreModule),
	    typeof(SufiAbpAuditLoggingEntityFrameworkCoreModule),
	    typeof(SufiAbpBackgroundJobsEntityFrameworkCoreModule){blobStoringDependsOn}
)]
public class {projectName}EntityFrameworkCoreModule : SufiAbpModule
{{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {{
        // Configure EF Core options here if needed
    }}

    public override void ConfigureServices(ServiceConfigurationContext context)
    {{
        context.Services.AddAbpDbContext<{projectName}DbContext>(options =>
        {{
            options.AddDefaultRepositories(includeAllEntities: true);
        }});

        Configure<AbpDbContextOptions>(options =>
        {{
            options.{useMethod}();
        }});
    }}
}}
";
    }

    private string GenerateDbContextFactoryContent(string solutionName, string projectName, EfProviderKind provider)
    {
        var connStringExpr = @"configuration.GetConnectionString(""Default"")";
        var builderCall = GetDbContextFactoryBuilderCall(provider, connStringExpr);
        var additionalUsings = GetFactoryAdditionalUsings(provider);

        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;{additionalUsings}

namespace {solutionName}.EntityFrameworkCore;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// </summary>
public class {projectName}DbContextFactory : IDesignTimeDbContextFactory<{projectName}DbContext>
{{
    public {projectName}DbContext CreateDbContext(string[] args)
    {{
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<{projectName}DbContext>()
            {builderCall};

        return new {projectName}DbContext(builder.Options);
    }}

    private static IConfigurationRoot BuildConfiguration()
    {{
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), ""../{{SolutionName}}.HttpApi.Host/"".Replace(""{{SolutionName}}"", ""{solutionName}"")))
            .AddJsonFile(""appsettings.json"", optional: false);

        return builder.Build();
    }}
}}
";
    }

    private string GenerateDbSchemaMigratorContent(string solutionName, string projectName)
    {
        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using {solutionName}.Domain.Data;

namespace {solutionName}.EntityFrameworkCore;

public class EntityFrameworkCore{projectName}DbSchemaMigrator : I{projectName}DbSchemaMigrator, ITransientDependency
{{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCore{projectName}DbSchemaMigrator(IServiceProvider serviceProvider)
    {{
        _serviceProvider = serviceProvider;
    }}

    public async Task MigrateAsync()
    {{
        await _serviceProvider
            .GetRequiredService<{projectName}DbContext>()
            .Database
            .MigrateAsync();
    }}
}}
";
    }

    private void AddEfCoreProjectToSolution(ProjectBuildContext context)
    {
        var solutionName = context.Args.SolutionName;
        var slnFile = context.Files.Keys.FirstOrDefault(f => f.EndsWith(".sln"));
        if (slnFile == null)
            return;

        var content = Encoding.UTF8.GetString(context.Files[slnFile]);
        var lines = content.Split('\n').ToList();
        var result = new List<string>();
        
        var efGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var srcFolderGuid = "58E47500-2571-4B38-84FE-3455689053E9";
        var addedProject = false;
        var addedConfigs = false;
        var addedNested = false;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            
            // Add project before Global (use src\ prefix to match host solution structure)
            if (!addedProject && line.Trim() == "Global")
            {
                result.Add($"Project(\"{{9A19103F-16F7-4668-BE54-9A1E7A4F7556}}\") = \"{solutionName}.EntityFrameworkCore\", \"src\\{solutionName}.EntityFrameworkCore\\{solutionName}.EntityFrameworkCore.csproj\", \"{{{efGuid}}}\"");
                result.Add("EndProject");
                addedProject = true;
            }

            result.Add(line);

            // Add configurations
            if (!addedConfigs && line.Trim() == "EndGlobalSection" && i > 0)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (lines[j].Contains("GlobalSection(ProjectConfigurationPlatforms)"))
                    {
                        result.RemoveAt(result.Count - 1);
                        result.Add($"\t\t{{{efGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                        result.Add($"\t\t{{{efGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                        result.Add($"\t\t{{{efGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                        result.Add($"\t\t{{{efGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU");
                        result.Add(line);
                        addedConfigs = true;
                        break;
                    }
                    if (lines[j].Contains("GlobalSection"))
                        break;
                }
            }

            // Add nested projects
            if (!addedNested && line.Trim() == "EndGlobalSection" && i > 0)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (lines[j].Contains("GlobalSection(NestedProjects)"))
                    {
                        result.RemoveAt(result.Count - 1);
                        result.Add($"\t\t{{{efGuid}}} = {{{srcFolderGuid}}}");
                        result.Add(line);
                        addedNested = true;
                        break;
                    }
                    if (lines[j].Contains("GlobalSection"))
                        break;
                }
            }
        }

        context.Files[slnFile] = Encoding.UTF8.GetBytes(string.Join('\n', result));
    }

    private void UpdateHostProjectReferences(ProjectBuildContext context)
    {
        var solutionName = context.Args.SolutionName;
        var mongoRef = $"{solutionName}.MongoDB";
        var efRef = $"{solutionName}.EntityFrameworkCore";

        // Find host project files that reference MongoDB
        var csprojFiles = context.Files.Keys
            .Where(f => f.EndsWith(".csproj") && 
                       (f.Contains("HttpApi.Host") || f.Contains("Blazor.WebApp") || f.Contains("DbMigrator") || f.Contains("AuthServer")))
            .ToList();

        foreach (var csprojFile in csprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[csprojFile]);
            
            // Replace MongoDB reference with EF Core reference
            content = content.Replace(
                $"<ProjectReference Include=\"..\\{mongoRef}\\{mongoRef}.csproj\" />",
                $"<ProjectReference Include=\"..\\{efRef}\\{efRef}.csproj\" />"
            );
            
            // Handle various path formats
            content = Regex.Replace(
                content,
                $@"<ProjectReference[^>]*{Regex.Escape(mongoRef)}[^>]*/?>",
                $"<ProjectReference Include=\"..\\{efRef}\\{efRef}.csproj\" />",
                RegexOptions.IgnoreCase
            );

            context.Files[csprojFile] = Encoding.UTF8.GetBytes(content);
        }
    }

    private void UpdateModuleDependencies(ProjectBuildContext context)
    {
        var solutionName = context.Args.SolutionName;
        var projectName = context.Args.ProjectName;

        // Find module files in host projects
        var moduleFiles = context.Files.Keys
            .Where(f => f.EndsWith("Module.cs") && 
                       (f.Contains("HttpApi.Host") || f.Contains("Blazor.WebApp") || f.Contains("DbMigrator") || f.Contains("AuthServer")))
            .ToList();

        foreach (var moduleFile in moduleFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[moduleFile]);
            
            // Replace MongoDB module dependency with EF Core
            content = content.Replace(
                $"typeof({projectName}MongoDbModule)",
                $"typeof({projectName}EntityFrameworkCoreModule)"
            );
            
            // Update using statement
            content = content.Replace(
                $"using {solutionName}.MongoDB;",
                $"using {solutionName}.EntityFrameworkCore;"
            );

            context.Files[moduleFile] = Encoding.UTF8.GetBytes(content);
        }
    }

    /// <summary>
    /// Switches SufiAbp module database packages from MongoDB to EntityFrameworkCore.
    /// For example: SufiChain.SufiAbp.FileManager.MongoDB -> SufiChain.SufiAbp.FileManager.EntityFrameworkCore
    /// </summary>
    private void SwitchSpModuleDbPackages(ProjectBuildContext context)
    {
        // SufiAbp modules that have both MongoDB and EF Core packages
        var spModuleDbSwaps = new Dictionary<string, string>
        {
            // FileManager
            ["SufiChain.SufiAbp.FileManager.MongoDB"] = "SufiChain.SufiAbp.FileManager.EntityFrameworkCore",
            ["FileManagerMongoDbModule"] = "FileManagerEntityFrameworkCoreModule",
            ["using SufiChain.SufiAbp.FileManager.MongoDB;"] = "using SufiChain.SufiAbp.FileManager.EntityFrameworkCore;",
            
            // LocalizationManagement
            ["SufiChain.SufiAbp.LocalizationManagement.MongoDB"] = "SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore",
            ["LocalizationManagementMongoDbModule"] = "LocalizationManagementEntityFrameworkCoreModule",
            ["using SufiChain.SufiAbp.LocalizationManagement.MongoDB;"] = "using SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;",
        };

        // Process .csproj files
        var csprojFiles = context.Files.Keys.Where(f => f.EndsWith(".csproj")).ToList();
        foreach (var file in csprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            var modified = false;

            if (content.Contains("SufiChain.SufiAbp.FileManager.MongoDB"))
            {
                content = content.Replace(
                    "SufiChain.SufiAbp.FileManager.MongoDB",
                    "SufiChain.SufiAbp.FileManager.EntityFrameworkCore"
                );
                modified = true;
            }

            if (content.Contains("SufiChain.SufiAbp.LocalizationManagement.MongoDB"))
            {
                content = content.Replace(
                    "SufiChain.SufiAbp.LocalizationManagement.MongoDB",
                    "SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore"
                );
                modified = true;
            }

            if (modified)
                context.Files[file] = Encoding.UTF8.GetBytes(content);
        }

        // Process .cs files (typeof references and using statements)
        var csFiles = context.Files.Keys.Where(f => f.EndsWith(".cs")).ToList();
        foreach (var file in csFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            var modified = false;

            foreach (var (mongoVal, efVal) in spModuleDbSwaps)
            {
                if (content.Contains(mongoVal))
                {
                    content = content.Replace(mongoVal, efVal);
                    modified = true;
                }
            }

            if (modified)
                context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    /// <summary>
    /// Swaps ABP infrastructure MongoDB NuGet packages to EntityFrameworkCore equivalents in host .csproj files.
    /// Affects WebApp template (host has ABP packages directly) and TEMPLATE-ONLY sections when uncommented.
    /// </summary>
    private void SwitchAbpInfraPackagesInHostCsproj(ProjectBuildContext context)
    {
        var abpMongoToEf = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SufiChain.SufiAbp.Identity.MongoDB"] = "SufiChain.SufiAbp.Identity.EntityFrameworkCore",
            ["SufiChain.SufiAbp.OpenIdDict.MongoDB"] = "SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore",
            ["SufiChain.SufiAbp.OpenIddict.MongoDB"] = "SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore",
            ["SufiChain.SufiAbp.TenantManagement.MongoDB"] = "SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore",
            ["SufiChain.SufiAbp.AuditLogging.MongoDB"] = "SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore",
            ["SufiChain.SufiAbp.PermissionManagement.MongoDB"] = "SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore",
            ["SufiChain.SufiAbp.FeatureManagement.MongoDB"] = "SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore",
            ["SufiChain.SufiAbp.SettingManagement.MongoDB"] = "SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore",
            ["SufiChain.SufiAbp.BackgroundJobs.MongoDB"] = "SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore",
        };

        var csprojFiles = context.Files.Keys
            .Where(f => f.EndsWith(".csproj") && !f.Contains(".MongoDB"))
            .ToList();

        foreach (var file in csprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            var modified = false;

            foreach (var (mongo, ef) in abpMongoToEf)
            {
                if (content.Contains(mongo))
                {
                    content = content.Replace(mongo, ef);
                    modified = true;
                }
            }

            if (modified)
                context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    /// <summary>
    /// Injects ABP EF Core module typeof() and using statements into host module .cs files.
    /// The TemplateMarkerProcessorStep removes MongoDB module refs when db:efcore is selected;
    /// this method adds the corresponding EF Core module refs.
    /// </summary>
    private void InjectAbpEfCoreModuleDependencies(ProjectBuildContext context)
    {
        var isSingle = context.Args.SolutionKind == SolutionKind.WebApp;

        // Using statements to add (before namespace)
        var efCoreUsings = new List<string>
        {
            "using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;",
            "using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;",
            "using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;",
        };
        if (isSingle)
        {
            efCoreUsings.AddRange(new[]
            {
                "using SufiChain.SufiAbp.Identity.EntityFrameworkCore;",
                "using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;",
                "using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;",
                "using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;",
                "using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;",
            });
        }

        // Module files to process (host modules that configure DB)
        var moduleFiles = context.Files.Keys
            .Where(f => f.EndsWith("Module.cs") && (
                (f.Contains("Blazor.WebApp") && !f.Contains("Blazor.WebApp.Client")) ||
                f.Contains("AuthServer") || f.Contains("HttpApi.Host")))
            .ToList();

        foreach (var file in moduleFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            // Add using statements before namespace
            var namespaceIndex = content.IndexOf("\nnamespace ", StringComparison.Ordinal);
            if (namespaceIndex > 0)
            {
                var insertBlock = string.Join("\n", efCoreUsings.Where(u => !content.Contains(u))) + "\n";
                if (insertBlock.Length > 1)
                {
                    content = content.Insert(namespaceIndex, insertBlock);
                }
            }

            // Add typeof for AuditLogging (before FeatureManagement in layered)
            if (!isSingle && !content.Contains("SufiAbpAuditLoggingEntityFrameworkCoreModule"))
            {
                content = content.Replace(
                    "typeof(SufiAbpFeatureManagementApplicationModule),",
                    "typeof(SufiAbpAuditLoggingEntityFrameworkCoreModule),\n    typeof(SufiAbpFeatureManagementApplicationModule),");
            }

            // Add typeof for FeatureManagement and SettingManagement
            content = content.Replace(
                "typeof(SufiAbpFeatureManagementApplicationModule),",
                "typeof(SufiAbpFeatureManagementApplicationModule),\n    typeof(SufiAbpFeatureManagementEntityFrameworkCoreModule),");
            content = content.Replace(
                "typeof(SufiAbpSettingManagementApplicationModule),",
                "typeof(SufiAbpSettingManagementApplicationModule),\n    typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),");

            if (isSingle)
            {
                if (!content.Contains("SufiAbpIdentityEntityFrameworkCoreModule"))
                {
                    content = content.Replace(
                        "typeof(SufiAbpPermissionManagementDomainOpenIddictModule),",
                        "typeof(SufiAbpPermissionManagementDomainOpenIddictModule),\n    typeof(SufiAbpIdentityEntityFrameworkCoreModule),\n    typeof(SufiAbpOpenIddictEntityFrameworkCoreModule),\n    typeof(SufiAbpTenantManagementEntityFrameworkCoreModule),\n    typeof(SufiAbpAuditLoggingEntityFrameworkCoreModule),\n    typeof(SufiAbpPermissionManagementEntityFrameworkCoreModule),");
                }
                if (!content.Contains("SufiAbpBackgroundJobsEntityFrameworkCoreModule"))
                {
                    content = content.Replace(
                        "typeof(SufiAbpSettingManagementApplicationModule),\n    typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),",
                        "typeof(SufiAbpSettingManagementApplicationModule),\n    typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),\n    typeof(SufiAbpBackgroundJobsEntityFrameworkCoreModule),");
                }
            }

            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    private void UpdateConnectionStrings(ProjectBuildContext context, EfProviderKind provider)
    {
        var dbName = context.Args.CompanyName + context.Args.ProjectName;
        
        // Use explicit connection string if provided via --connection-string CLI option
        var connectionString = context.Args.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = GetDefaultConnectionString(provider, dbName);
        }
        
        var appSettingsFiles = context.Files.Keys
            .Where(f => f.EndsWith("appsettings.json") || f.EndsWith("appsettings.Development.json"))
            .ToList();

        foreach (var file in appSettingsFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            
            // Replace MongoDB connection string with the provider-specific one
            content = Regex.Replace(
                content,
                @"""Default"":\s*""mongodb://[^""]+""",
                $@"""Default"": ""{connectionString}"""
            );

            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }
}
