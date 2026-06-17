namespace SufiChain.SufiAbp.CLI.Templates;

/// <summary>
/// Blazor Web App solution template information.
/// Based on the KomTheme.DemoApp demo solution.
/// </summary>
public class BlazorWebAppTemplate : ITemplateInfo
{
    public string Name => "blazor-webapp";
    
    public string DisplayName => "Blazor Web App";
    
    public string Description => "A Blazor Web App solution with ABP framework, " +
                                 "supporting both MongoDB and Entity Framework Core, " +
                                 "with tiered, layered, or WebApp architecture options.";
    
    public IReadOnlyList<string> SupportedDatabaseProviders => new[] { "MongoDB", "EntityFrameworkCore" };
    
    public bool SupportsTiered => true;
    
    public bool SupportsSingle => true;
    
    public string BaseSolutionName => "KomTheme.DemoApp";
    
    public string BaseCompanyName => "KomTheme";
    
    public string BaseProjectName => "DemoApp";
}
