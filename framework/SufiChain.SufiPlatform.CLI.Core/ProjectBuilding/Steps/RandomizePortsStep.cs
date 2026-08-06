using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Randomizes ports in configuration files to avoid conflicts between multiple scaffolded projects.
/// Applies to all hosts (AuthServer, HttpApi.Host, Blazor.WebApp, etc.) and DbMigrator appsettings.json.
/// OpenIddict Applications RootUrl values must match host ports.
/// Port sources: .dev/hosts/layered (44305 Api, 44350 Blazor), .dev/hosts/layered-tiered (44316 Blazor, etc.).
/// </summary>
public class RandomizePortsStep : ProjectBuildPipelineStep
{
    public override string Description => "Randomizing ports...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var ports = context.Ports;

        // Process all JSON and settings files (includes DbMigrator appsettings.json)
        var filesToProcess = context.Files.Keys
            .Where(f => f.EndsWith(".json") || f.EndsWith("launchSettings.json"))
            .ToList();

        foreach (var filePath in filesToProcess)
        {
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            var originalContent = content;

            // Replace ports - order matters! Replace longer/more specific port numbers first
            // to avoid partial matches (e.g. 44306 before 44305)
            
            // Replace Blazor.WebApp URLs (44316, 44317)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.BlazorPort, ports.BlazorPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.BlazorHttpPort, ports.BlazorHttpPort);
            
            // Replace AuthServer URLs (44306)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.AuthServerPort, ports.AuthServerPort);
            
            // Replace HttpApi.Host URLs (44305)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.ApiPort, ports.ApiPort);
            
            // Replace Blazor WASM URLs (44307)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.BlazorWasmPort, ports.BlazorWasmPort);
            
            // Replace WebSite URLs (60927, 60928 and tiered 44320 used in OpenIddict/DbMigrator/AuthServer)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.PublicPort, ports.PublicPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.PublicHttpPort, ports.PublicHttpPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.PublicPortTiered, ports.PublicPort);
            
            // Replace Web MVC URLs (44302)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.WebPort, ports.WebPort);
            
            // Replace WebApp architecture host URLs (44338, 44339)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.SingleHostPort, ports.SingleHostPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.SingleHostHttpPort, ports.SingleHostHttpPort);
            
            // Replace Blazor.WebApp.Client dev server (62577, 62578)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.BlazorWebAppClientPort, ports.BlazorWebAppClientPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.BlazorWebAppClientHttpPort, ports.BlazorWebAppClientHttpPort);
            
            // Replace Blazor.WebSite.Client dev server (65419, 65420)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.WebSiteClientPort, ports.WebSiteClientPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.WebSiteClientHttpPort, ports.WebSiteClientHttpPort);
            
            // Replace WebApp architecture client URLs (65463, 65464)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.SingleClientPort, ports.SingleClientPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.SingleClientHttpPort, ports.SingleClientHttpPort);
            
            // Replace layered (non-tiered) architecture ports (44350, 44351, 62590, 62591)
            content = ReplacePort(content, PortConfiguration.OriginalPorts.LayeredBlazorPort, ports.BlazorPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.LayeredBlazorHttpPort, ports.BlazorHttpPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.LayeredBlazorWebAppClientPort, ports.BlazorWebAppClientPort);
            content = ReplacePort(content, PortConfiguration.OriginalPorts.LayeredBlazorWebAppClientHttpPort, ports.BlazorWebAppClientHttpPort);

            if (content != originalContent)
            {
                context.Files[filePath] = Encoding.UTF8.GetBytes(content);
            }
        }

        return Task.CompletedTask;
    }

    private static string ReplacePort(string content, int oldPort, int newPort)
    {
        if (oldPort == newPort)
            return content;

        // Replace in URLs: https://localhost:PORT or http://localhost:PORT
        // Using string concatenation to avoid interpolation issues with $1
        content = Regex.Replace(
            content,
            @"(https?://localhost:)" + oldPort + @"\b",
            "${1}" + newPort
        );

        // Replace in sslPort JSON property
        content = Regex.Replace(
            content,
            @"(""sslPort"":\s*)" + oldPort + @"\b",
            "${1}" + newPort
        );

        // Replace in applicationUrl with multiple ports (e.g., "https://localhost:44316;http://localhost:44317")
        // This is handled by the URL replacement above

        return content;
    }
}
