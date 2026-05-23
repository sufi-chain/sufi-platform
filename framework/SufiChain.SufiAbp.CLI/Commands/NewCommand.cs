using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.Modules;
using SufiChain.SufiAbp.CLI.ProjectBuilding;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SufiChain.SufiAbp.CLI.Commands;

/// <summary>
/// Command to create a new Sufi Platform solution.
/// </summary>
public class NewCommand : AsyncCommand<NewCommand.Settings>
{
    public class Settings : CommandSettings
    {
        // ============================================================
        // NON-INTERACTIVE MODE OPTIONS (bypass wizard)
        // When NAME is provided, wizard is skipped and these options are used
        // ============================================================
        
        [CommandArgument(0, "[NAME]")]
        [Description("Solution name (e.g., MyCompany.MyProject). When provided, skips interactive wizard.")]
        public string Name { get; set; } = string.Empty;

        [CommandOption("-d|--database")]
        [Description("Database provider: 'ef' or 'mongo' (default: mongo)")]
        [DefaultValue("mongo")]
        public string Database { get; set; } = "mongo";

        [CommandOption("--solution-kind")]
        [Description("Solution type: 'single' or 'layered' (default: layered)")]
        [DefaultValue("layered")]
        public string SolutionKindStr { get; set; } = "layered";

        [CommandOption("--tiered")]
        [Description("Use tiered architecture with separate API + Auth hosts (only for layered)")]
        public bool Tiered { get; set; }

        [CommandOption("--multi-tenancy")]
        [Description("Enable multi-tenancy (forces tenant-management module)")]
        public bool MultiTenancy { get; set; }

        [CommandOption("--public-website")]
        [Description("Include SufiCMS public website")]
        public bool PublicWebsite { get; set; }

        [CommandOption("--ef-provider")]
        [Description("EF Core sub-provider: sqlserver, postgresql, mysql, mariadb, sqlite")]
        public string? EfProvider { get; set; }
        
        [CommandOption("--connection-string")]
        [Description("Database connection string")]
        public string? ConnectionString { get; set; }

        [CommandOption("-o|--output")]
        [Description("Output directory (default: current directory)")]
        public string? OutputDirectory { get; set; }
        
        [CommandOption("--modules")]
        [Description("Optional sample/demo modules to include (e.g. sufi-blazor-demo). Real platform modules are enabled by default.")]
        public string? Modules { get; set; }
        
        [CommandOption("--app-name")]
        [Description("Application display name for branding")]
        public string? AppName { get; set; }
        
        [CommandOption("--logo-url")]
        [Description("Logo URL for branding")]
        public string? LogoUrl { get; set; }
        
        // ============================================================
        // UTILITY OPTIONS
        // ============================================================
        
        [CommandOption("--list-modules")]
        [Description("List available modules and exit")]
        public bool ListModules { get; set; }
        
        [CommandOption("--no-interactive")]
        [Description("Disable interactive wizard (for CI/CD). Requires NAME argument.")]
        public bool NoInteractive { get; set; }
        
        /// <summary>
        /// Determines if the interactive wizard should run.
        /// Wizard runs by default unless:
        /// - NAME argument is provided (non-interactive mode)
        /// - --no-interactive flag is set
        /// </summary>
        public bool ShouldRunWizard => !NoInteractive && string.IsNullOrWhiteSpace(Name);

        public override ValidationResult Validate()
        {
            // Skip validation if just listing modules
            if (ListModules)
            {
                return ValidationResult.Success();
            }
            
            // Skip validation if wizard will run (wizard gathers all required data)
            if (ShouldRunWizard)
            {
                return ValidationResult.Success();
            }
            
            // Non-interactive mode: validate all required parameters
            if (string.IsNullOrWhiteSpace(Name))
            {
                return ValidationResult.Error("Solution name is required in non-interactive mode. Run without arguments for interactive wizard.");
            }

            if (!Name.Contains('.'))
            {
                return ValidationResult.Error("Solution name must be in format 'CompanyName.ProjectName' (e.g., MyCompany.MyProject)");
            }

            var db = Database.ToLowerInvariant();
            if (db != "ef" && db != "entityframeworkcore" && db != "mongo" && db != "mongodb")
            {
                return ValidationResult.Error("Database must be 'ef' (Entity Framework Core) or 'mongo' (MongoDB)");
            }

            var kind = SolutionKindStr.ToLowerInvariant();
            if (kind != "single" && kind != "layered")
            {
                return ValidationResult.Error("Solution kind must be 'single' or 'layered'.");
            }
            
            if (Tiered && kind == "single")
            {
                return ValidationResult.Error("Tiered architecture is only available for layered solutions.");
            }
            
            // Validate EF provider if specified
            if (!string.IsNullOrEmpty(EfProvider))
            {
                var validProviders = new[] { "sqlserver", "postgresql", "mysql", "mariadb", "sqlite" };
                if (!validProviders.Contains(EfProvider.ToLowerInvariant()))
                {
                    return ValidationResult.Error($"Invalid EF provider: '{EfProvider}'. Valid: sqlserver, postgresql, mysql, mariadb, sqlite");
                }
            }

            return ValidationResult.Success();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        // Handle --list-modules option
        if (settings.ListModules)
        {
            DisplayAvailableModules();
            return 0;
        }
        
        AnsiConsole.Write(new FigletText("Sufi CLI").Color(Color.Blue));
        AnsiConsole.WriteLine();
        
        // Interactive wizard is the DEFAULT behavior
        // It only runs when no NAME argument is provided
        if (settings.ShouldRunWizard)
        {
            settings = RunInteractiveWizard(settings);
        }
        
        // Parse and normalize settings
        var args = ParseBuildArgs(settings);

        // Display configuration
        DisplayConfiguration(args);

        // Build the project
        var builder = new TemplateProjectBuilder();
        
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Creating solution...", async ctx =>
                {
                    await builder.BuildAsync(args, msg => ctx.Status(msg));
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]Success![/] Solution created at: [blue]{args.OutputDirectory}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Next steps:[/]");
            AnsiConsole.MarkupLine($"  1. cd {Path.GetFileName(args.OutputDirectory)}");
            AnsiConsole.MarkupLine("  2. dotnet restore");
            
            if (args.DatabaseProvider == DatabaseProvider.MongoDB)
            {
                AnsiConsole.MarkupLine("  3. Start MongoDB server");
            }
            else
            {
                AnsiConsole.MarkupLine("  3. Update connection string in appsettings.json");
            }
            
            if (args.IsTiered)
            {
                AnsiConsole.MarkupLine("  4. Run AuthServer first (OIDC authority)");
                AnsiConsole.MarkupLine("  5. Run HttpApi.Host (API)");
                if (args.IncludedHosts.Contains(HostType.WebApp))
                {
                    AnsiConsole.MarkupLine("  6. Run Blazor.WebApp (admin panel)");
                }
                if (args.IncludedHosts.Contains(HostType.WebPublic))
                {
                    AnsiConsole.MarkupLine("  7. Run Blazor.WebPublic (public site)");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("  4. Run the WebApp project");
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
    
    private static void DisplayAvailableModules()
    {
        var registry = new ModuleRegistry();
        
        AnsiConsole.Write(new FigletText("Sufi CLI").Color(Color.Blue));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Available Modules[/]");
        AnsiConsole.WriteLine();
        
        var table = new Table();
        table.AddColumn("Key");
        table.AddColumn("Name");
        table.AddColumn("Category");
        table.AddColumn("Default");
        table.AddColumn("Hosts");
        table.Border = TableBorder.Rounded;
        
        foreach (var module in registry.GetAllModules())
        {
            table.AddRow(
                module.Key,
                module.DisplayName,
                module.Category.ToString(),
                module.IsCore ? "[green]Yes[/]" : "No",
                string.Join(", ", module.ApplicableHosts.Select(h => h.ToString()))
            );
        }
        
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Real platform modules are enabled by default. Optional entries are sample/demo modules only.[/]");
        AnsiConsole.MarkupLine("[dim]Example: sufi new MyCompany.MyProject --modules sufi-blazor-demo[/]");
    }
    
    /// <summary>
    /// Runs the interactive wizard to gather project settings from the user.
    /// </summary>
    private static Settings RunInteractiveWizard(Settings settings)
    {
        AnsiConsole.MarkupLine("[yellow]Interactive Project Setup[/]");
        AnsiConsole.WriteLine();
        
        // Step 1 -- Solution type
        var solutionKindChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]?[/] Select solution type:")
                .PageSize(5)
                .AddChoices(new[]
                {
                    "Single (minimal 3 projects)",
                    "Layered (DDD with full project structure)"
                }));
        settings.SolutionKindStr = solutionKindChoice.StartsWith("Single") ? "single" : "layered";
        
        // If Layered, ask about tiered
        if (settings.SolutionKindStr == "layered")
        {
            settings.Tiered = AnsiConsole.Prompt(
                new ConfirmationPrompt("[green]?[/] Enable tiered architecture? (separate API + Auth hosts)")
                    { DefaultValue = true });
        }
        
        // Step 2 -- Solution name
        var solutionName = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]?[/] What is your solution name?")
                .DefaultValue("MyCompany.MyProject")
                .Validate(name =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                        return ValidationResult.Error("Solution name cannot be empty");
                    if (!name.Contains('.'))
                        return ValidationResult.Error("Solution name must be in format 'CompanyName.ProjectName'");
                    return ValidationResult.Success();
                }));
        settings.Name = solutionName;
        
        // Step 3 -- Database provider
        var databaseChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]?[/] Select database provider:")
                .PageSize(5)
                .AddChoices(new[] { "Entity Framework Core (recommended)", "MongoDB" }));
        settings.Database = databaseChoice.StartsWith("Entity Framework") ? "ef" : "mongo";
        
        // If EF Core, ask for sub-provider
        if (settings.Database == "ef")
        {
            var efProviderChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]?[/] Select EF Core provider:")
                    .PageSize(6)
                    .AddChoices(new[]
                    {
                        "SQL Server (recommended)",
                        "PostgreSQL",
                        "MySQL",
                        "MariaDB",
                        "SQLite"
                    }));
            settings.EfProvider = efProviderChoice.Replace(" ", "").ToLowerInvariant();
            
            // Connection string (optional)
            var connStr = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]?[/] Connection string (optional, press Enter to skip):")
                    .AllowEmpty());
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                settings.ConnectionString = connStr;
            }
        }
        
        // Step 4 -- Multi-tenancy
        settings.MultiTenancy = AnsiConsole.Prompt(
            new ConfirmationPrompt("[green]?[/] Enable multi-tenancy?")
                { DefaultValue = true });
        
        // Step 5 -- Public website (only for layered tiered)
        if (settings.SolutionKindStr == "layered" && settings.Tiered)
        {
            settings.PublicWebsite = AnsiConsole.Prompt(
                new ConfirmationPrompt("[green]?[/] Include SufiCMS Public Website?")
                    { DefaultValue = false });
        }
        
        // Step 6 -- Optional sample/demo modules
        var registry = new ModuleRegistry();
        var optionalModules = registry.GetOptionalModules()
            .Select(m => $"{m.Key} - {m.Description}")
            .ToArray();
        
        if (optionalModules.Any())
        {
            var moduleChoices = new MultiSelectionPrompt<string>()
                .Title("[green]?[/] Select optional modules:")
                .PageSize(10)
                .NotRequired()
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                .AddChoices(optionalModules);
            
            var selectedModules = AnsiConsole.Prompt(moduleChoices);
            var modules = selectedModules
                .Select(m => m.Split(" - ")[0])
                .ToList();
            settings.Modules = modules.Count > 0 ? string.Join(",", modules) : string.Empty;
        }
        
        // Step 7 -- Application display name
        var projectName = solutionName.Contains('.') 
            ? solutionName.Split('.').Last() 
            : solutionName;
        var appName = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]?[/] Application display name:")
                .DefaultValue(projectName)
                .AllowEmpty());
        if (!string.IsNullOrWhiteSpace(appName) && appName != projectName)
        {
            settings.AppName = appName;
        }
        
        // Step 8 -- Output directory
        var currentDir = Directory.GetCurrentDirectory();
        var defaultOutputPath = Path.Combine(currentDir, solutionName);
        
        var outputDirChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]?[/] Output directory:")
                .AddChoices(new[]
                {
                    $"Current directory ({defaultOutputPath})",
                    "Custom location..."
                }));
        
        if (outputDirChoice.StartsWith("Custom"))
        {
            var customPath = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]?[/] Enter output path:")
                    .DefaultValue(currentDir)
                    .Validate(path =>
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            return ValidationResult.Error("Output path cannot be empty");
                        return ValidationResult.Success();
                    }));
            
            if (!customPath.EndsWith(solutionName, StringComparison.OrdinalIgnoreCase))
            {
                settings.OutputDirectory = Path.Combine(customPath, solutionName);
            }
            else
            {
                settings.OutputDirectory = customPath;
            }
        }
        else
        {
            settings.OutputDirectory = defaultOutputPath;
        }
        
        AnsiConsole.MarkupLine($"[dim]Output: {Path.GetFullPath(settings.OutputDirectory)}[/]");
        AnsiConsole.WriteLine();
        
        return settings;
    }

    private static ProjectBuildArgs ParseBuildArgs(Settings settings)
    {
        var parts = settings.Name.Split('.', 2);
        var companyName = parts[0];
        var projectName = parts.Length > 1 ? parts[1] : parts[0];

        var db = settings.Database.ToLowerInvariant();
        var databaseProvider = db switch
        {
            "ef" or "entityframeworkcore" => DatabaseProvider.EntityFrameworkCore,
            _ => DatabaseProvider.MongoDB
        };

        // Parse solution kind
        var solutionKind = settings.SolutionKindStr.ToLowerInvariant() switch
        {
            "single" => SolutionKind.Single,
            _ => SolutionKind.Layered
        };
        
        // Tiered only valid for Layered
        var isTiered = solutionKind == SolutionKind.Layered && settings.Tiered;
        
        // AuthServer is implied by tiered
        var includeAuthServer = isTiered;
        
        // Public website
        var includePublicWebApp = settings.PublicWebsite;
        
        // Multi-tenancy
        var isMultiTenancyEnabled = settings.MultiTenancy;
        
        // EF sub-provider
        EfProviderKind? efProvider = null;
        if (databaseProvider == DatabaseProvider.EntityFrameworkCore)
        {
            // Default to SQL Server if no provider specified
            var providerStr = string.IsNullOrEmpty(settings.EfProvider) 
                ? "sqlserver" 
                : settings.EfProvider.ToLowerInvariant();
            
            efProvider = providerStr switch
            {
                "sqlserver" => EfProviderKind.SqlServer,
                "postgresql" => EfProviderKind.PostgreSQL,
                "mysql" => EfProviderKind.MySQL,
                "mariadb" => EfProviderKind.MariaDB,
                "sqlite" => EfProviderKind.Sqlite,
                _ => EfProviderKind.SqlServer
            };
        }

        var outputDir = settings.OutputDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), settings.Name);

        // Compute hosts from solution kind
        var includedHosts = ProjectBuildArgs.ComputeIncludedHosts(solutionKind, isTiered, includePublicWebApp);
        
        // Compute template name
        var templateName = ProjectBuildArgs.ComputeTemplateName(solutionKind, isTiered);
        
        // Parse included modules with constraint logic
        var includedModules = ParseModules(settings);
        
        // Real platform modules are enabled by default by ModuleRegistry.
        // --modules is now reserved for optional sample/demo modules.

        return new ProjectBuildArgs
        {
            SolutionName = settings.Name,
            CompanyName = companyName,
            ProjectName = projectName,
            DatabaseProvider = databaseProvider,
            SolutionKind = solutionKind,
            IsTiered = isTiered,
            IncludeAuthServer = includeAuthServer,
            IncludePublicWebApp = includePublicWebApp,
            EfProvider = efProvider,
            ConnectionString = settings.ConnectionString,
            IsMultiTenancyEnabled = isMultiTenancyEnabled,
            OutputDirectory = Path.GetFullPath(outputDir),
            TemplateName = templateName,
            IncludedHosts = includedHosts,
            IncludedModules = includedModules,
            AppName = settings.AppName,
            LogoUrl = settings.LogoUrl
        };
    }
    
    private static HashSet<string> ParseModules(Settings settings)
    {
        var modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        if (!string.IsNullOrEmpty(settings.Modules))
        {
            var moduleList = settings.Modules.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var module in moduleList)
            {
                modules.Add(module);
            }
        }
        
        return modules;
    }

    private static void DisplayConfiguration(ProjectBuildArgs args)
    {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.Border = TableBorder.Rounded;

        table.AddRow("Solution Name", args.SolutionName);
        table.AddRow("Company Name", args.CompanyName);
        table.AddRow("Project Name", args.ProjectName);
        table.AddRow("Solution Kind", args.SolutionKind.ToString());
        table.AddRow("Database Provider", args.DatabaseProvider.ToString());
        
        if (args.EfProvider.HasValue)
        {
            table.AddRow("EF Provider", args.EfProvider.Value.ToString());
        }
        
        table.AddRow("Architecture", args.IsTiered ? "Layered-Tiered" : args.SolutionKind == SolutionKind.Single ? "Single" : "Layered");
        table.AddRow("Multi-Tenancy", args.IsMultiTenancyEnabled ? "Enabled" : "Disabled");
        table.AddRow("Hosts", string.Join(", ", args.IncludedHosts.Select(h => h.ToString())));
        table.AddRow("Template", args.TemplateName);
        
        if (args.IncludedModules.Count > 0)
        {
            table.AddRow("Modules", string.Join(", ", args.IncludedModules));
        }
        
        if (!string.IsNullOrEmpty(args.AppName))
        {
            table.AddRow("App Name", args.AppName);
        }
        
        table.AddRow("Output Directory", args.OutputDirectory);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
