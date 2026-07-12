using SufiChain.SufiPlatform.CLI.ProjectBuilding;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

internal static class BlazorWebAppHostCleanup
{
    public static void RemoveApiHosting(ProjectBuildContext context)
    {
        CleanupWebAppProject(context, removeLocalAuthServer: false);
        CleanupWebAppModule(context, removeLocalAuthServer: false);
    }

    public static void RemoveApiAndLocalAuthServerHosting(ProjectBuildContext context)
    {
        CleanupWebAppProject(context, removeLocalAuthServer: true);
        CleanupWebAppModule(context, removeLocalAuthServer: true);
        RemoveLocalApplicationAndDatabaseHosting(context);
    }

    private static void CleanupWebAppProject(ProjectBuildContext context, bool removeLocalAuthServer)
    {
        var webAppCsprojFiles = context.Files.Keys
            .Where(file => file.EndsWith(".Blazor.WebApp.csproj", StringComparison.OrdinalIgnoreCase) &&
                           !file.Contains("Blazor.WebApp.Client", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in webAppCsprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            content = RemoveReferenceLines(content, ".HttpApi\\");
            content = RemoveReferenceLines(content, ".HttpApi\"");
            content = RemovePackageReferenceLines(content, ".HttpApi");
            content = RemovePackageReferenceLines(content, ".Swashbuckle");
            content = RemoveReferenceLines(content, "Sufi.Swashbuckle");
            content = RemoveCommentLinesContaining(content, "HttpApi");
            content = RemoveCommentLinesContaining(content, "Swagger");

            if (removeLocalAuthServer)
            {
                content = RemovePackageReferenceLines(content, "AspNetCore.Authentication.Server");
                content = RemoveReferenceLines(content, "AspNetCore.Authentication.Server");
                content = RemovePackageReferenceLines(content, "Account.Application");
                content = RemovePackageReferenceLines(content, "Account.HttpApi");
            }

            content = RemoveEmptyItemGroups(content);
            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    private static void CleanupWebAppModule(ProjectBuildContext context, bool removeLocalAuthServer)
    {
        var webAppModuleFiles = context.Files.Keys
            .Where(file => file.EndsWith("Module.cs", StringComparison.OrdinalIgnoreCase) &&
                           file.Contains("Blazor.WebApp", StringComparison.OrdinalIgnoreCase) &&
                           !file.Contains("Blazor.WebApp.Client", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in webAppModuleFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            content = RemoveUsingLines(content, ".HttpApi");
            content = RemoveUsingLines(content, ".Swashbuckle");
            content = RemoveUsingLines(content, "Microsoft.OpenApi");

            content = RemoveDependsOnLine(content, "HttpApiModule");
            content = RemoveDependsOnLine(content, "HttpApi");
            content = RemoveDependsOnLine(content, "SwashbuckleModule");

            content = RemoveMethodCall(content, "ConfigureConventionalControllers");
            content = RemoveMethodCall(content, "ConfigureSwaggerServices");
            content = RemoveMethod(content, "ConfigureConventionalControllers");
            content = RemoveMethod(content, "ConfigureSwaggerServices");

            content = RemoveSwaggerMiddleware(content);
            content = RemoveCommentLinesContaining(content, "HttpApi");
            content = RemoveCommentLinesContaining(content, "Swagger");

            if (removeLocalAuthServer)
            {
                content = RemoveUsingLines(content, "AspNetCore.Authentication.Server");
                content = RemoveDependsOnLine(content, "AuthenticationServerModule");
                content = RemoveDependsOnLine(content, "AccountApplicationModule");
                content = RemoveDependsOnLine(content, "AccountHttpApiModule");
            }

            content = NormalizeDependsOnCommas(content);
            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    private static void RemoveLocalApplicationAndDatabaseHosting(ProjectBuildContext context)
    {
        var webAppCsprojFiles = context.Files.Keys
            .Where(file => file.EndsWith(".Blazor.WebApp.csproj", StringComparison.OrdinalIgnoreCase) &&
                           !file.Contains("Blazor.WebApp.Client", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in webAppCsprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            content = RemoveReferenceLines(content, ".Application\\");
            content = RemoveReferenceLines(content, ".EntityFrameworkCore\\");
            content = RemoveReferenceLines(content, ".MongoDB\\");
            content = RemovePackageReferenceLines(content, "Identity.Application");
            content = RemovePackageReferenceLines(content, "Tenants.Application");
            content = RemovePackageReferenceLines(content, "FileManager.Application");
            content = RemovePackageReferenceLines(content, "AI.Application");
            content = RemovePackageReferenceLines(content, "Calendar.Application");
            content = RemovePackageReferenceLines(content, "ShortLinks.Application");
            content = RemovePackageReferenceLines(content, "Tags.Application");
            content = RemovePackageReferenceLines(content, "Menus.Application");
            content = RemovePackageReferenceLines(content, "AuditLogging.Application");
            content = RemovePackageReferenceLines(content, "BackgroundJobs.Application");
            content = RemovePackageReferenceLines(content, "Features.Application");
            content = RemovePackageReferenceLines(content, "Settings.Application");
            content = RemovePackageReferenceLines(content, "Account.Application");
            content = RemovePackageReferenceLines(content, "Permissions.Application");
            content = RemovePackageReferenceLines(content, "Localization.Application");
            content = RemovePackageReferenceLines(content, ".EntityFrameworkCore");
            content = RemovePackageReferenceLines(content, ".MongoDB");
            content = RemovePackageReferenceLines(content, "Microsoft.EntityFrameworkCore.Tools");

            content = RemoveEmptyItemGroups(content);
            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }

        var webAppModuleFiles = context.Files.Keys
            .Where(file => file.EndsWith("Module.cs", StringComparison.OrdinalIgnoreCase) &&
                           file.Contains("Blazor.WebApp", StringComparison.OrdinalIgnoreCase) &&
                           !file.Contains("Blazor.WebApp.Client", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in webAppModuleFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);

            content = RemoveUsingLines(content, ".EntityFrameworkCore");
            content = RemoveUsingLines(content, ".MongoDB");
            content = RemoveUsingLines(content, ".Data");

            content = RemoveDependsOnLine(content, "ApplicationModule");
            content = RemoveDependsOnLine(content, "EntityFrameworkCoreModule");
            content = RemoveDependsOnLine(content, "MongoDbModule");
            content = RemoveDependsOnLine(content, "MongoDBModule");

            content = Regex.Replace(
                content,
                @"^[ \t]*app\.UseAbpOpenIddictValidation\(\);[ \t]*(?:\r?\n)?",
                "",
                RegexOptions.Multiline);
            content = RemoveCommentLinesContaining(content, "database");

            content = NormalizeDependsOnCommas(content);
            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }

    private static string RemoveReferenceLines(string content, string marker)
    {
        var pattern = $@"^[ \t]*<ProjectReference\b[^\r\n]*{Regex.Escape(marker)}[^\r\n]*/>[ \t]*(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    private static string RemovePackageReferenceLines(string content, string marker)
    {
        var pattern = $@"^[ \t]*<PackageReference\b[^\r\n]*{Regex.Escape(marker)}[^\r\n]*/>[ \t]*(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    private static string RemoveUsingLines(string content, string marker)
    {
        var pattern = $@"^[ \t]*using\b[^\r\n]*{Regex.Escape(marker)}[^\r\n]*;[ \t]*(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    private static string RemoveDependsOnLine(string content, string marker)
    {
        var pattern = $@"^[ \t]*typeof\([^)\r\n]*{Regex.Escape(marker)}[^)\r\n]*\),?[ \t]*(?://[^\r\n]*)?(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }

    private static string RemoveMethodCall(string content, string methodName)
    {
        var pattern = $@"^[ \t]*{Regex.Escape(methodName)}\([^;\r\n]*\);[ \t]*(?://[^\r\n]*)?(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.Multiline);
    }

    private static string RemoveMethod(string content, string methodName)
    {
        var methodIndex = content.IndexOf($"private void {methodName}", StringComparison.Ordinal);
        if (methodIndex < 0)
        {
            return content;
        }

        var braceIndex = content.IndexOf('{', methodIndex);
        if (braceIndex < 0)
        {
            return content;
        }

        var depth = 0;
        for (var i = braceIndex; i < content.Length; i++)
        {
            if (content[i] == '{')
            {
                depth++;
            }
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    var end = i + 1;
                    while (end < content.Length && (content[end] == '\r' || content[end] == '\n'))
                    {
                        end++;
                    }

                    var start = methodIndex;
                    while (start > 0 && (content[start - 1] == ' ' || content[start - 1] == '\t'))
                    {
                        start--;
                    }

                    return content.Remove(start, end - start);
                }
            }
        }

        return content;
    }

    private static string RemoveSwaggerMiddleware(string content)
    {
        content = Regex.Replace(
            content,
            @"^[ \t]*app\.UseSwagger\(\);[ \t]*(?:\r?\n)?",
            "",
            RegexOptions.Multiline);

        return Regex.Replace(
            content,
            @"^[ \t]*app\.UseAbpSwaggerUI\(.*?^[ \t]*\}\);[ \t]*(?:\r?\n)?",
            "",
            RegexOptions.Singleline | RegexOptions.Multiline);
    }

    private static string NormalizeDependsOnCommas(string content)
    {
        content = Regex.Replace(content, @",(\s*\))", "$1");
        content = Regex.Replace(content, @"(\[DependsOn\(\s*)\n\s*,", "$1\n");
        return content;
    }

    private static string RemoveEmptyItemGroups(string content)
    {
        return Regex.Replace(
            content,
            @"[ \t]*<ItemGroup>\s*</ItemGroup>[ \t]*(?:\r?\n)?",
            "",
            RegexOptions.IgnoreCase);
    }

    private static string RemoveCommentLinesContaining(string content, string marker)
    {
        var pattern = $@"^[ \t]*(?://|<!--)[^\r\n]*{Regex.Escape(marker)}[^\r\n]*(?:-->)*[ \t]*(?:\r?\n)?";
        return Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    }
}
