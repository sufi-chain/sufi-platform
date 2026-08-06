using SufiChain.SufiPlatform.CLI.Args;

namespace SufiChain.SufiPlatform.CLI.Modules;

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
        RegisterProModules();
    }
    
    private void RegisterCoreModules()
    {
        // Identity module - always included
        Register(new ModuleDefinition
        {
            Key = "identity",
            DisplayName = "Identity",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Identity",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "User and role management with ABP Identity integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.WebSite, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });
        
        // Account module - always included
        Register(new ModuleDefinition
        {
            Key = "account",
            DisplayName = "Account",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Account",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Account management with ABP Account integration (register, profile, password reset)",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient | ModulePackageTypes.Blazor
        });
        
        // Permission Management - always included
        Register(new ModuleDefinition
        {
            Key = "permissions",
            DisplayName = "Permission Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Permissions",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Permission management with ABP Permission Management integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient
        });
        
        // Feature Management - always included
        Register(new ModuleDefinition
        {
            Key = "features",
            DisplayName = "Feature Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Features",
            Category = ModuleCategory.Core,
            IsCore = true,
            Description = "Feature flag management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient | ModulePackageTypes.Blazor
        });
        
        // Setting Management - always included
        Register(new ModuleDefinition
        {
            Key = "settings",
            DisplayName = "Setting Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Settings",
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
            Key = "tenants",
            DisplayName = "Tenant Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Tenants",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Multi-tenant support with tenant switching UI",
            DependsOn = new[] { "features" },
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web }
        });
        
        // File Manager
        Register(new ModuleDefinition
        {
            Key = "file-manager",
            DisplayName = "File Manager",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.FileManager",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "File upload, storage, and management with RTE integration",
            ApplicableHosts = new[] { HostType.WebApp, HostType.WebSite, HostType.HttpApi, HostType.Web }
        });
        
        // Audit Logging
        Register(new ModuleDefinition
        {
            Key = "audit-logging",
            DisplayName = "Audit Logging",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.AuditLogging",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Audit log viewing and management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        // Background Jobs
        Register(new ModuleDefinition
        {
            Key = "jobs",
            DisplayName = "Background Jobs",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.BackgroundJobs",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Background job management and monitoring UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        // Localization Management
        Register(new ModuleDefinition
        {
            Key = "localization",
            DisplayName = "Localization Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Localization",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Runtime localization management UI",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web }
        });
        
        Register(new ModuleDefinition
        {
            Key = "ai",
            DisplayName = "AI Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.SufiAI",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "AI workspace, RAG, MCP, and provider management",
            DependsOn = new[] { "file-manager" },
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });

        Register(new ModuleDefinition
        {
            Key = "short-links",
            DisplayName = "Short Link Generator",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.ShortLinks",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "URL shortening with click analytics",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });

        Register(new ModuleDefinition
        {
            Key = "blob-database",
            DisplayName = "Blob Storing Database",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.BlobDatabase",
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
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Users",
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
            NuGetPackagePrefix = "SufiChain.SufiPlatform.OpenIddict",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "OpenIddict application, authorization, scope, and token storage",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.Domain | ModulePackageTypes.EntityFrameworkCore | ModulePackageTypes.MongoDB
        });

        Register(new ModuleDefinition
        {
            Key = "sufi-theme",
            DisplayName = "SufiTheme",
            NuGetPackagePrefix = "SufiChain.SufiTheme",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Default Sufi Platform shell, layout, navigation, and theme",
            ApplicableHosts = new[] { HostType.WebApp, HostType.AuthServer, HostType.WebSite, HostType.Web },
            AvailablePackages = ModulePackageTypes.Blazor
        });

        Register(new ModuleDefinition
        {
            Key = "calendar",
            DisplayName = "Calendar",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Calendar",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Calendar events, scheduling, public calendar UI, and AI assistance",
            ApplicableHosts = new[] { HostType.WebApp, HostType.WebSite, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All | ModulePackageTypes.BlazorPublic
        });

        Register(new ModuleDefinition
        {
            Key = "menus",
            DisplayName = "Menu Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Menus",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Dynamic menu and navigation management",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.All
        });

        Register(new ModuleDefinition
        {
            Key = "tags",
            DisplayName = "Tags Management",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Tags",
            Category = ModuleCategory.Infrastructure,
            IsCore = true,
            Description = "Reusable tag taxonomy and tagging services",
            ApplicableHosts = new[] { HostType.WebApp, HostType.HttpApi, HostType.Web },
            AvailablePackages = ModulePackageTypes.DomainShared | ModulePackageTypes.Domain | ModulePackageTypes.ApplicationContracts | ModulePackageTypes.Application | ModulePackageTypes.EntityFrameworkCore | ModulePackageTypes.MongoDB | ModulePackageTypes.HttpApi | ModulePackageTypes.HttpApiClient
        });

        // Demo/sample modules must be explicitly selected.
        Register(new ModuleDefinition
        {
            Key = "file-manager-demo",
            DisplayName = "File Manager Demo",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.FileManager.Demo",
            Category = ModuleCategory.Feature,
            IsCore = false,
            Description = "File Manager sample/demo UI",
            DependsOn = new[] { "file-manager" },
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
        Register(new ModuleDefinition
        {
            Key = "editions",
            DisplayName = "Editions",
            NuGetPackagePrefix = "SufiChain.SufiPlatform.Editions",
            Category = ModuleCategory.Feature,
            IsDefault = true,
            Description = "Edition definitions and feature-plan management",
            ApplicableHosts = [],
            Bindings =
            [
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.DomainShared,
                    PackageId = "SufiChain.SufiPlatform.Editions.Domain.Shared",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsDomainSharedModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.Domain,
                    PackageId = "SufiChain.SufiPlatform.Editions.Domain",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsDomainModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.ApplicationContracts,
                    PackageId = "SufiChain.SufiPlatform.Editions.Application.Contracts",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsApplicationContractsModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.Application,
                    PackageId = "SufiChain.SufiPlatform.Editions.Application",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsApplicationModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.EntityFrameworkCore,
                    PackageId = "SufiChain.SufiPlatform.Editions.EntityFrameworkCore",
                    ModuleType = "SufiChain.SufiPlatform.Editions.EntityFrameworkCore.SufiEditionsEntityFrameworkCoreModule",
                    DatabaseProvider = DatabaseProvider.EntityFrameworkCore
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.MongoDB,
                    PackageId = "SufiChain.SufiPlatform.Editions.MongoDB",
                    ModuleType = "SufiChain.SufiPlatform.Editions.MongoDB.SufiEditionsMongoDbModule",
                    DatabaseProvider = DatabaseProvider.MongoDB
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.HttpApi,
                    PackageId = "SufiChain.SufiPlatform.Editions.HttpApi",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsHttpApiModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.HttpApiClient,
                    PackageId = "SufiChain.SufiPlatform.Editions.HttpApi.Client",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsHttpApiClientModule"
                },
                new ModuleBinding
                {
                    IntegrationPoint = ModuleIntegrationPoint.BlazorWebApp,
                    PackageId = "SufiChain.SufiPlatform.Editions.Blazor",
                    ModuleType = "SufiChain.SufiPlatform.Editions.SufiEditionsBlazorModule"
                }
            ]
        });
    }

    private void RegisterProModules()
    {
        foreach (var module in ProModuleCatalog.CreateAll())
        {
            Register(module);
        }
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

    public IEnumerable<ModuleDefinition> GetDefaultModules()
    {
        return _modules.Values.Where(m => m.IsDefault);
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

    public HashSet<string> ResolveSelection(
        IEnumerable<string> requestedModuleKeys,
        IEnumerable<string> excludedModuleKeys,
        bool includeDefaults)
    {
        var requested = new HashSet<string>(
            includeDefaults ? GetDefaultModules().Select(module => module.Key) : [],
            StringComparer.OrdinalIgnoreCase);
        requested.UnionWith(requestedModuleKeys);

        var excluded = new HashSet<string>(excludedModuleKeys, StringComparer.OrdinalIgnoreCase);
        var unknown = requested.Concat(excluded)
            .Where(key => GetModule(key) == null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (unknown.Length > 0)
        {
            throw new ArgumentException($"Unknown module key(s): {string.Join(", ", unknown)}");
        }

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var module in GetOptionalModules())
            {
                if (excluded.Contains(module.Key) ||
                    !module.DependsOn.Any(excluded.Contains))
                {
                    continue;
                }

                excluded.Add(module.Key);
                changed = true;
            }
        }

        requested.ExceptWith(excluded);
        return new HashSet<string>(
            ResolveWithDependencies(requested)
                .Where(module => !excluded.Contains(module.Key))
                .Select(module => module.Key),
            StringComparer.OrdinalIgnoreCase);
    }
}
