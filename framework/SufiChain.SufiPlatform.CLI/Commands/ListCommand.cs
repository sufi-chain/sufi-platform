using SufiChain.SufiPlatform.CLI.Templates;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SufiChain.SufiPlatform.CLI.Commands;

/// <summary>
/// Lists available templates.
/// </summary>
public class ListCommand : Command<ListCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var templateManager = new TemplateManager();
        var templates = templateManager.GetAvailableTemplates();

        AnsiConsole.MarkupLine("[bold]Available Templates[/]");
        AnsiConsole.WriteLine();

        if (!templates.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No templates found.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("For development, set the [green]SUFI_TEMPLATE_PATH[/] environment variable to point to the template directory.");
            return 0;
        }

        // Group by source
        var embeddedTemplates = templates.Where(t => t.Source == "embedded").ToList();
        var filesystemTemplates = templates.Where(t => t.Source == "filesystem").ToList();

        var table = new Table();
        table.AddColumn("Template");
        table.AddColumn("Description");
        table.AddColumn("Source");
        table.AddColumn("Database Providers");
        table.AddColumn("Architectures");

        foreach (var template in templates)
        {
            var source = template.Source == "embedded" 
                ? "[blue]embedded[/]" 
                : $"[green]filesystem[/] ({template.Path})";
            
            table.AddRow(
                $"[bold]{template.Name}[/]",
                template.Description,
                source,
                string.Join(", ", template.SupportedDatabaseProviders),
                string.Join(", ", template.SupportedArchitectures)
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Usage:[/] sufi new [options]");
        AnsiConsole.MarkupLine("[dim]Example:[/] sufi new MyCompany.MyApp -d ef --tiered");
        
        return 0;
    }
}
