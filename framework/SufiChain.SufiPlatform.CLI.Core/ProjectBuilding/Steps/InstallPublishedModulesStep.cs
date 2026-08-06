using System.Text;
using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.Modules;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

public sealed class InstallPublishedModulesStep : ProjectBuildPipelineStep
{
    private static readonly IReadOnlyDictionary<ModuleIntegrationPoint, string> ProjectSuffixes =
        new Dictionary<ModuleIntegrationPoint, string>
        {
            [ModuleIntegrationPoint.DomainShared] = ".Domain.Shared.csproj",
            [ModuleIntegrationPoint.Domain] = ".Domain.csproj",
            [ModuleIntegrationPoint.ApplicationContracts] = ".Application.Contracts.csproj",
            [ModuleIntegrationPoint.Application] = ".Application.csproj",
            [ModuleIntegrationPoint.EntityFrameworkCore] = ".EntityFrameworkCore.csproj",
            [ModuleIntegrationPoint.MongoDB] = ".MongoDB.csproj",
            [ModuleIntegrationPoint.HttpApi] = ".HttpApi.csproj",
            [ModuleIntegrationPoint.HttpApiClient] = ".HttpApi.Client.csproj",
            [ModuleIntegrationPoint.BlazorWebApp] = ".Blazor.WebApp.csproj",
            [ModuleIntegrationPoint.BlazorWebSite] = ".Blazor.WebSite.csproj",
            [ModuleIntegrationPoint.DbMigrator] = ".DbMigrator.csproj"
        };

    public override string Description => "Installing published module packages...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var registry = new ModuleRegistry();
        var modules = registry.ResolveWithDependencies(context.Args.IncludedModules)
            .Where(module => module.Bindings.Length > 0)
            .ToArray();

        foreach (var binding in modules.SelectMany(module => module.Bindings))
        {
            if (binding.DatabaseProvider.HasValue &&
                binding.DatabaseProvider.Value != context.Args.DatabaseProvider)
            {
                continue;
            }

            if (binding.IntegrationPoint == ModuleIntegrationPoint.BlazorWebSite &&
                !context.Args.IncludeWebSite)
            {
                continue;
            }

            if (binding.IntegrationPoint == ModuleIntegrationPoint.HttpApiClient &&
                context.Args.SolutionKind == SolutionKind.WebApp)
            {
                continue;
            }

            var projectSuffix = ResolveProjectSuffix(context, binding);
            var projectPath = AddPackageReference(context, projectSuffix, binding);
            AddModuleDependency(context, projectPath, binding.ModuleType);
        }

        return Task.CompletedTask;
    }

    private static string ResolveProjectSuffix(ProjectBuildContext context, ModuleBinding binding)
    {
        if (binding.IntegrationPoint == ModuleIntegrationPoint.BackendHost)
        {
            return context.Args.SolutionKind == SolutionKind.WebApp
                ? ".Blazor.WebApp.csproj"
                : ".HttpApi.Host.csproj";
        }

        if (ProjectSuffixes.TryGetValue(binding.IntegrationPoint, out var projectSuffix))
        {
            return projectSuffix;
        }

        throw new InvalidOperationException(
            $"Cannot install package '{binding.PackageId}': unsupported integration point '{binding.IntegrationPoint}'.");
    }

    private static string AddPackageReference(
        ProjectBuildContext context,
        string projectSuffix,
        ModuleBinding binding)
    {
        var projectPath = context.Files.Keys.SingleOrDefault(path =>
            path.EndsWith(projectSuffix, StringComparison.OrdinalIgnoreCase));
        if (projectPath == null)
        {
            throw new InvalidOperationException(
                $"Cannot install package '{binding.PackageId}': no generated project matches '{projectSuffix}'.");
        }

        var content = Encoding.UTF8.GetString(context.Files[projectPath]);
        if (content.Contains($"Include=\"{binding.PackageId}\"", StringComparison.OrdinalIgnoreCase))
        {
            return projectPath;
        }

        if (!content.Contains("</Project>", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Cannot install package '{binding.PackageId}': project '{projectPath}' has no closing Project element.");
        }

        var itemGroup =
            $"\n  <ItemGroup>\n" +
            $"    <PackageReference Include=\"{binding.PackageId}\" Version=\"$({binding.VersionProperty})\" />\n" +
            "  </ItemGroup>\n";
        content = content.Replace("</Project>", $"{itemGroup}</Project>", StringComparison.Ordinal);
        context.Files[projectPath] = Encoding.UTF8.GetBytes(content);
        return projectPath;
    }

    private static void AddModuleDependency(
        ProjectBuildContext context,
        string projectPath,
        string moduleType)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)?.Replace('\\', '/');
        var modulePath = context.Files.Keys
            .Where(path => path.EndsWith("Module.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => path.Replace('\\', '/').StartsWith($"{projectDirectory}/", StringComparison.OrdinalIgnoreCase))
            .Select(path => new
            {
                Path = path,
                Content = Encoding.UTF8.GetString(context.Files[path])
            })
            .FirstOrDefault(file => file.Content.Contains("[DependsOn(", StringComparison.Ordinal))
            ?.Path;
        if (modulePath == null)
        {
            throw new InvalidOperationException(
                $"Cannot install module '{moduleType}': project '{projectPath}' has no module class with DependsOn.");
        }

        var content = Encoding.UTF8.GetString(context.Files[modulePath]);
        if (content.Contains($"typeof({moduleType})", StringComparison.Ordinal))
        {
            return;
        }

        var classIndex = content.IndexOf("public class ", StringComparison.Ordinal);
        if (classIndex < 0)
        {
            throw new InvalidOperationException(
                $"Cannot install module '{moduleType}': module class was not found in '{modulePath}'.");
        }

        var dependsOnEnd = content.LastIndexOf(")]", classIndex, StringComparison.Ordinal);
        if (dependsOnEnd < 0)
        {
            throw new InvalidOperationException(
                $"Cannot install module '{moduleType}': DependsOn attribute is malformed in '{modulePath}'.");
        }

        var before = content[..dependsOnEnd].TrimEnd();
        var separator = before.EndsWith("(", StringComparison.Ordinal) ? string.Empty : ",";
        content = $"{before}{separator}\n        typeof({moduleType})\n{content[dependsOnEnd..]}";
        context.Files[modulePath] = Encoding.UTF8.GetBytes(content);
    }
}
