using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the solution for WebApp (non-tiered) architecture.
/// In WebApp mode, Blazor.WebApp hosts both the UI and API directly with database access.
/// </summary>
public class ConfigureSingleStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring WebApp architecture...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (context.Args.IsTiered)
            return Task.CompletedTask;

        // WebApp architecture merges API into Blazor host
        context.Symbols.Add("webapp");
        context.Symbols.Add("arch:webapp");
        context.Symbols.Add("single");
        context.Symbols.Add("arch:single");

        // Remove HttpApi.Host project - Blazor.WebApp will host the API
        var httpApiHostProject = $"{context.Args.SolutionName}.HttpApi.Host";
        context.ProjectsToRemove.Add(httpApiHostProject);

        // Remove HttpApi.Client since we don't need HTTP client in WebApp mode
        var httpApiClientProject = $"{context.Args.SolutionName}.HttpApi.Client";
        context.ProjectsToRemove.Add(httpApiClientProject);

        // Update Blazor.WebApp csproj to include direct references
        UpdateBlazorWebAppCsproj(context);

        // Update the Blazor.WebApp module for WebApp mode
        UpdateBlazorWebAppModule(context);

        // Ensure the WebAssembly client does not keep references to the removed HttpApi.Client project.
        UpdateBlazorWebAppClientForSingle(context);

        // Update appsettings.json for WebApp mode
        UpdateAppSettings(context);

        return Task.CompletedTask;
    }

    private void UpdateBlazorWebAppClientForSingle(ProjectBuildContext context)
    {
        var clientModulePath = context.Files.Keys
            .FirstOrDefault(f => f.Contains("Blazor.WebApp.Client") && f.EndsWith("Module.cs"));

        if (clientModulePath != null)
        {
            var content = Encoding.UTF8.GetString(context.Files[clientModulePath]);
            content = Regex.Replace(
                content,
                @"\s*,?\s*typeof\([^)]*HttpApiClientModule\)",
                "",
                RegexOptions.IgnoreCase);
            context.Files[clientModulePath] = Encoding.UTF8.GetBytes(content);
        }

        var clientCsprojPath = context.Files.Keys
            .FirstOrDefault(f => f.Contains("Blazor.WebApp.Client") && f.EndsWith(".csproj"));

        if (clientCsprojPath != null)
        {
            var content = Encoding.UTF8.GetString(context.Files[clientCsprojPath]);
            content = Regex.Replace(
                content,
                @"\s*<ProjectReference[^>]*HttpApi\.Client[^>]*/>\s*",
                "\n",
                RegexOptions.IgnoreCase);
            context.Files[clientCsprojPath] = Encoding.UTF8.GetBytes(content);
        }
    }

    private void UpdateBlazorWebAppCsproj(ProjectBuildContext context)
    {
        var solutionName = context.Args.SolutionName;
        var dbProvider = context.Args.DatabaseProvider == DatabaseProvider.EntityFrameworkCore
            ? "EntityFrameworkCore"
            : "MongoDB";

        var blazorCsprojPath = context.Files.Keys
            .FirstOrDefault(f => f.Contains("Blazor.WebApp") && f.EndsWith(".csproj") && !f.Contains("Client"));

        if (blazorCsprojPath == null)
            return;

        var content = Encoding.UTF8.GetString(context.Files[blazorCsprojPath]);

        // Remove HttpApi.Client reference
        content = Regex.Replace(
            content,
            $@"\s*<ProjectReference[^>]*HttpApi\.Client[^>]*/>\s*",
            "\n",
            RegexOptions.IgnoreCase
        );

        // Remove tiered-only packages
        content = Regex.Replace(
            content,
            @"\s*<PackageReference[^>]*Volo\.Abp\.AspNetCore\.Mvc\.Client[^>]*/>\s*",
            "\n"
        );
        content = Regex.Replace(
            content,
            @"\s*<PackageReference[^>]*Volo\.Abp\.AspNetCore\.Authentication\.OpenIdConnect[^>]*/>\s*",
            "\n"
        );
        content = Regex.Replace(
            content,
            @"\s*<PackageReference[^>]*Volo\.Abp\.Http\.Client\.IdentityModel\.Web[^>]*/>\s*",
            "\n"
        );

        // Add WebApp-mode project references (Application, HttpApi, Database)
        var additionalRefs = $@"
    <!-- WebApp mode: Direct application and database access -->
    <ProjectReference Include=""..\{solutionName}.Application\{solutionName}.Application.csproj"" />
    <ProjectReference Include=""..\{solutionName}.HttpApi\{solutionName}.HttpApi.csproj"" />
    <ProjectReference Include=""..\{solutionName}.{dbProvider}\{solutionName}.{dbProvider}.csproj"" />";

        // Find the ItemGroup with ProjectReferences and add our references
        if (!content.Contains($"{solutionName}.Application.csproj"))
        {
            // Insert before the first </ItemGroup> that contains ProjectReference
            content = Regex.Replace(
                content,
                @"(<ItemGroup>\s*(?:<ProjectReference[^>]+>\s*)+)(</ItemGroup>)",
                $"$1{additionalRefs}\n  $2",
                RegexOptions.Singleline
            );
        }

        // Add WebApp-mode packages (OpenIddict for auth server, AspNetCore.Mvc for API)
        var additionalPackages = @"
	    <!-- WebApp mode: Auth server and API hosting -->
	    <PackageReference Include=""SufiChain.SufiPlatform.OpenIddict.AspNetCore"" Version=""$(SufiVersion)"" />
	    <PackageReference Include=""SufiChain.SufiPlatform.Identity.AspNetCore"" Version=""$(SufiVersion)"" />";

        if (!content.Contains("SufiChain.SufiPlatform.OpenIddict.AspNetCore"))
        {
            // Insert before the first </ItemGroup> that contains PackageReference
            content = Regex.Replace(
                content,
                @"(<ItemGroup>\s*(?:<PackageReference[^>]+>\s*)+)(</ItemGroup>)",
                $"$1{additionalPackages}\n  $2",
                RegexOptions.Singleline
            );
        }

        context.Files[blazorCsprojPath] = Encoding.UTF8.GetBytes(content);
    }

    private void UpdateBlazorWebAppModule(ProjectBuildContext context)
    {
        var solutionName = context.Args.SolutionName;
        var projectName = context.Args.ProjectName;
        var dbProvider = context.Args.DatabaseProvider == DatabaseProvider.EntityFrameworkCore
            ? "EntityFrameworkCore"
            : "MongoDB";
        var dbModuleName = dbProvider == "EntityFrameworkCore"
            ? $"{projectName}EntityFrameworkCoreModule"
            : $"{projectName}MongoDbModule";

        var blazorModulePath = context.Files.Keys
            .FirstOrDefault(f => f.Contains("Blazor.WebApp") && f.EndsWith("Module.cs") && !f.Contains("Client"));

        if (blazorModulePath == null)
            return;

        var content = Encoding.UTF8.GetString(context.Files[blazorModulePath]);

        // Remove tiered-only module dependencies
        content = Regex.Replace(content, @",?\s*typeof\([^)]*HttpApiClientModule\)", "");
        content = Regex.Replace(content, @",?\s*typeof\(AbpAspNetCoreMvcClientModule\)", "");
        content = Regex.Replace(content, @",?\s*typeof\(AbpAspNetCoreAuthenticationOpenIdConnectModule\)", "");
        content = Regex.Replace(content, @",?\s*typeof\(AbpHttpClientIdentityModelWebModule\)", "");

        // Add WebApp-mode module dependencies only when the template does not already carry them.
        var singleModeDeps = $@"
    typeof({projectName}ApplicationModule),
    typeof({projectName}HttpApiModule),
    typeof({dbModuleName}),";

        // Insert after the first DependsOn opening
        if (!content.Contains($"{projectName}ApplicationModule"))
        {
            content = Regex.Replace(
                content,
                @"(\[DependsOn\(\s*)",
                $"$1{singleModeDeps}\n    "
            );
        }

        // Add using statements for WebApp mode
        var singleModeUsings = $@"using {solutionName}.{dbProvider};
		";

        // Add usings before namespace
        if (!content.Contains($"using {solutionName}.{dbProvider};"))
        {
            content = Regex.Replace(
                content,
                @"(namespace\s+)",
                $"{singleModeUsings}\n$1"
            );
        }

        // The unified WebApp template already configures authentication and controllers.
        content = RemoveOidcConfiguration(content);
        content = UpdateOnApplicationInitialization(content);

        context.Files[blazorModulePath] = Encoding.UTF8.GetBytes(content);
    }

    private string RemoveOidcConfiguration(string content)
    {
        // Remove the entire ConfigureAuthentication method that uses OIDC
        content = Regex.Replace(
            content,
            @"private void ConfigureAuthentication\(ServiceConfigurationContext context, IConfiguration configuration\)\s*\{[^}]*\.AddAbpOpenIdConnect[^}]*\}[^}]*\}",
            "",
            RegexOptions.Singleline
        );

        // Remove the call to ConfigureAuthentication(context, configuration)
        content = Regex.Replace(
            content,
            @"\s*ConfigureAuthentication\(context,\s*configuration\);\s*",
            "\n"
        );

        // Remove OIDC using statement
        content = Regex.Replace(
            content,
            @"using Microsoft\.IdentityModel\.Protocols\.OpenIdConnect;\s*\n?",
            ""
        );

        return content;
    }

    private string UpdateOnApplicationInitialization(string content)
    {
        // Add OpenIddict validation middleware
        if (!content.Contains("UseAbpOpenIddictValidation"))
        {
            content = Regex.Replace(
                content,
                @"(app\.UseAuthentication\(\);)",
                "$1\n            app.UseAbpOpenIddictValidation();\n            app.UseJwtTokenMiddleware();"
            );
        }

        // Add CORS if not present
        if (!content.Contains("app.UseCors"))
        {
            content = Regex.Replace(
                content,
                @"(app\.UseRouting\(\);)",
                "$1\n            app.UseCors();"
            );
        }

        return content;
    }

    private void UpdateAppSettings(ProjectBuildContext context)
    {
        var blazorAppSettings = context.Files.Keys
            .Where(f => f.Contains("Blazor.WebApp") &&
                       !f.Contains("Client") &&
                       f.EndsWith("appsettings.json"))
            .ToList();

        foreach (var file in blazorAppSettings)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            // Remove RemoteServices configuration - not needed in WebApp mode
            content = Regex.Replace(
                content,
                @"\s*""RemoteServices"":\s*\{[^}]+\},?",
                "",
                RegexOptions.Singleline
            );

            // Remove AuthServer configuration pointing to external server
            content = Regex.Replace(
                content,
                @"\s*""AuthServer"":\s*\{[^}]+\},?",
                "",
                RegexOptions.Singleline
            );

            // Add ConnectionStrings if not present (for database access)
            if (!content.Contains("ConnectionStrings"))
            {
                var dbProvider = context.Args.DatabaseProvider;
                var dbName = context.Args.ProjectName;
                var connString = dbProvider == DatabaseProvider.MongoDB
                    ? $"mongodb://localhost:27017/{dbName}"
                    : $"Server=(localdb)\\\\MSSQLLocalDB;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True";

                content = Regex.Replace(
                    content,
                    @"(\{)",
                    $"$1\n  \"ConnectionStrings\": {{\n    \"Default\": \"{connString}\"\n  }},"
                );
            }

            // Clean up trailing commas and extra whitespace
            content = Regex.Replace(content, @",(\s*[}\]])", "$1");
            content = Regex.Replace(content, @"\n\s*\n\s*\n", "\n\n");

            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }
}
