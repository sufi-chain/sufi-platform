using System;
using System.Linq;
using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// Read models may say whether a key exists. They do not declare the key itself.
/// Create, update, and input types stay on the write path.
/// </summary>
public class WorkspaceReadDtoSecretContractTests
{
    private static readonly string[] SecretNames =
    [
        "ApiKey",
        "Secret",
        "Password",
        "AuthorizationBearerToken",
        "AccessToken",
        "RefreshToken",
        "ClientSecret",
        "EnvironmentJson",
        "Token"
    ];

    [Fact]
    public void Read_dtos_do_not_declare_secret_fields()
    {
        var offenders = typeof(WorkspaceDto).Assembly
            .GetTypes()
            .Where(type => type.IsClass && type.IsPublic && !type.IsAbstract)
            .Where(type => type.Namespace != null &&
                           type.Namespace.StartsWith("SufiChain.SufiPlatform.SufiAI", StringComparison.Ordinal))
            .Where(type => type.Name.EndsWith("Dto", StringComparison.Ordinal) ||
                           type.Name.EndsWith("Result", StringComparison.Ordinal))
            .Where(type => !type.Name.StartsWith("Create", StringComparison.Ordinal))
            .Where(type => !type.Name.StartsWith("Update", StringComparison.Ordinal))
            .Where(type => !type.Name.EndsWith("Input", StringComparison.Ordinal))
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => SecretNames.Contains(property.Name, StringComparer.Ordinal))
                .Select(property => type.FullName + "." + property.Name))
            .ToArray();

        offenders.ShouldBeEmpty();
    }
}
