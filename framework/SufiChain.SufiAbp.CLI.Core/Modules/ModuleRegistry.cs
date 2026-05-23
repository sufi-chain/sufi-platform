using SufiChain.SufiAbp.CLI.Args;

namespace SufiChain.SufiAbp.CLI.Modules;

/// <summary>
/// Registry of available modules for Sufi Platform solutions.
/// </summary>
public class ModuleRegistry
{
    private readonly Dictionary<string, ModuleDefinition> _modules = new(StringComparer.OrdinalIgnoreCase);
    
    public ModuleRegistry()
    {
        RegisterCoreModules();
        RegisterInfrastructureModules();
        RegisterAdminModules();
        RegisterFeatureModules();
    }
    
    private void RegisterCoreModules()
    {
        // Identity module - always included
        Register(new ModuleDefinition
        {
            Key = "identity",
            DisplayName = "Identity",
            NuGetPackagePrefix = "SufiChain.SufiAbp.Identity",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "User and role management with ABP Identity integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.WebPublic, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });
        
        // Account module - always included
        Register(new ModuleDefinition
        {
            Key = "account",
            DisplayName = "Account",
            NuGetPackagePrefix = "SufiChain.SufiAbp.Account",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Account management with ABP Account integration (register, profile, password reset)",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient | ModulePackageTypes.Blazor
        });
        
        // Permission Management - always included
        Register(new ModuleDefinition
        {
            Key = "permission-management",
            DisplayName = "Permission Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.PermissionManagement",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Permission management with ABP Permission Management integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient
        });
        
        // Feature Management - always included
        Register(new ModuleDefinition
        {
            Key = "feature-management",
            DisplayName = "Feature Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.FeatureManagement",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Feature flag management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient | ModulePackageTypes.Blazor
        });
        
        // Setting Management - always included
        Register(new ModuleDefinition
        {
            Key = "setting-management",
            DisplayName = "Setting Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.SettingManagement",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Application settings management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient | ModulePackageTypes.Blazor
        });
    }
    
    private void RegisterInfrastructureModules()
    {
        Register(new ModuleDefinition
        {
            Key = "tenant-management",
            DisplayName = "Tenant Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.TenantManagement",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Multi-tenant support with tenant switching UI",
            DependsOn = new[] { "feature-management" },
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web }
        });
        
        // File Manager
        Register(new ModuleDefinition
        {
            Key = "file-manager",
            DisplayName = "File Manager",
            NuGetPackagePrefix = "SufiChain.SufiAbp.FileManager",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "File upload, storage, and management with RTE integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.WebPublic, HostType.HttpApi, HostType.Web }
        });
        
        // Audit Logging
        Register(new ModuleDefinition
        {
            Key = "audit-logging",
            DisplayName = "Audit Logging",
            NuGetPackagePrefix = "SufiChain.SufiAbp.AuditLogging",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Audit log viewing and management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        // Background Jobs
        Register(new ModuleDefinition
        {
            Key = "background-jobs",
            DisplayName = "Background Jobs",
            NuGetPackagePrefix = "SufiChain.SufiAbp.BackgroundJobs",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Background job management and monitoring UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        // Localization Management
        Register(new ModuleDefinition
        {
            Key = "localization-management",
            DisplayName = "Localization Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.LocalizationManagement",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Runtime localization management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        Register(new ModuleDefinition
        {
            Key = "ai-management",
            DisplayName = "AI Management",
            NuGetPackagePrefix = "SufiChain.SufiAbp.AIManagement",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "AI workspace, RAG, MCP, and provider management",
            DependsOn = new[] { "file-manager" },
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });

        Register(new ModuleDefinition
        {
            Key = "short-link-generator",
            DisplayName = "Short Link Generator",
            NuGetPackagePrefix = "SufiChain.SufiAbp.ShortLinkGenerator",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "URL shortening with click analytics",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });

        Register(new ModuleDefinition
        {
            Key = "blob-storing-database",
            DisplayName = "Blob Storing Database",
            NuGetPackagePrefix = "SufiChain.SufiAbp.BlobStoring.Database",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Database-backed blob storage module",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.Domain | ModulePackageTypes.EntityFrameworkCore | ModulePackageTypes.MongoDB
        });

        Register(new ModuleDefinition
        {
            Key = "users",
            DisplayName = "Users",
            NuGetPackagePrefix = "SufiChain.SufiAbp.Users",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "User management abstractions and persistence",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.Domain | ModulePackageTypes.EntityFrameworkCore | ModulePackageTypes.MongoDB
        });

        Register(new ModuleDefinition
        {
            Key = "openiddict",
            DisplayName = "OpenIddict",
            NuGetPackagePrefix = "SufiChain.SufiAbp.OpenIddict",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "OpenIddict application, authorization, scope, and token storage",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.Domain | ModulePackageTypes.EntityFrameworkCore | ModulePackageTypes.MongoDB
        });

        Register(new ModuleDefinition
        {
            Key = "kom-theme",
            DisplayName = "KomTheme",
            NuGetPackagePrefix = "SufiChain.KomTheme",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Default Sufi Platform shell, layout, navigation, and theme",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.WebPublic, HostType.Web },
            AvailablePackages = ModulePackageTypes.Blazor
        });

        // Demo/sample modules must be explicitly selected.
        Register(new ModuleDefinition
        {
            Key = "file-manager-demo",
            DisplayName = "File Manager Demo",
            NuGetPackagePrefix = "SufiChain.SufiAbp.FileManager.Demo",
            Category = ModuleCategory.Feature,
            IsCore = false,
            Description = "File Manager sample/demo UI",
            DependsOn = new[] { "file-manager" },
            ApplicableHosts = new[] { HostType.WebApp, HostType.Web }
        });

        Register(new ModuleDefinition
        {
            Key = "sufi-blazor-demo",
            DisplayName = "Sufi Blazor Demo",
            NuGetPackagePrefix = "SufiChain.SufiBlazor.Demo",
            Category = ModuleCategory.Feature,
            IsCore = false,
            Description = "SufiBlazor component samples and documentation",
            ApplicableHosts = new[] { HostType.WebApp, HostType.Web }
        });
    }
    
    private void RegisterAdminModules()
    {
        // NOTE: openiddict-management and sufi-cms are not yet implemented.
        // Re-add them here when their NuGet packages and host references exist.
    }
    
    private void RegisterFeatureModules()
    {
        // Reserved for future feature modules (e.g. Sufi CMS).
    }
    
    /// <summary>
    /// Registers a module definition.
    /// </summary>
    public void Register(ModuleDefinition module)
    {
        _modules[module.Key] = module;
    }
    
    /// <summary>
    /// Gets a module by its key.
    /// </summary>
    public ModuleDefinition? GetModule(string key)
    {
        return _modules.TryGetValue(key, out var module) ? module : null;
    }
    
    /// <summary>
    /// Gets all registered modules.
    /// </summary>
    public IEnumerable<ModuleDefinition> GetAllModules()
    {
        return _modules.Values.OrderBy(m => m.Category).ThenBy(m => m.DisplayName);
    }
    
    /// <summary>
    /// Gets all core modules (always included).
    /// </summary>
    public IEnumerable<ModuleDefinition> GetCoreModules()
    {
        return _modules.Values.Where(m => m.IsCore);
    }
    
    /// <summary>
    /// Gets optional modules (user can choose to include).
    /// </summary>
    public IEnumerable<ModuleDefinition> GetOptionalModules()
    {
        return _modules.Values.Where(m => !m.IsCore);
    }
    
    /// <summary>
    /// Gets modules applicable to a specific host type.
    /// </summary>
    public IEnumerable<ModuleDefinition> GetModulesForHost(HostType host)
    {
        return _modules.Values.Where(m => m.ApplicableHosts.Contains(host));
    }
    
    /// <summary>
    /// Resolves module dependencies and returns all required modules.
    /// </summary>
    public IEnumerable<ModuleDefinition> ResolveWithDependencies(IEnumerable<string> moduleKeys)
    {
        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ModuleDefinition>();
        
        void Resolve(string key)
        {
            if (resolved.Contains(key))
                return;
                
            var module = GetModule(key);
            if (module == null)
                return;
                
            // Resolve dependencies first
            foreach (var dep in module.DependsOn)
            {
                Resolve(dep);
            }
            
            resolved.Add(key);
            result.Add(module);
        }
        
        // Always include core modules
        foreach (var core in GetCoreModules())
        {
            Resolve(core.Key);
        }
        
        // Resolve requested modules
        foreach (var key in moduleKeys)
        {
            Resolve(key);
        }
        
        return result;
    }
}
