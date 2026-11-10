using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Host-only iran-mcp <see cref="MCPServer"/> rows. Existing keys keep their Id.
/// Missing keys receive a deterministic per-environment Id from <see cref="IranMcpServerId"/>.
/// </summary>
public class IranMcpServerDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string EmbeddedResourceName = "SufiChain.SufiPlatform.SufiAI.Data.iran-mcp-servers.json";
    public const string EnvironmentConfigKey = "SufiAI:IranMcp:Environment";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] HostEnvironmentKeys =
    [
        "environment",
        "DOTNET_ENVIRONMENT",
        "ASPNETCORE_ENVIRONMENT"
    ];

    private readonly IMCPServerRepository _servers;
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IranMcpServerDataSeedContributor> _logger;

    public IranMcpServerDataSeedContributor(
        IMCPServerRepository servers,
        ICurrentTenant currentTenant,
        IConfiguration configuration,
        ILogger<IranMcpServerDataSeedContributor> logger)
    {
        _servers = servers;
        _currentTenant = currentTenant;
        _configuration = configuration;
        _logger = logger;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
        {
            return;
        }

        using (_currentTenant.Change(null))
        {
            var environment = ResolveEnvironment();
            foreach (var entry in LoadServers())
            {
                await SeedServerAsync(environment, entry);
            }
        }
    }

    public virtual string ResolveEnvironment()
    {
        var configured = _configuration[EnvironmentConfigKey]
            ?? _configuration[IranMcpSettings.Environment];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        // AbpApplicationFactory (the DbMigrator container) does not register
        // IHostEnvironment. The configuration copied from the generic host already
        // carries --environment, DOTNET_ENVIRONMENT, and ASPNETCORE_ENVIRONMENT.
        foreach (var key in HostEnvironmentKeys)
        {
            var value = _configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return Environments.Production;
    }

    public virtual IReadOnlyList<IranMcpServerEntry> LoadServers()
    {
        using var stream = typeof(IranMcpServerDataSeedContributor).Assembly
            .GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource '{EmbeddedResourceName}'.");
        var document = JsonSerializer.Deserialize<IranMcpServerFile>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Iran MCP server catalog is empty.");
        return document.Servers;
    }

    private async Task SeedServerAsync(string environment, IranMcpServerEntry entry)
    {
        var existing = await _servers.FindByKeyAsync(entry.Name);
        var endpoint = BuildEndpoint(entry.Name);
        if (existing != null)
        {
            // Keep the stored Id. Refresh transport settings only when needed.
            if (!string.Equals(existing.Endpoint, endpoint, StringComparison.OrdinalIgnoreCase)
                || existing.TransportType != MCPTransportType.HTTP
                || existing.BindingMode != MCPBindingMode.UserScoped)
            {
                existing.ApplyConfiguration(
                    MCPTransportType.HTTP,
                    MCPBindingMode.UserScoped,
                    endpoint,
                    command: null,
                    argumentsJson: null);
                await _servers.UpdateAsync(existing, autoSave: true);
            }

            _logger.LogDebug(
                "Iran MCP server '{Server}' kept host row {ServerId} in environment {Environment}.",
                entry.Name,
                existing.Id,
                IranMcpServerId.NormalizeEnvironment(environment));
            return;
        }

        var id = IranMcpServerId.Derive(environment, entry.Name);
        var server = new MCPServer(id, entry.Name, entry.Title, MCPTransportType.HTTP, tenantId: null);
        server.SetBindingMode(MCPBindingMode.UserScoped);
        server.ConfigureHttpEndpoint(endpoint);
        await _servers.InsertAsync(server, autoSave: true);

        _logger.LogInformation(
            "Iran MCP server '{Server}' seeded as host row {ServerId} for environment {Environment}.",
            entry.Name,
            id,
            IranMcpServerId.NormalizeEnvironment(environment));
    }

    public static string BuildEndpoint(string serverName)
        => $"http://{IranMcpServerId.NormalizeServerName(serverName)}:8080/mcp";

    public sealed class IranMcpServerFile
    {
        public List<IranMcpServerEntry> Servers { get; set; } = [];
    }

    public sealed class IranMcpServerEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
