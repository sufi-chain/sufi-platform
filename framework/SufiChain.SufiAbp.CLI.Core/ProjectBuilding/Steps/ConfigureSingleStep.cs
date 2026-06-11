using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the solution for single (non-tiered) architecture.
/// In single mode, Blazor.WebApp hosts both the UI and API directly with database access.
/// </summary>
public class ConfigureSingleStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring single architecture...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (context.Args.IsTiered)
            return Task.CompletedTask;

        // Single architecture merges API into Blazor host
        context.Symbols.Add("single");

        // Remove HttpApi.Host project - Blazor.WebApp will host the API
        var httpApiHostProject = $"{context.Args.SolutionName}.HttpApi.Host";
        context.ProjectsToRemove.Add(httpApiHostProject);

        // Remove HttpApi.Client since we don't need HTTP client in single mode
        var httpApiClientProject = $"{context.Args.SolutionName}.HttpApi.Client";
        context.ProjectsToRemove.Add(httpApiClientProject);

        // Update Blazor.WebApp csproj to include direct references
        UpdateBlazorWebAppCsproj(context);

        // Update the Blazor.WebApp module for single mode
        UpdateBlazorWebAppModule(context);

        // Update appsettings.json for single mode
        UpdateAppSettings(context);

        return Task.CompletedTask;
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

        // Add single-mode project references (Application, HttpApi, Database)
        var additionalRefs = $@"
    <!-- Single mode: Direct application and database access -->
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

        // Add single-mode packages (OpenIddict for auth server, AspNetCore.Mvc for API)
        var additionalPackages = @"
    <!-- Single mode: Auth server and API hosting -->
    <PackageReference Include=""SufiChain.SufiAbp.OpenIddict.AspNetCore"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.AspNetCore.Authentication.JwtBearer"" Version=""$(SufiVersion)"" />
    <PackageReference Include=""SufiChain.SufiAbp.Identity.AspNetCore"" Version=""$(SufiVersion)"" />";

        if (!content.Contains("SufiChain.SufiAbp.OpenIddict.AspNetCore"))
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

        // Add single-mode module dependencies
        var singleModeDeps = $@"
    typeof({projectName}ApplicationModule),
    typeof({projectName}HttpApiModule),
    typeof({dbModuleName}),
    typeof(SufiAbpOpenIddictAspNetCoreModule),
    typeof(SufiAbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(SufiAbpIdentityAspNetCoreModule),";

        // Insert after the first DependsOn opening
        if (!content.Contains($"{projectName}ApplicationModule"))
        {
            content = Regex.Replace(
                content,
                @"(\[DependsOn\(\s*)",
                $"$1{singleModeDeps}\n    "
            );
        }

        // Add using statements for single mode
        var singleModeUsings = $@"using {solutionName}.Application;
using {solutionName}.HttpApi;
using {solutionName}.{dbProvider};
using OpenIddict.Validation.AspNetCore;
using SufiChain.SufiAbp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc;
using SufiChain.SufiAbp.Identity.AspNetCore;
using SufiChain.SufiAbp.OpenIddict;
";

        // Add usings before namespace
        if (!content.Contains($"using {solutionName}.Application;"))
        {
            content = Regex.Replace(
                content,
                @"(namespace\s+)",
                $"{singleModeUsings}\n$1"
            );
        }

        // Remove OIDC configuration and replace with Identity auth
        content = RemoveOidcConfiguration(content);
        content = AddSingleModeAuthConfiguration(content, projectName);
        content = AddSingleModeServices(content, projectName);
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

    private string AddSingleModeAuthConfiguration(string content, string projectName)
    {
        var authConfig = $@"
    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {{
        // Single mode: Use cookie authentication with Identity
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {{
                options.Authority = configuration[""App:SelfUrl""];
                options.RequireHttpsMetadata = false;
                options.Audience = ""{projectName}"";
            }});
    }}

    private void ConfigureOpenIddict(ServiceConfigurationContext context)
    {{
        // Configure OpenIddict server for single mode
        PreConfigure<OpenIddictBuilder>(builder =>
        {{
            builder.AddValidation(options =>
            {{
                options.AddAudiences(""{projectName}"");
                options.UseLocalServer();
                options.UseAspNetCore();
            }});
        }});
    }}

    private void ConfigureConventionalControllers()
    {{
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {{
            options.ConventionalControllers.Create(typeof({projectName}ApplicationModule).Assembly);
        }});
    }}
";

        // Insert before the last closing brace of the class
        content = Regex.Replace(
            content,
            @"(\n\})\s*$",
            $"{authConfig}$1"
        );

        return content;
    }

    private string AddSingleModeServices(string content, string projectName)
    {
        // Add calls to single-mode configuration methods in ConfigureServices
        var singleModeServiceCalls = @"
        ConfigureAuthentication(context, configuration);
        ConfigureConventionalControllers();";

        // Insert after the first line of ConfigureServices (after getting configuration)
        content = Regex.Replace(
            content,
            @"(var configuration = context\.Services\.GetConfiguration\(\);)",
            $"$1\n{singleModeServiceCalls}"
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

            // Remove RemoteServices configuration - not needed in single mode
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
                var dbName = context.Args.CompanyName + context.Args.ProjectName;
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
