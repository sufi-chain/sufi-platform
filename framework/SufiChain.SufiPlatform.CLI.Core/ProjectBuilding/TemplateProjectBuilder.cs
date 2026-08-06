using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;
using SufiChain.SufiPlatform.CLI.Templates;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding;

/// <summary>
/// Orchestrates the project building process using a pipeline of steps.
/// </summary>
public class TemplateProjectBuilder
{
    private readonly TemplateManager _templateManager;
    
    public TemplateProjectBuilder()
    {
        _templateManager = new TemplateManager();
    }
    
    /// <summary>
    /// Builds a new solution from template.
    /// </summary>
    /// <param name="args">Build arguments specifying solution name, database, architecture, etc.</param>
    /// <param name="reportProgress">Optional callback invoked before each pipeline step with the step description.</param>
    public async Task BuildAsync(ProjectBuildArgs args, Action<string>? reportProgress = null)
    {
        // Create build context
        var context = new ProjectBuildContext
        {
            Args = args
        };
        
        // Initialize context
        context.InitializeSymbols();
        context.InitializeReplacements();
        
        // Build the pipeline
        var templatePath = _templateManager.GetTemplatePath(args.TemplateName);
        var pipeline = CreatePipeline(args, templatePath);
        
        // Execute pipeline
        await pipeline.ExecuteAsync(context, reportProgress);
    }

    private ProjectBuildPipeline CreatePipeline(ProjectBuildArgs args, string? templatePath)
    {
        var pipeline = new ProjectBuildPipeline();

        // =====================================================================
        // PHASE 1: Load and Transform Template
        // =====================================================================

        // Step 1: Load template files from hosts/mongodb/tiered/
        pipeline.AddStep(new ReadTemplateFilesStep(templatePath, _templateManager));
        
        // Step 2: Process TEMPLATE-REMOVE and TEMPLATE-ONLY markers
        // This converts project references to NuGet references and handles conditional code
        pipeline.AddStep(new TemplateMarkerProcessorStep());
        
        // Step 3: Clean template (remove projects not needed for Blazor WebApp)
        pipeline.AddStep(new CleanTemplateStep());
        
        // Step 4: Rename solution and projects (Sufi.DemoApp → CompanyName.ProjectName)
        pipeline.AddStep(new SolutionRenameStep());
        
        // =====================================================================
        // PHASE 2: Configure Solution Structure
        // =====================================================================
        
        // Step 6: Configure database provider
        if (args.DatabaseProvider == DatabaseProvider.EntityFrameworkCore)
        {
            pipeline.AddStep(new SwitchToEfCoreStep());
            
            // Step 6.5: Switch EF Core provider (if not SQL Server)
            pipeline.AddStep(new SwitchEfProviderStep());
        }
        else
        {
            pipeline.AddStep(new SwitchToMongoDbStep());
        }
        
        // Step 7: Configure architecture based on SolutionKind + Tiered
        if (args.SolutionKind == SolutionKind.WebApp)
        {
            pipeline.AddStep(new ConfigureSingleStep());
        }
        else if (args.IsTiered)
        {
            // Layered-Tiered: 3+ hosts (WebApp + AuthServer + HttpApi.Host + optional WebSite)
            pipeline.AddStep(new ConfigureTieredAuthServerStep());
        }
        else
        {
            // Layered non-tiered: WebApp host plus HttpApi.Host
            pipeline.AddStep(new ConfigureLayeredNonTieredStep());
        }
        
        // =====================================================================
        // PHASE 3: Host and Module Configuration
        // =====================================================================
        
        // Step 8: Remove excluded host projects (WebSite, Web if not selected)
        pipeline.AddStep(new RemoveHostStep());
        
        // Step 9: Configure modules (keep default platform modules, remove unselected demos)
        pipeline.AddStep(new AddModuleStep());

        // Step 9.5: Add selected published feature-pack packages and module dependencies
        pipeline.AddStep(new InstallPublishedModulesStep());
        
        // Step 10: Remove unused projects (after host and module configuration)
        pipeline.AddStep(new RemoveProjectStep());
        
        // Step 11: Generate modern .slnx solution file (after all projects are finalized)
        pipeline.AddStep(new GenerateSlnxStep());
        
        // =====================================================================
        // PHASE 4: Configuration and Branding
        // =====================================================================
        
        // Step 12: Update connection strings
        pipeline.AddStep(new UpdateConnectionStringsStep());
        
        // Step 13: Apply branding (app name, logo URL)
        pipeline.AddStep(new ApplyBrandingStep());
        
        // Step 14: Randomize ports
        pipeline.AddStep(new RandomizePortsStep());
        
        // Step 15: Select and place docker-compose template
        pipeline.AddStep(new SelectDockerComposeStep());
        
        // =====================================================================
        // PHASE 5: Finalize Output
        // =====================================================================
        
        // Step 16: Create README
        pipeline.AddStep(new CreateReadmeStep());
        
        // Step 17: Create .gitignore
        pipeline.AddStep(new CreateGitIgnoreStep());
        
        // Step 18: Create output directory and write files.
        // Directory.Build.props and versions.props come from the template unchanged.
        pipeline.AddStep(new CreateOutputStep());

        // Step 19: Generate initial EF Core migration (interactive prompt, runs after files are written)
        pipeline.AddStep(new GenerateInitialMigrationStep());
        
        // Step 20: Run DbMigrator to seed data (interactive prompt, runs after migration)
        pipeline.AddStep(new RunDbMigratorStep());
        
        return pipeline;
    }
}
