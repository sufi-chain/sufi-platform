using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.Modules;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding;

/// <summary>
/// Context object that holds the state during project building.
/// </summary>
public class ProjectBuildContext
{
    /// <summary>
    /// Build arguments provided by the user.
    /// </summary>
    public required ProjectBuildArgs Args { get; init; }
    
    /// <summary>
    /// In-memory file system containing the template files.
    /// Key: relative path, Value: file content
    /// </summary>
    public Dictionary<string, byte[]> Files { get; } = new();
    
    /// <summary>
    /// Symbols that can be used for conditional processing.
    /// </summary>
    public HashSet<string> Symbols { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Template placeholders and their replacement values.
    /// </summary>
    public Dictionary<string, string> Replacements { get; } = new();
    
    /// <summary>
    /// Files to be removed from the output.
    /// </summary>
    public HashSet<string> FilesToRemove { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Projects to be removed from the solution.
    /// </summary>
    public HashSet<string> ProjectsToRemove { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Port configuration for the generated solution.
    /// </summary>
    public PortConfiguration Ports { get; set; } = PortConfiguration.GenerateRandom();
    
    /// <summary>
    /// Initialize symbols based on build args.
    /// </summary>
    public void InitializeSymbols()
    {
        // Database symbols
        if (Args.DatabaseProvider == DatabaseProvider.EntityFrameworkCore)
        {
            Symbols.Add("EFCORE");
            Symbols.Add("db:efcore");
            
            // EF sub-provider
            if (Args.EfProvider.HasValue)
            {
                Symbols.Add($"efprovider:{Args.EfProvider.Value.ToString().ToLowerInvariant()}");
                Symbols.Add($"efp:{Args.EfProvider.Value.ToString().ToLowerInvariant()}");
            }
            else
            {
                Symbols.Add("efp:sqlserver");
            }
        }
        else
        {
            Symbols.Add("MONGODB");
            Symbols.Add("db:mongodb");
        }

        // Solution kind symbols
        Symbols.Add($"solution:{Args.SolutionKind.ToString().ToLowerInvariant()}");
        
        // Architecture symbols
        if (Args.IsTiered)
        {
            Symbols.Add("tiered");
            Symbols.Add("arch:tiered");
        }
        else if (Args.SolutionKind == SolutionKind.WebApp)
        {
            Symbols.Add("webapp");
            Symbols.Add("arch:webapp");
            Symbols.Add("single");
            Symbols.Add("arch:single");
        }
        else
        {
            Symbols.Add("arch:layered");
        }
        
        // AuthServer symbol
        if (Args.IncludeAuthServer)
        {
            Symbols.Add("authserver");
        }
        
        // Multi-tenancy symbol
        if (Args.IsMultiTenancyEnabled)
        {
            Symbols.Add("multi-tenancy");
        }
        
        // Public website symbol
        if (Args.IncludeWebSite)
        {
            Symbols.Add("website");
        }
        
        // Host symbols
        foreach (var host in Args.IncludedHosts)
        {
            Symbols.Add($"host:{host.ToString().ToLowerInvariant()}");
        }
        
        // Real platform modules are enabled by default. Samples/demos are opt-in.
        var moduleRegistry = new ModuleRegistry();
        var enabledModules = moduleRegistry.ResolveWithDependencies(Args.IncludedModules);
        foreach (var module in enabledModules)
        {
            Symbols.Add($"module:{module.Key.ToLowerInvariant()}");
        }
    }
    
    /// <summary>
    /// Initialize standard replacements for template processing.
    /// </summary>
    public void InitializeReplacements()
    {
        // Template uses "MyCompanyName.MyProjectName" as placeholders
        // We need to replace these with the user's actual company/project names
        // IMPORTANT: Replace longest strings first to avoid partial replacements
        
        // Full solution name: MyCompanyName.MyProjectName -> TestCompany.TestProduct
        Replacements["MyCompanyName.MyProjectName"] = Args.SolutionName;
        
        // Individual parts
        Replacements["MyCompanyName"] = Args.CompanyName;
        Replacements["MyProjectName"] = Args.ProjectName;
        
        // Lowercase versions for URLs, connection strings, etc.
        Replacements["mycompanyname.myprojectname"] = Args.SolutionName.ToLowerInvariant();
        Replacements["mycompanyname"] = Args.CompanyName.ToLowerInvariant();
        Replacements["myprojectname"] = Args.ProjectName.ToLowerInvariant();
        
        // Connection string database name (combined PascalCase)
        Replacements["MyCompanyNameMyProjectName"] = Args.CompanyName + Args.ProjectName;
        
        // ============================================================
        // LEGACY: Also include Sufi.DemoApp replacements
        // in case loading from hosts directly (development mode)
        // ============================================================
        Replacements["Sufi.DemoApp"] = Args.SolutionName;
        Replacements["Sufi"] = Args.CompanyName;
        Replacements["DemoApp"] = Args.ProjectName;
        
        // Lowercase legacy versions
        Replacements["sufiplatform.demoapp"] = Args.SolutionName.ToLowerInvariant();
        Replacements["sufiplatform"] = Args.CompanyName.ToLowerInvariant();
        Replacements["demoapp"] = Args.ProjectName.ToLowerInvariant();
        
        // Legacy connection string database name
        Replacements["SufiDemoApp"] = Args.CompanyName + Args.ProjectName;
        
        // ============================================================
        // Port replacements for AuthServer (tiered architecture)
        // ============================================================
        Replacements[PortConfiguration.OriginalPorts.AuthServerPort.ToString()] = Ports.AuthServerPort.ToString();
        Replacements[PortConfiguration.OriginalPorts.PublicPort.ToString()] = Ports.PublicPort.ToString();
        Replacements[PortConfiguration.OriginalPorts.PublicHttpPort.ToString()] = Ports.PublicHttpPort.ToString();
    }
}
