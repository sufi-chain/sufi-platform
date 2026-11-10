using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Data;

public class IranMcpServerDataSeedContributorTests
{
    [Fact]
    public void Explicit_config_wins_over_host_environment_names()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            [IranMcpServerDataSeedContributor.EnvironmentConfigKey] = "Custom",
            [IranMcpSettings.Environment] = "Settings",
            ["environment"] = "CommandLine",
            ["DOTNET_ENVIRONMENT"] = "Staging",
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        });

        contributor.ResolveEnvironment().ShouldBe("Custom");
    }

    [Fact]
    public void Settings_key_wins_when_the_colon_config_key_is_absent()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            [IranMcpSettings.Environment] = "Settings",
            ["DOTNET_ENVIRONMENT"] = "Staging"
        });

        contributor.ResolveEnvironment().ShouldBe("Settings");
    }

    [Fact]
    public void Command_line_environment_wins_over_environment_variables()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            ["environment"] = "Staging",
            ["DOTNET_ENVIRONMENT"] = "Production",
            ["ASPNETCORE_ENVIRONMENT"] = "Development"
        });

        contributor.ResolveEnvironment().ShouldBe("Staging");
    }

    [Fact]
    public void Dotnet_environment_wins_over_aspnetcore_environment()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            ["DOTNET_ENVIRONMENT"] = "Staging",
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        });

        var environment = contributor.ResolveEnvironment();

        environment.ShouldBe("Staging");
        IranMcpServerId.Derive(environment, "demo")
            .ShouldNotBe(IranMcpServerId.Derive("Production", "demo"));
    }

    [Fact]
    public void Aspnetcore_environment_is_used_when_dotnet_environment_is_absent()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Staging"
        });

        contributor.ResolveEnvironment().ShouldBe("Staging");
    }

    [Fact]
    public void Missing_environment_falls_back_to_production()
    {
        var contributor = Create(new Dictionary<string, string?>
        {
            ["DOTNET_ENVIRONMENT"] = " ",
            ["ASPNETCORE_ENVIRONMENT"] = ""
        });

        contributor.ResolveEnvironment().ShouldBe("Production");
    }

    private static IranMcpServerDataSeedContributor Create(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        return new IranMcpServerDataSeedContributor(
            Substitute.For<IMCPServerRepository>(),
            Substitute.For<ICurrentTenant>(),
            configuration,
            NullLogger<IranMcpServerDataSeedContributor>.Instance);
    }
}
