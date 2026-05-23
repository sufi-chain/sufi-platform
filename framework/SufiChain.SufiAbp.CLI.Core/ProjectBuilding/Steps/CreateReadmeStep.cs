using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Creates a README.md file for the generated solution.
/// </summary>
public class CreateReadmeStep : ProjectBuildPipelineStep
{
    public override string Description => "Creating README...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var content = GenerateReadme(context.Args);
        context.Files["README.md"] = Encoding.UTF8.GetBytes(content);
        return Task.CompletedTask;
    }

    private string GenerateReadme(ProjectBuildArgs args)
    {
        var dbSection = args.DatabaseProvider == DatabaseProvider.MongoDB
            ? GetMongoDbSection()
            : GetEfCoreSection(args.SolutionName);

        var architectureSection = args.IsTiered
            ? GetTieredSection(args.SolutionName)
            : GetSingleSection(args.SolutionName);

        return $@"# {args.SolutionName}

This solution was generated using the Sufi Platform CLI.

## Configuration

- **Database Provider**: {args.DatabaseProvider}
- **Architecture**: {(args.IsTiered ? "Tiered" : "Single")}

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- {(args.DatabaseProvider == DatabaseProvider.MongoDB ? "MongoDB Server" : "SQL Server (LocalDB or full instance)")}
- Node.js (for frontend asset bundling)

### Running the Application

{architectureSection}

{dbSection}

## Solution Structure

```
{args.SolutionName}/
├── {args.SolutionName}.Domain.Shared/     # Shared constants, enums, DTOs
├── {args.SolutionName}.Domain/            # Domain entities and business logic
├── {args.SolutionName}.Application.Contracts/ # Application service interfaces
├── {args.SolutionName}.Application/       # Application service implementations
├── {args.SolutionName}.{(args.DatabaseProvider == DatabaseProvider.MongoDB ? "MongoDB" : "EntityFrameworkCore")}/  # Database layer
├── {args.SolutionName}.HttpApi/           # HTTP API controllers
{(args.IsTiered ? $"├── {args.SolutionName}.HttpApi.Host/       # API host (Auth server)\n├── {args.SolutionName}.HttpApi.Client/     # HTTP client proxies" : "")}
├── {args.SolutionName}.Blazor.WebApp/     # Blazor Server application
└── {args.SolutionName}.Blazor.WebApp.Client/ # Blazor WebAssembly client
```

## Built With

- [ABP Framework](https://abp.io/) - Application framework
- [SufiChain Sufi Platform](https://github.com/SufiChain) - UI framework with SufiBlazor components
- [Blazor](https://blazor.net/) - Web UI framework

## License

This project is proprietary software.
";
    }

    private string GetMongoDbSection()
    {
        return @"### Database Setup (MongoDB)

1. Install MongoDB Server
2. Start MongoDB service
3. The default connection string is `mongodb://localhost:27017/YourDatabaseName`
4. Update `appsettings.json` if your MongoDB is on a different host/port";
    }

    private string GetEfCoreSection(string solutionName)
    {
        return $@"### Database Setup (SQL Server)

1. Update the connection string in `appsettings.json`
2. Run migrations:
   ```bash
   cd {solutionName}.EntityFrameworkCore
   dotnet ef database update
   ```
3. Or use the DbMigrator project if available";
    }

    private string GetTieredSection(string solutionName)
    {
        return $@"1. First, start the API host:
   ```bash
   cd {solutionName}.HttpApi.Host
   dotnet run
   ```

2. Then, in a separate terminal, start the Blazor app:
   ```bash
   cd {solutionName}.Blazor.WebApp
   dotnet run
   ```

3. Open https://localhost:44316 in your browser";
    }

    private string GetSingleSection(string solutionName)
    {
        return $@"1. Start the Blazor application:
   ```bash
   cd {solutionName}.Blazor.WebApp
   dotnet run
   ```

2. Open the URL shown in the terminal (typically https://localhost:5001)";
    }
}
