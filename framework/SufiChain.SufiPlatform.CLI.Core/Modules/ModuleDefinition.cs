using SufiChain.SufiPlatform.CLI.Args;

namespace SufiChain.SufiPlatform.CLI.Modules;

/// <summary>
/// Defines a module that can be included in a generated solution.
/// </summary>
public class ModuleDefinition
{
    /// <summary>
    /// Unique key for the module (e.g., "identity", "file-manager", "sufi-cms").
    /// </summary>
    public required string Key { get; init; }
    
    /// <summary>
    /// Display name for the module.
    /// </summary>
    public required string DisplayName { get; init; }
    
    /// <summary>
    /// NuGet package prefix (e.g., "SufiChain.SufiPlatform.Identity").
    /// </summary>
    public required string NuGetPackagePrefix { get; init; }
    
    /// <summary>
    /// Category of the module.
    /// </summary>
    public required ModuleCategory Category { get; init; }
    
    /// <summary>
    /// Whether this is a core module that is always included.
    /// </summary>
    public bool IsCore { get; init; }
    
    /// <summary>
    /// Other modules this module depends on.
    /// </summary>
    public string[] DependsOn { get; init; } = Array.Empty<string>();
    
    /// <summary>
    /// Which hosts can use this module.
    /// </summary>
    public HostType[] ApplicableHosts { get; init; } = Array.Empty<HostType>();
    
    /// <summary>
    /// Description of the module.
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Available package types for this module.
    /// </summary>
    public ModulePackageTypes AvailablePackages { get; init; } = ModulePackageTypes.All;
}

/// <summary>
/// Categories of modules.
/// </summary>
public enum ModuleCategory
{
    /// <summary>
    /// Default platform modules that are always included.
    /// </summary>
    Core,
    
    /// <summary>
    /// Infrastructure modules (FileManager, BackgroundJobs, AuditLogging).
    /// </summary>
    Infrastructure,
    
    /// <summary>
    /// Feature modules (SufiCMS, future verticals).
    /// </summary>
    Feature,
    
    /// <summary>
    /// Admin-only modules (OpenIddict management, future admin modules).
    /// </summary>
    Admin
}

/// <summary>
/// Flags for available package types in a module.
/// </summary>
[Flags]
public enum ModulePackageTypes
{
    None = 0,
    DomainShared = 1,
    Domain = 2,
    ApplicationContracts = 4,
    Application = 8,
    EntityFrameworkCore = 16,
    MongoDB = 32,
    HttpApi = 64,
    HttpApiClient = 128,
    Blazor = 256,
    BlazorPublic = 512,
    All = DomainShared | Domain | ApplicationContracts | Application | EntityFrameworkCore | MongoDB | HttpApi | HttpApiClient | Blazor
}
