using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Encryption;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

/// <summary>
/// sufi-chain#557: EnvironmentJson is a secret input. Action auditing keeps every other field
/// and must not keep the plaintext. The encrypted extra property must survive a reload.
/// </summary>
public class McpEnvironmentSecretAuditTests : SufiAITestBase<McpEnvironmentAuditTestModule>
{
    [Fact]
    public async Task Audited_create_and_update_omit_the_environment_secret()
    {
        var store = GetRequiredService<CapturingAuditingStore>();
        var logs = GetRequiredService<CapturingLoggerProvider>();
        store.Clear();
        logs.Clear();
        var createSecret = "mcp-env-sentinel-" + Guid.NewGuid().ToString("N");
        var updateSecret = "mcp-env-sentinel-" + Guid.NewGuid().ToString("N");
        var service = GetRequiredService<IMCPServerAppService>();

        var created = await CallAsync(() => service.CreateAsync(new CreateMCPServerDto
        {
            Key = "env-audit",
            Name = "Env audit",
            TransportType = "STDIO",
            Command = "python3",
            ArgumentsJson = "[\"fixture.py\"]",
            BindingMode = "TenantLevel",
            EnvironmentJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PROVIDER_TOKEN"] = createSecret
            })
        }), createSecret, store, logs);

        created.Key.ShouldBe("env-audit");
        created.EnvironmentKeys.ShouldBe(new[] { "PROVIDER_TOKEN" });
        JsonSerializer.Serialize(created).ShouldNotContain(createSecret);

        store.Clear();
        logs.Clear();
        var updated = await CallAsync(() => service.UpdateAsync(created.Id, new UpdateMCPServerDto
        {
            Name = "Env audit renamed",
            TransportType = "STDIO",
            Command = "python3",
            ArgumentsJson = "[\"fixture.py\"]",
            EnvironmentJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PROVIDER_TOKEN"] = updateSecret
            })
        }), updateSecret, store, logs);

        updated.Name.ShouldBe("Env audit renamed");
        JsonSerializer.Serialize(updated).ShouldNotContain(updateSecret);
        await AssertReloadedSecretAsync(created.Id, updateSecret);
    }

    [Fact]
    public async Task Rejected_environment_value_does_not_echo_the_secret()
    {
        var store = GetRequiredService<CapturingAuditingStore>();
        var logs = GetRequiredService<CapturingLoggerProvider>();
        store.Clear();
        logs.Clear();
        var secret = "mcp-env-sentinel-" + Guid.NewGuid().ToString("N");
        var service = GetRequiredService<IMCPServerAppService>();

        var error = await Should.ThrowAsync<Exception>(() => service.CreateAsync(new CreateMCPServerDto
        {
            Key = "env-audit-bad",
            Name = "Env audit bad",
            TransportType = "STDIO",
            Command = "python3",
            EnvironmentJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["PROVIDER_TOKEN"] = secret + "\n"
            })
        }));

        AssertSecretIsAbsent(secret, error, store, logs);
        Parameters(store).ShouldContain("env-audit-bad");
    }

    private async Task<MCPServerDto> CallAsync(
        Func<Task<MCPServerDto>> call,
        string secret,
        CapturingAuditingStore store,
        CapturingLoggerProvider logs)
    {
        try
        {
            var result = await call();
            AssertSecretIsAbsent(secret, error: null, store, logs);
            Parameters(store).ShouldContain("python3");
            return result;
        }
        catch (Exception exception)
        {
            AssertSecretIsAbsent(secret, exception, store, logs);
            throw;
        }
    }

    private static void AssertSecretIsAbsent(
        string secret,
        Exception? error,
        CapturingAuditingStore store,
        CapturingLoggerProvider logs)
    {
        store.Logs.ShouldNotBeEmpty();
        var parameters = Parameters(store);
        parameters.ShouldNotBeNullOrWhiteSpace();
        parameters.ShouldNotContain(secret);
        error?.ToString().ShouldNotContain(secret);
        if (error is Volo.Abp.BusinessException business)
        {
            foreach (var value in business.Data.Values)
            {
                value?.ToString().ShouldNotContain(secret);
            }
        }

        foreach (var line in logs.Lines)
        {
            line.ShouldNotContain(secret);
        }
    }

    private static string Parameters(CapturingAuditingStore store)
    {
        return string.Join(
            "\n",
            store.Logs.SelectMany(log => log.Actions.Select(action => action.Parameters ?? string.Empty)));
    }

    private async Task AssertReloadedSecretAsync(Guid id, string secret)
    {
        var encryptor = GetRequiredService<IStringEncryptionService>();
        var reloaded = await WithUnitOfWorkAsync(() =>
            GetRequiredService<IRepository<MCPServer, Guid>>().GetAsync(id));
        var stored = reloaded.GetProperty<string>(McpServerEnvironment.PropertyName);
        stored.ShouldNotBeNullOrWhiteSpace();
        stored.ShouldNotContain(secret);
        McpServerEnvironment.Decrypt(reloaded, encryptor)["PROVIDER_TOKEN"].ShouldBe(secret);
    }
}

[DependsOn(typeof(SufiAIApplicationTestModule))]
public class McpEnvironmentAuditTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var store = new CapturingAuditingStore();
        var logs = new CapturingLoggerProvider();
        context.Services.AddSingleton(Substitute.For<IDataProtectionProvider>());
        context.Services.AddSingleton(store);
        context.Services.Replace(ServiceDescriptor.Singleton<IAuditingStore>(store));
        context.Services.AddSingleton(logs);
        context.Services.AddSingleton<ILoggerProvider>(logs);
    }
}

public sealed class CapturingAuditingStore : IAuditingStore
{
    private readonly List<AuditLogInfo> _logs = new();

    public IReadOnlyList<AuditLogInfo> Logs
    {
        get
        {
            lock (_logs)
            {
                return _logs.ToList();
            }
        }
    }

    public Task SaveAsync(AuditLogInfo auditInfo)
    {
        lock (_logs)
        {
            _logs.Add(auditInfo);
        }

        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_logs)
        {
            _logs.Clear();
        }
    }
}

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<string> _lines = new();

    public IReadOnlyCollection<string> Lines => _lines.ToArray();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(_lines);

    public void Clear()
    {
        while (_lines.TryDequeue(out _))
        {
        }
    }

    public void Dispose()
    {
    }

    private sealed class CapturingLogger : ILogger
    {
        private readonly ConcurrentQueue<string> _lines;

        public CapturingLogger(ConcurrentQueue<string> lines)
        {
            _lines = lines;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var text = formatter(state, exception);
            if (exception != null)
            {
                text = text + "\n" + exception;
            }

            _lines.Enqueue(text);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
