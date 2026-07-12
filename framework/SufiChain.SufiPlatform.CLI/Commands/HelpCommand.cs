using Spectre.Console;
using Spectre.Console.Cli;

namespace SufiChain.SufiPlatform.CLI.Commands;

/// <summary>
/// Command to show help information.
/// </summary>
public class HelpCommand : Command<HelpCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.Write(new FigletText("Sufi CLI").Color(Color.Blue));
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold]SufiChain Sufi Platform CLI[/]");
        AnsiConsole.MarkupLine("Scaffolds new Sufi Platform solutions with customizable options.");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold yellow]Commands:[/]");
        AnsiConsole.MarkupLine("  [green]new[/] <name>    Creates a new Sufi Platform solution");
        AnsiConsole.MarkupLine("  [green]help[/]         Shows this help information");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold yellow]Options for 'new' command:[/]");
        var table = new Table();
        table.AddColumn("Option");
        table.AddColumn("Description");
        table.AddColumn("Default");
        table.Border = TableBorder.Simple;

        table.AddRow("-d, --database", "Database provider: 'ef' or 'mongo'", "mongo");
        table.AddRow("--tiered", "Tiered architecture with separate API + Auth hosts", "false");
        table.AddRow("--solution-kind", "Solution type: webapp or layered", "layered");
        table.AddRow("--modules", "Optional sample/demo modules only", "none");
        table.AddRow("--include-website", "Include optional Blazor.WebSite host (tiered only)", "false");
        table.AddRow("-o, --output", "Output directory", "current directory");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold yellow]Examples:[/]");
        AnsiConsole.MarkupLine("  sufi new MyCompany.MyProject");
        AnsiConsole.MarkupLine("  sufi new MyCompany.MyProject -d ef");
        AnsiConsole.MarkupLine("  sufi new MyCompany.MyProject -d mongo --solution-kind webapp");
        AnsiConsole.MarkupLine("  sufi new MyCompany.MyProject -d ef --tiered -o C:\\Projects");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold yellow]Database Providers:[/]");
        AnsiConsole.MarkupLine("  [green]mongo[/] (MongoDB)     - NoSQL document database (default)");
        AnsiConsole.MarkupLine("  [green]ef[/] (EF Core)        - Entity Framework Core with SQL Server");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold yellow]Architectures:[/]");
        AnsiConsole.MarkupLine("  [green]Layered[/] (default)   - Blazor.WebApp + HttpApi.Host");
        AnsiConsole.MarkupLine("  [green]Tiered[/]              - Blazor.WebApp + HttpApi.Host + AuthServer");
        AnsiConsole.MarkupLine("  [green]WebApp[/]              - Integrated Blazor.WebApp with embedded API and cookie auth");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Modules:[/]");
        AnsiConsole.MarkupLine("  Real platform modules in src/modules are enabled by default.");
        AnsiConsole.MarkupLine("  Demo/sample modules are opt-in, e.g. [green]--modules sufi-blazor-demo[/].");

        return 0;
    }
}
