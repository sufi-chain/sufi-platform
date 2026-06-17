using SufiChain.SufiAbp.CLI.Commands;
using Spectre.Console.Cli;

namespace SufiChain.SufiAbp.CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();
        
        app.Configure(config =>
        {
            config.SetApplicationName("sufi");
            config.SetApplicationVersion("0.0.1-alpha");
            
            config.AddCommand<NewCommand>("new")
                .WithDescription("Creates a new Sufi Platform solution.")
                .WithExample("new", "MyCompany.MyProject")
                .WithExample("new", "MyCompany.MyProject", "-d", "ef")
                .WithExample("new", "MyCompany.MyProject", "-d", "mongo", "--tiered")
                .WithExample("new", "MyCompany.MyProject", "-d", "ef", "--solution-kind", "webapp");
            
            config.AddCommand<ListCommand>("list")
                .WithDescription("Lists available templates.");
            
            config.AddCommand<HelpCommand>("help")
                .WithDescription("Shows help information.");
            
            config.AddCommand<VersionCommand>("version")
                .WithDescription("Shows version information.");
        });
        
        return await app.RunAsync(args);
    }
}
