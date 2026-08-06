using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;

namespace SufiChain.SufiPlatform.CLI.Commands;

/// <summary>
/// Command to show CLI version information.
/// </summary>
public class VersionCommand : Command<VersionCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion 
            ?? "0.0.0";
        
        AnsiConsole.MarkupLine($"[bold]Sufi CLI[/] version [green]{version}[/]");
        AnsiConsole.MarkupLine("SufiChain Sufi Platform Solution Scaffolding Tool");
        
        return 0;
    }
}
