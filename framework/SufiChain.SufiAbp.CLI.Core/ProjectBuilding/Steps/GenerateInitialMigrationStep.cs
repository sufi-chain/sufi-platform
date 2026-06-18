using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Diagnostics;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Prompts user to generate initial EF Core migration and runs the command if confirmed.
/// Only runs if EF Core is selected (not MongoDB).
/// </summary>
public class GenerateInitialMigrationStep : ProjectBuildPipelineStep
{
    public override string Description => "Generating initial migration...";

    public override async Task ExecuteAsync(ProjectBuildContext context)
    {
        // Only run for EF Core projects
        if (!context.Symbols.Contains("db:efcore"))
        {
            return;
        }

        // Prompt user
        Console.WriteLine();
        Console.Write("Generate Initial migration? (Y/n): ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (response == "n" || response == "no")
        {
            Console.WriteLine("Skipping migration generation.");
            return;
        }

        // Find EntityFrameworkCore and startup projects
        var efCoreProject = FindProjectFile(context, "EntityFrameworkCore");
        var startupProject = FindStartupProject(context);

        if (efCoreProject == null)
        {
            Console.WriteLine("Warning: EntityFrameworkCore project not found. Skipping migration generation.");
            return;
        }

        if (startupProject == null)
        {
            Console.WriteLine("Warning: Startup project not found. Skipping migration generation.");
            return;
        }

        Console.WriteLine($"Running: dotnet ef migrations add Initial");
        Console.WriteLine($"  Project: {efCoreProject}");
        Console.WriteLine($"  Startup: {startupProject}");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"ef migrations add Initial --project \"{efCoreProject}\" --startup-project \"{startupProject}\"",
                    WorkingDirectory = context.Args.OutputDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("✓ Initial migration generated successfully.");
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine(output);
                }
            }
            else
            {
                Console.WriteLine($"⚠ Migration generation failed with exit code {process.ExitCode}");
                Console.WriteLine("You can generate the migration manually by running:");
                Console.WriteLine($"  cd {context.Args.OutputDirectory}");
                Console.WriteLine($"  dotnet ef migrations add Initial --project \"{efCoreProject}\" --startup-project \"{startupProject}\"");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine(error);
                }
                // Don't throw, just warn and continue
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error generating migration: {ex.Message}");
        }
    }

    private string? FindProjectFile(ProjectBuildContext context, string projectNamePart)
    {
        var projectFiles = context.Files.Keys
            .Where(f => f.EndsWith(".csproj") && f.Contains(projectNamePart))
            .ToList();

        return projectFiles.FirstOrDefault();
    }

    private string? FindStartupProject(ProjectBuildContext context)
    {
        var candidates = context.Args.SolutionKind == SolutionKind.WebApp
            ? new[] { "Blazor.WebApp", "DbMigrator" }
            : new[] { "HttpApi.Host", "AuthServer", "DbMigrator" };

        foreach (var candidate in candidates)
        {
            var project = FindProjectFile(context, candidate);
            if (project != null)
            {
                return project;
            }
        }

        return null;
    }
}
