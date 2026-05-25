using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Diagnostics;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Prompts user to run DbMigrator and executes it if confirmed.
/// </summary>
public class RunDbMigratorStep : ProjectBuildPipelineStep
{
    public override string Description => "Running DbMigrator...";

    public override async Task ExecuteAsync(ProjectBuildContext context)
    {
        // Find DbMigrator project
        var dbMigratorProject = FindProjectFile(context, "DbMigrator");

        if (dbMigratorProject == null)
        {
            Console.WriteLine("Warning: DbMigrator project not found. Skipping.");
            return;
        }

        // Prompt user
        Console.WriteLine();
        Console.Write("Run DbMigrator to seed initial data? (Y/n): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (response == "n" || response == "no")
        {
            Console.WriteLine("Skipping DbMigrator.");
            return;
        }

        Console.WriteLine($"Running: dotnet run --project \"{dbMigratorProject}\"");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{dbMigratorProject}\"",
                    WorkingDirectory = context.Args.OutputDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            // Stream output in real-time
            var outputTask = Task.Run(async () =>
            {
                string? line;
                while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                {
                    Console.WriteLine(line);
                }
            });

            var errorTask = Task.Run(async () =>
            {
                string? line;
                while ((line = await process.StandardError.ReadLineAsync()) != null)
                {
                    Console.Error.WriteLine(line);
                }
            });

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("✓ DbMigrator completed successfully.");
            }
            else
            {
                Console.WriteLine($"⚠ DbMigrator failed with exit code {process.ExitCode}");
                Console.WriteLine("You can run DbMigrator manually by running:");
                Console.WriteLine($"  cd {context.Args.OutputDirectory}");
                Console.WriteLine($"  dotnet run --project \"{dbMigratorProject}\"");
                // Don't throw, just warn and continue
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error running DbMigrator: {ex.Message}");
            Console.WriteLine("You can run DbMigrator manually after fixing the issue.");
        }
    }

    private string? FindProjectFile(ProjectBuildContext context, string projectNamePart)
    {
        var projectFiles = context.Files.Keys
            .Where(f => f.EndsWith(".csproj") && f.Contains(projectNamePart))
            .ToList();

        return projectFiles.FirstOrDefault();
    }
}
